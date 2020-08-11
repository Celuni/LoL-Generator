using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace LoL_Generator
{

    class Generator
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            //string clientpath = Path.GetDirectoryName(GetProcessFilename(Process.GetProcessesByName("LeagueClient").FirstOrDefault()));

            // Create event query to be notified within 1 second of
            // a change in a service
            WqlEventQuery query =
                new WqlEventQuery("__InstanceCreationEvent",
                new TimeSpan(0, 0, 1),
                "TargetInstance isa \"Win32_Process\"");

            // Initialize an event watcher and subscribe to events
            // that match this query
            ManagementEventWatcher watcher =
                new ManagementEventWatcher();
            watcher.Query = query;


            // Block until the next event occurs
            // Note: this can be done in a loop if waiting for
            //        more than one occurrence
            Console.WriteLine(
                "Open an application (notepad.exe) to trigger an event.");
            ManagementBaseObject e = watcher.WaitForNextEvent();

            //Display information from the event
            Console.WriteLine(
                "Process {0} has been created, path is: {1}",
                ((ManagementBaseObject)e
                ["TargetInstance"])["Name"],
                ((ManagementBaseObject)e
                ["TargetInstance"])["ExecutablePath"]);




            /*string lockfile = "";
            using (FileStream fs = File.Open(clientpath + @"\lockfile", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] buf = new byte[1024];
                int c;

                while ((c = fs.Read(buf, 0, buf.Length)) > 0)
                {
                    lockfile = Encoding.UTF8.GetString(buf, 0, c);
                }
            }

            SessionInfo.port = lockfile.Split(':')[2];
            SessionInfo.password = lockfile.Split(':')[3];
            SessionInfo.encoding = Encoding.ASCII.GetBytes($"riot:{SessionInfo.password}");

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (requestMessage, certificate, chain, policyErrors) => true;

            using (HttpClient httpClient = new HttpClient(handler))
            {
                await sendRequestAsync(httpClient, "GET", $"https://127.0.0.1:{SessionInfo.port}/swagger/v3/openapi.json", null);

                string summonerJson = await sendRequestAsync(httpClient, "GET", $"https://127.0.0.1:{SessionInfo.port}/lol-summoner/v1/current-summoner", null);

                SummonerInfo summonerJsonObject = JsonConvert.DeserializeObject<SummonerInfo>(summonerJson);
                long summonerId = summonerJsonObject.summonerId;

                string gameJson = await sendRequestAsync(httpClient, "GET", $"https://127.0.0.1:{SessionInfo.port}/lol-champ-select/v1/session", null);
                GameSession gameJsonObject = JsonConvert.DeserializeObject<GameSession>(gameJson);

                int championId = gameJsonObject.myTeam.FirstOrDefault(x => x.summonerId == summonerId).championId;

                string championJson = await sendRequestAsync(httpClient, "GET", $"http://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default/v1/champions/{championId}.json", null);
                ChampionInfo championJsonObject = JsonConvert.DeserializeObject<ChampionInfo>(championJson);

                string champion = championJsonObject.name;

                HtmlDocument htmlDoc = new HtmlWeb().Load($"https://na.op.gg/champion/{champion}/statistics/");
                string xpath = $"//ul[@class='champion-stats-position']//li";

                foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes(xpath))
                {
                    string role = node.GetAttributeValue("data-position", "nothing");

                    RunePage runePage = new RunePage(champion, role);

                    string runeJson = JsonConvert.SerializeObject(runePage, Formatting.Indented);

                    await sendRequestAsync(httpClient, "POST", $"https://127.0.0.1:{SessionInfo.port}/lol-perks/v1/pages", runeJson);
                }
            }

            /*string json = JsonConvert.SerializeObject(new ItemSet("Anivia", "Mid"), Formatting.Indented);
            using (var tw = new StreamWriter(@"C:\Users\tavinc\Documents\test.json", false))
            {
                tw.WriteLine(json);
                tw.Close();
            }
            Console.WriteLine(json);*/
        }

        static async System.Threading.Tasks.Task<string> sendRequestAsync(HttpClient httpClient, string method, string url, string json)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), url))
            {
                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.TryAddWithoutValidation("Authorization", "Basic " + Convert.ToBase64String(SessionInfo.encoding));

                if (method == "POST")
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

        public class SummonerInfo
        {
            public long summonerId { get; set; }
        }

        public class GameSession
        {
            public List<MyTeam> myTeam { get; set; }
        }

        public class MyTeam
        {
            public int championId { get; set; }
            public int championPickIntent { get; set; }
            public long summonerId { get; set; }
        }

        public class ChampionInfo
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        static class SessionInfo
        {
            public static string port;
            public static string password;
            public static byte[] encoding;
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

            IntPtr ptr;
            try
            {
                ptr = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, p.Id);
            }
            catch (Exception)
            {
                return null;
            }

            if (!QueryFullProcessImageName(ptr, 0, builder, ref capacity))
            {
                return String.Empty;
            }

            return builder.ToString();
        }
    }

}    