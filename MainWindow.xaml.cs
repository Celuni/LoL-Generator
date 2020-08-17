using System.Windows;


namespace Lol_Generator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            /* ReadyTex = ReadyText;

             tokenSource = new CancellationTokenSource();
             StartNewTask(InitiatePolling, TimeSpan.FromSeconds(3), tokenSource.Token);*/

            /*            string clientpath = Path.GetDirectoryName(GetProcessFilename(Process.GetProcessesByName("LeagueClient").FirstOrDefault()));
                        lockfileloc = clientpath + @"\lockfile";

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

                        string summonerJson = await SendRequestAsync("GET", $"https://127.0.0.1:{port}/lol-summoner/v1/current-summoner", null);
                        SummonerInfo summonerJsonObject = JsonConvert.DeserializeObject<SummonerInfo>(summonerJson);

                        string summonerId = summonerJsonObject.summonerId;

                        string itemPagesJson = await SendRequestAsync("GET", $"https://127.0.0.1:{port}/lol-item-sets/v1/item-sets/{summonerId}/sets", null);
                        ItemSets itemPagesObject = JsonConvert.DeserializeObject<ItemSets>(itemPagesJson);

                        itemPagesObject.itemSets.Add(new ItemSet("Annie", "Mid", 1));

                        string itemsetsJson = JsonConvert.SerializeObject(itemPagesObject, Formatting.Indented);
                        Console.WriteLine(itemsetsJson);*/
        }
    }
}