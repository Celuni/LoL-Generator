using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;

namespace League_Itemset_Generator
{

    class Generator
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {

            string clientpath = Path.GetDirectoryName(GetProcessFilename(Process.GetProcessesByName("LeagueClient").FirstOrDefault()));

            string lockfile = "";
            using (FileStream fs = File.Open(clientpath + @"\lockfile", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] buf = new byte[1024];
                int c;

                while ((c = fs.Read(buf, 0, buf.Length)) > 0)
                {
                    lockfile = Encoding.UTF8.GetString(buf, 0, c);
                }
            }

            string port = lockfile.Split(':')[2];
            string password = lockfile.Split(':')[3];

            byte[] encoding = Encoding.ASCII.GetBytes($"riot:{password}");

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (requestMessage, certificate, chain, policyErrors) => true;

            using (HttpClient httpClient = new HttpClient(handler))
            {
                using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), $"https://127.0.0.1:{port}/swagger/v3/openapi.json"))
                {
                    HttpResponseMessage response = await httpClient.SendAsync(request);
                }

                long summonerId;
                using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), $"https://127.0.0.1:{port}/lol-summoner/v1/current-summoner"))
                {
                    request.Headers.TryAddWithoutValidation("Accept", "application/json");
                    request.Headers.TryAddWithoutValidation("Authorization", "Basic " + Convert.ToBase64String(encoding));

                    HttpResponseMessage response = await httpClient.SendAsync(request);

                    SummonerInfo jsonData = JsonConvert.DeserializeObject<SummonerInfo>(await response.Content.ReadAsStringAsync());

                    summonerId = jsonData.summonerId;
                }


                using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), $"https://127.0.0.1:{port}/lol-champ-select/v1/session"))
                {
                    request.Headers.TryAddWithoutValidation("Accept", "application/json");
                    request.Headers.TryAddWithoutValidation("Authorization", "Basic " + Convert.ToBase64String(encoding));

                    HttpResponseMessage response = await httpClient.SendAsync(request);

                    Session jsonData = JsonConvert.DeserializeObject<Session>(await response.Content.ReadAsStringAsync());

                    Console.WriteLine(jsonData.myTeam.FirstOrDefault(x => x.summonerId == summonerId).championId);
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