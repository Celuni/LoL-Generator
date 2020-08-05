using System;
using System.Net;
using Newtonsoft.Json;

namespace League_Itemset_Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            string key = "RGAPI-34f7a362-6a59-4a20-a53a-88ebfd17305a";

            using (var w = new WebClient())
            {
                var json_data = string.Empty;
                // attempt to download JSON data as a string
                try
                {
                    json_data = w.DownloadString("https://na1.api.riotgames.com/lol/summoner/v4/summoners/by-name/Doublelift?api_key=" + key);
                    Console.WriteLine(json_data);                
                }
                catch (Exception) { }

            }
        }
    }
}
