using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace League_Itemset_Generator
{

    class Generator
    {
        static void Main(string[] args)
        {
            string json = JsonConvert.SerializeObject(new ItemSet("Anivia", "Mid"));

            using (var tw = new StreamWriter(@"C:\Users\tavinc\Documents\test.json", false))
            {
                tw.WriteLine(json);
                tw.Close();
            }
            Console.WriteLine(json);
        }
    }

        class ItemSet
    {
        public string map = "any";

        public List<Category> blocks;

        public string title;

        public string priority = "false";

        public string mode = "any";

        public string type = "custom";

        public int sortrank = 1;

        public string champion;

        public ItemSet(string name, string pos)
        {
            blocks = new List<Category>();
            title = name + " " + pos;
            champion = name.ToLower();

            Utility.allItemIds = new List<string>();

            blocks.Add(new Category(name: "Starters", champion: champion, role: pos));
            blocks.Add(new Category(name: "Core Build", champion: champion, role: pos));
            blocks.Add(new Category(name: "Other Items", champion: champion, role: pos));
        }
    }

    class Category
    {
        public string type;

        public List<Item> items;

        public Category(string name, string champion, string role)
        {
            items = new List<Item>();
            type = name;

            HtmlDocument htmlDoc = new HtmlWeb().Load($"https://na.op.gg/champion/{champion}/statistics/{role}");

            switch (type)
            {
                case "Starters":
                    HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes(Utility.paths["Skill Order"]);

                    type += " | Skills: ";
                    for (int i = 0; i < 4; i++)
                    {
                        type += nodes[i].InnerText.Trim();
                        if (i < 3)
                        {
                            type += ".";
                        }
                    }

                    nodes = htmlDoc.DocumentNode.SelectNodes(Utility.paths["Upgrade Order"]);

                    type += " - ";
                    for (int i = 0; i < 3; i++)
                    {
                        type += nodes[i].InnerText;
                        if (i < 2)
                        {
                            type += ">";
                        }
                    }

                    foreach (string id in getItemIds(htmlDoc, name).Reverse<string>())
                    {
                        Item newItem = new Item();

                        newItem.id = id;
                        newItem.count = (id == "2003") ? 2 : 1;

                        items.Add(newItem);
                    }

                    break;

                case "Core Build":
                    foreach (string id in getItemIds(htmlDoc, "Most Common Boot").Concat(getItemIds(htmlDoc, name)))
                    {
                        Item newItem = new Item();

                        newItem.id = id;
                        newItem.count = 1;

                        items.Add(newItem);
                    }

                    break;

                case "Other Items":
                    foreach (string id in getItemIds(htmlDoc, name))
                    {
                        Item newItem = new Item();

                        newItem.id = id;
                        newItem.count = 1;

                        items.Add(newItem);
                    }

                    Item temp = items[0];
                    items[0] = items[items.Count-2];
                    items[items.Count - 2] = temp;

                    temp = items[1];
                    items[1] = items[items.Count - 1];
                    items[items.Count - 1] = temp;

                    /* WebClient client = new WebClient();
                    Stream stream = client.OpenRead("http://ddragon.leagueoflegends.com/cdn/10.16.1/data/en_US/champion/Aatrox.json");
                    StreamReader reader = new StreamReader(stream);

                    dynamic DynamicData = JsonConvert.DeserializeObject(reader.ReadLine());

                    Console.WriteLine(DynamicData.data.Aatrox.recommended[4].blocks[6].items.Count);
                    Console.WriteLine(DynamicData.data.Aatrox.recommended[4].blocks[6].items[1].id);
                    Console.WriteLine(DynamicData.data.Aatrox.recommended[4].blocks[6].items[2].id);
                    Console.WriteLine(DynamicData.data.Aatrox.recommended[4].blocks[7].items[0].id);
                    Console.WriteLine(DynamicData.data.Aatrox.recommended[4].blocks[7].items[1].id);
                    Console.WriteLine(DynamicData.data.Aatrox.recommended[4].blocks[7].items[2].id);*/

                    break;
            }
        }

        List<string> getItemIds(HtmlDocument htmlDoc, string itemtype)
        {
            List<String> itemIds = new List<String>();

            foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes(Utility.paths[itemtype]))
            {
                MatchCollection regex = Regex.Matches(node.GetAttributeValue("src", "nothing"), @"\b(\d{4})\b");

                if (regex.Count > 0)
                {
                    string id = regex[0].ToString();

                    if (!Utility.allItemIds.Contains(id))
                    {
                        itemIds.Add(id);

                        Utility.allItemIds.Add(id);
                    }
                }
            }

            return itemIds;
        }
    }

    class Item
    {
        public string id { get; set; }
        public int count { get; set; }
    }

    static class Utility
    {
        public static Dictionary<string, string> paths = new Dictionary<string, string>()
            {
                {"Starters", "//text()[contains(., 'Starter Items')]/ancestor::tr[1]//img"},
                {"Skill Order", "//table[@class='champion-skill-build__table']//td"},
                {"Upgrade Order",  "//table[@class='champion-skill-build__table']//td/ancestor::td[1]//ul//span"},
                {"Core Build","//text()[contains(., 'Recommended Builds')]/ancestor::tr[1]//img"},
                {"Most Common Boot", "//text()[contains(., 'Boots')]/ancestor::tr[1]//img"},
                {"Other Items", "//text()[contains(., 'Recommended Builds')]/ancestor::tbody[1]//img"}
            };

        public static List<string> allItemIds;
    }


}


