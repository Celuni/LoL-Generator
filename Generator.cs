using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace LoL_Generator
{
    class Generator
    {
        static Timer timer;

        static string lockfileloc;

        static string port;
        static string password;
        static byte[] encoding;
        static long summonerId;

        static bool champLocked;

        static HttpClient httpClient;
        static HttpClientHandler handler;

        static void Main(string[] args)
        {
            StartTimer(3000, InitiatePolling);

            /*string json = JsonConvert.SerializeObject(new ItemSet("Anivia", "Mid"), Formatting.Indented);
            using (var tw = new StreamWriter(@"C:\Users\tavinc\Documents\test.json", false))
            {
                tw.WriteLine(json);
                tw.Close();
            }
            Console.WriteLine(json);*/

        }

        static void StartTimer(int interval, Action<object, EventArgs> function)
        {
            timer = new Timer(interval);
            timer.Elapsed += new ElapsedEventHandler(function);
            timer.AutoReset = true;

            timer.Enabled = true;
            Console.ReadLine();
        }

        static bool CheckClientIsOpen()
        {
            return Process.GetProcessesByName("LeagueClient").FirstOrDefault() != null;
        }

        static void InitiatePolling(object source, EventArgs e)
        {
            Console.WriteLine("Waiting for LeagueClient.exe to start...");

            if (CheckClientIsOpen())
            {
                Console.WriteLine("LeagueClient.exe has been opened");
                timer.Stop();

                string clientpath = Path.GetDirectoryName(GetProcessFilename(Process.GetProcessesByName("LeagueClient").FirstOrDefault()));
                lockfileloc = clientpath + @"\lockfile";

                StartTimer(500, CheckLockFileExists);
            }
        }

        static void CheckLockFileExists(object source, EventArgs e)
        {
            Console.WriteLine("Waiting for lockfile to be created...");

            if (CheckClientIsOpen())
            {
                if (File.Exists(lockfileloc))
                {
                    Console.WriteLine("lockfile has been created");
                    timer.Stop();

                    string lockfile = "";
                    using (FileStream fs = File.Open(lockfileloc, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        byte[] buf = new byte[1024];
                        int c;

                        while ((c = fs.Read(buf, 0, buf.Length)) > 0)
                        {
                            lockfile = Encoding.UTF8.GetString(buf, 0, c);
                        }
                    }

                    port = lockfile.Split(':')[2];
                    password = lockfile.Split(':')[3];
                    encoding = Encoding.ASCII.GetBytes($"riot:{password}");

                    handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback = (requestMessage, certificate, chain, policyErrors) => true;
                    httpClient = new HttpClient(handler);

                    StartTimer(500, CheckInChampSelect);
                }
            }
            else
            {
                Console.WriteLine("LeagueClient.exe has stopped.");
                timer.Stop();

                StartTimer(3000, InitiatePolling);
            }
        }

        static async void CheckInChampSelect(object source, EventArgs e)
        {
            if (CheckClientIsOpen())
            {
                try
                {
                    string gamephase = await SendRequestAsync("GET", $"https://127.0.0.1:{port}/lol-gameflow/v1/gameflow-phase", null);

                    if (gamephase == "\"ChampSelect\"" && !champLocked)
                    {
                        Console.WriteLine("In Champion Select");

                        if (summonerId == default)
                        {
                            string summonerJson = await SendRequestAsync("GET", $"https://127.0.0.1:{port}/lol-summoner/v1/current-summoner", null);
                            SummonerInfo summonerJsonObject = JsonConvert.DeserializeObject<SummonerInfo>(summonerJson);
                            summonerId = summonerJsonObject.summonerId;
                        }

                        string championId = await SendRequestAsync("GET", $"https://127.0.0.1:{port}/lol-champ-select/v1/current-champion", null);

                        if (championId != "0" && !champLocked)
                        {
                            Console.WriteLine("A Champion has been locked in, generating rune page(s) and item set(s)...");
                            champLocked = true;

                            string championJson = await SendRequestAsync("GET", $"http://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default/v1/champions/{championId}.json", null);
                            ChampionInfo championJsonObject = JsonConvert.DeserializeObject<ChampionInfo>(championJson);

                            string champion = championJsonObject.name;

                            HtmlDocument htmlDoc = new HtmlWeb().Load($"https://na.op.gg/champion/{champion}/statistics/");
                            string xpath = $"//ul[@class='champion-stats-position']//li";

                            string runePagesJson = await SendRequestAsync("GET", $"https://127.0.0.1:{port}/lol-perks/v1/pages", null);
                            List<RunePageInfo> runePageObject = JsonConvert.DeserializeObject<List<RunePageInfo>>(runePagesJson);

                            foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes(xpath))
                            {
                                string role = node.GetAttributeValue("data-position", "nothing");

                                RunePage runePage = new RunePage(champion, role);

                                string runeJson = JsonConvert.SerializeObject(runePage, Formatting.Indented);

                                if (runePageObject.FirstOrDefault(x => x.name == champion + " " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.ToLower())) != null)
                                {
                                    Console.WriteLine("Modifying rune page for " + champion + " " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.ToLower()));

                                    int id = runePageObject.FirstOrDefault(x => x.name == champion + " " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.ToLower())).id;

                                    await SendRequestAsync("PUT", $"https://127.0.0.1:{port}/lol-perks/v1/pages/{id}", runeJson);
                                }
                                else
                                {
                                    await SendRequestAsync("POST", $"https://127.0.0.1:{port}/lol-perks/v1/pages/", runeJson);
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Waiting for champion select to start/restart...");

                        champLocked = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("LeagueClient.exe has stopped.");
                timer.Stop();

                StartTimer(3000, InitiatePolling);
            }
        }
                        
            static async Task<string> SendRequestAsync(string method, string url, string json)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), url))
            {
                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.TryAddWithoutValidation("Authorization", "Basic " + Convert.ToBase64String(encoding));

                if (method == "POST" || method == "PUT")
                {
                    request.Content = new StringContent(json);
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                }

                HttpResponseMessage response = await httpClient.SendAsync(request);

                if (method == "GET")
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }

            return null;
        }

        public class Error
        {
            public string errorCode;
            public int httpStatus;
        }

        public class SummonerInfo
        {
            public long summonerId { get; set; }
        }
                
        public class ChampionInfo
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class RunePageInfo
        {
            public int id;
            public string name;
        }

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            QueryLimitedInformation = 0x00001000
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryFullProcessImageName(
              [In] IntPtr hProcess,
              [In] int dwFlags,
              [Out] StringBuilder lpExeName,
              ref int lpdwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
         ProcessAccessFlags processAccess,
         bool bInheritHandle,
         int processId);

        static String GetProcessFilename(Process p)
        {
            int capacity = 2000;
            StringBuilder builder = new StringBuilder(capacity);

            IntPtr ptr = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, p.Id);

            if (!QueryFullProcessImageName(ptr, 0, builder, ref capacity))
            {
                return String.Empty;
            }

            return builder.ToString();
        }
    }

}    