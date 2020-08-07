using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace League_Itemset_Generator
{

    class Generator
    {
        static void Main(string[] args)
        {
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
          
            Console.WriteLine(JsonConvert.SerializeObject(new ItemSet("Anivia", "Mid")));
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

        public ItemSet(string name, string lane)
        {
            blocks = new List<Category>();
            title = name + " " + lane;
            champion = name.ToLower();

            blocks.Add(new Category(name: "Starters", champion: champion, lane: lane));
            blocks.Add(new Category(name: "Core Build", champion: champion, lane: lane));
            blocks.Add(new Category(name: "Other Items", champion: champion, lane: lane));

        }
    }

    class Category
    {
        public List<Item> items;

        public string type;
        public Category(string name, string champion, string lane)
        {
            items = new List<Item>();
            type = name;

            HtmlDocument htmlDoc = new HtmlWeb().Load("https://na.op.gg/champion/" + champion + "/statistics/" + lane);

            if (type == "Starters")
            {
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes(XPath.paths["Skill Order"]);

                type += " | Skills: ";
                for (int i = 0; i < 4; i++)
                {
                    type += nodes[i].InnerText.Trim();
                    if (i < 3)
                    {
                        type += ".";
                    }
                }
                
                nodes = htmlDoc.DocumentNode.SelectNodes(XPath.paths["Upgrade Order"]);

                type += " - ";
                for (int i = 0; i < 3; i++)
                {
                    type += nodes[i].InnerText;
                    if (i < 2)
                    {
                        type += ">";
                    }
                }
            }
            else if (type == "Core Build")
            {
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes(XPath.paths["Most Common Boot"]);
                MatchCollection regex = Regex.Matches(nodes[0].GetAttributeValue("src", "nothing"), @"\b(\d{4})\b");
                Item newitem = new Item();

                newitem.id = regex[0].ToString();
                newitem.count = 1;
                items.Add(newitem);
            }


            foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes(XPath.paths[name]))
            {
                MatchCollection regex = Regex.Matches(node.GetAttributeValue("src", "nothing"), @"\b(\d{4})\b");
                Item newitem = new Item();

                foreach (Match id in regex)
                {
                    if (!items.Any(n => n.id == id.ToString()))
                    {
                        newitem.id = id.ToString();
                        newitem.count = 1;
                        items.Add(newitem);
                    }
                }
            }
        }
    }

    class Item
    {
        public string id { get; set; }
        public int count { get; set; }
    }

    static class XPath
    {
        public static Dictionary<string, string> paths = new Dictionary<string, string>()
            {
                {"Starters", "//text()[contains(., 'Starter Items')]/ancestor::tr[1]//img"},
                {"Skill Order", "//table[@class='champion-skill-build__table']//td"},
                {"Upgrade Order",  "//table[@class='champion-skill-build__table']//td/ancestor::td[1]//ul//span"},
                {"Core Build","//text()[contains(., 'Recommended Builds')]/ancestor::tr[1]//img"},
                {"Most Common Boot", "//text()[contains(., 'Boots')]/ancestor::tr[1]//img"},
                {"Other Items", "//text()[contains(., 'Starter Items')]/ancestor::tbody[1]//img"}/*,
                {"Recommended Skill Builds", "//li[@class='champion-stats__list__item tip tpd-delegation-uid-1']//img"}*/
            };
    }
}


