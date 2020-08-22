using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;


namespace LoL_Generator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;
        static MainWindow window;

        static string lockfileloc;

        static string port;
        static string password;
        static byte[] encoding;

        static long summonerId;

        static bool champLocked;
        static string champion;

        static HttpClient httpClient;
        static HttpClientHandler handler;

        static CancellationTokenSource tokenSource;

        public object FilenameUtils { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            window = new MainWindow();
            window.Show();

            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            notifyIcon = (TaskbarIcon)FindResource("MyNotifyIcon");

            tokenSource = new CancellationTokenSource();
            StartNewTask(InitiatePolling, TimeSpan.FromSeconds(3), tokenSource.Token);

            Console.WriteLine("Waiting for LeagueClient.exe to start...");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }

        bool CheckClientIsOpen()
        {
            if (Process.GetProcessesByName("LeagueClient").FirstOrDefault() == null)
            {
                Console.WriteLine("LeagueClient.exe has stopped.");
                tokenSource.Cancel();

                Action act = () => { window.ReadyText.Text = "Not Ready"; window.ReadyIcon.Source = new BitmapImage(new Uri(@"/images/offline.png", UriKind.Relative)); };
                window.Dispatcher.Invoke(act);

                tokenSource = new CancellationTokenSource();
                StartNewTask(InitiatePolling, TimeSpan.FromSeconds(3), tokenSource.Token);

                return false;
            }

            return true;
        }

        void InitiatePolling()
        {
            if (Process.GetProcessesByName("LeagueClient").FirstOrDefault() != null)
            {
                Console.WriteLine("LeagueClient.exe has been opened");
                tokenSource.Cancel();

                string clientpath = System.IO.Path.GetDirectoryName(GetProcessFilename(Process.GetProcessesByName("LeagueClient").FirstOrDefault()));
                lockfileloc = clientpath + @"\lockfile";

                tokenSource = new CancellationTokenSource();
                StartNewTask(CheckLockFileExists, TimeSpan.FromSeconds(0.5), tokenSource.Token);

                Console.WriteLine("Waiting for lockfile to be created...");
            }
        }

        async void CheckLockFileExists()
        {
            if (CheckClientIsOpen())
            {
                if (File.Exists(lockfileloc))
                {
                    Console.WriteLine("lockfile has been created");
                    tokenSource.Cancel();

                    Action act = () => { 
                        window.ReadyText.Text = "Ready"; 
                        window.ReadyIcon.Source = new BitmapImage(new Uri(@"/images/online.png", UriKind.Relative)); 
                    };

                    window.Dispatcher.Invoke(act);

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

                    try
                    {
                        string summonerJson = await SendRequestAsync("GET", $"https://127.0.0.1:{port}/lol-summoner/v1/current-summoner", null);
                        SummonerInfo summonerJsonObject = JsonConvert.DeserializeObject<SummonerInfo>(summonerJson);

                        summonerId = summonerJsonObject.summonerId;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }

                    tokenSource = new CancellationTokenSource();
                    StartNewTask(CheckInChampSelect, TimeSpan.FromSeconds(0.5), tokenSource.Token);
                }
            }
        }

        async void CheckInChampSelect()
        {
            if (CheckClientIsOpen())
            {
                try
                {
                    string gamephase = await SendRequestAsync("GET", $"https://127.0.0.1:{port}/lol-gameflow/v1/gameflow-phase", null);

                    if (gamephase == "\"ChampSelect\"" && !champLocked)
                    {
                        //Console.WriteLine("In Champion Select");

                        string championHoverJson = await SendRequestAsync("GET", $"https://127.0.0.1:{port}/lol-champ-select/v1/session", null);
                        ChampionHoverInfo championHoverObject = JsonConvert.DeserializeObject<ChampionHoverInfo>(championHoverJson);

                        int championHoverId = championHoverObject.myTeam.FirstOrDefault(x => x.summonerId == summonerId).championId;

                        HtmlDocument htmlDoc = default;
                        string championLockId = default;
                        Dictionary<string, bool> rolesActive = default;
                        if (championHoverId != 0)
                        {
                            string championJson = await SendRequestAsync("GET", $"http://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default/v1/champions/{championHoverId}.json", null);
                            ChampionInfo championJsonObject = JsonConvert.DeserializeObject<ChampionInfo>(championJson);
                            Regex regex = new Regex(@"[^A-Za-z0-9]+");
                                                      
                            if (champion == default || champion != regex.Replace(championJsonObject.name, ""))
                            {
                                champion = regex.Replace(championJsonObject.name, "");

                                htmlDoc = new HtmlWeb().Load($"https://na.op.gg/champion/{champion}/statistics/");

                                string xpath = "//table[@class='champion-overview__table champion-overview__table--summonerspell']//img[contains(@src, 'Summoner')]";
                                List<string> summonerImages = new List<string>();
                                Console.WriteLine(champion);
                                foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes(xpath))
                                {
                                    summonerImages.Add("https:" + node.GetAttributeValue("src", "nothing"));
                                }

                                xpath = $"//ul[@class='champion-stats-position']//li";
                                rolesActive = new Dictionary<string, bool>() { { "top", false }, { "jungle", false }, { "mid", false }, { "adc", false }, { "support", false } };

                                foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes(xpath))
                                {
                                    string role = node.GetAttributeValue("data-position", "nothing").ToLower();

                                    rolesActive[role] = true;
                                }

                                Action act = () => {
                                window.ChampionText.Text = championJsonObject.name;

                                window.ChampionIcon.Source = new BitmapImage(new Uri($@"https://opgg-static.akamaized.net/images/lol/champion/{champion}.png?image=q_auto,w_140&v=1596679559", UriKind.Absolute));
                                
                                window.Summoner1Image.Source = new BitmapImage(new Uri(summonerImages[0], UriKind.Absolute));
                                window.Summoner2Image.Source = new BitmapImage(new Uri(summonerImages[1], UriKind.Absolute));
                                window.Summoner3Image.Source = new BitmapImage(new Uri(summonerImages[2], UriKind.Absolute));
                                window.Summoner4Image.Source = new BitmapImage(new Uri(summonerImages[3], UriKind.Absolute));

                                window.TopButton.IsHitTestVisible = rolesActive["top"];
                                window.TopImage.Opacity = (rolesActive["top"]) ? 1 : 0.25;
                                window.JungleButton.IsHitTestVisible = rolesActive["jungle"];
                                window.JungleImage.Opacity = (rolesActive["jungle"]) ? 1 : 0.25;
                                window.MidButton.IsHitTestVisible = rolesActive["mid"];
                                window.MidImage.Opacity = (rolesActive["mid"]) ? 1 : 0.25;
                                window.BotButton.IsHitTestVisible = rolesActive["adc"];
                                window.BotImage.Opacity = (rolesActive["adc"]) ? 1 : 0.25;
                                window.SupportButton.IsHitTestVisible = rolesActive["support"];
                                window.SupportImage.Opacity = (rolesActive["support"]) ? 1 : 0.25;
                                };
                                window.Dispatcher.Invoke(act);

                                championLockId = await SendRequestAsync("GET", $"https://127.0.0.1:{port}/lol-champ-select/v1/current-champion", null);
                            }
                        }

                        if (championLockId != "0" && championLockId != default && !champLocked)
                        {
                            Console.WriteLine("A Champion has been locked in, generating rune page(s) and item set(s)...");
                            champLocked = true;
                                                      
                            string runePagesJson = await SendRequestAsync("GET", $"https://127.0.0.1:{port}/lol-perks/v1/pages", null);
                            List<RunePageInfo> runePageObject = JsonConvert.DeserializeObject<List<RunePageInfo>>(runePagesJson);

                            foreach(string role in rolesActive.Where(x => x.Value == true).Select(x => x.Key))
                            {
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

                            /*foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes(xpath))
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
                            }*/
                        }
                    }
                    if (gamephase != "\"ChampSelect\"")
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

        public static void StartNewTask(Action action, TimeSpan pollInterval, CancellationToken token, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None)
        {
            Task.Factory.StartNew(
                () =>
                {
                    do
                    {
                        try
                        {
                            action();
                            if (token.WaitHandle.WaitOne(pollInterval)) break;
                        }
                        catch
                        {
                            return;
                        }
                    }
                    while (true);
                },
                token,
                taskCreationOptions,
                TaskScheduler.Default);
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

    public class SummonerInfo
    {
        public long summonerId;
    }

    public class MyTeam
    {
        public long summonerId;
        public int championId;
    }

    public class ChampionHoverInfo
    {
        public List<MyTeam> myTeam;
    }

    public class ChampionInfo
    {
        public int id;
        public string name;
    }

    public class RunePageInfo
    {
        public int id;
        public string name;
    }


    public class ItemSets
    {
        public long accountId;
        public List<ItemSet> itemSets;
        public long timestamp;
    }

}



