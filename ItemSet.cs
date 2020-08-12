using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace LoL_Generator
{
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

        public ItemSet(string name, string role)
        {
            blocks = new List<Category>();
            title = name + " " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.ToLower());
            champion = name.ToLower();

            Utility.allItemIds = new List<string>();

            blocks.Add(new Category(name: "Consumables", champion: champion, role: role));
            blocks.Add(new Category(name: "Starters", champion: champion, role: role));
            blocks.Add(new Category(name: "Core Build", champion: champion, role: role));
            blocks.Add(new Category(name: "Other Items", champion: champion, role: role));
        }
    }

    class Category
    {
        public List<Item> items;

        public string type;

        public Category(string name, string champion, string role)
        {
            items = new List<Item>();
            type = name;

            HtmlDocument htmlDoc = new HtmlWeb().Load($"https://na.op.gg/champion/{champion}/statistics/{role}");

            switch (type)
            {
                case "Consumables":
                    HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes(Utility.Xpaths["Skill Order"]);

                    type += " | Skills: ";
                    for (int i = 0; i < 4; i++)
                    {
                        type += nodes[i].InnerText.Trim();
                        if (i < 3)
                        {
                            type += ".";
                        }
                    }

                    nodes = htmlDoc.DocumentNode.SelectNodes(Utility.Xpaths["Upgrade Order"]);

                    type += " - ";
                    for (int i = 0; i < 3; i++)
                    {
                        type += nodes[i].InnerText;
                        if (i < 2)
                        {
                            type += ">";
                        }
                    }

                    AddItemIds(new List<string>() { "2003", "2031", "2033", "2055", "2004", "3364", "3363", "2032", "2138", "2140", "2139" });

                    items[0].count = 1;

                    break;
                case "Starters":
                    AddItemIds(GetItemIds(htmlDoc, name).Reverse<string>());

                    Item ward = new Item();
                    ward.id = "3340";
                    ward.count = 1;

                    items.Add(ward);

                    break;

                case "Core Build":
                    AddItemIds(GetItemIds(htmlDoc, "Most Common Boot").Concat(GetItemIds(htmlDoc, name)));

                    break;

                case "Other Items":
                    AddItemIds(GetItemIds(htmlDoc, name));

                    Item temp = items[0];
                    items[0] = items[items.Count - 2];
                    items[items.Count - 2] = temp;

                    temp = items[1];
                    items[1] = items[items.Count - 1];
                    items[items.Count - 1] = temp;

                    break;
            }
        }

        List<string> GetItemIds(HtmlDocument htmlDoc, string itemtype)
        {
            List<String> itemIds = new List<String>();

            foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes(Utility.Xpaths[itemtype]))
            {
                MatchCollection regex = Regex.Matches(node.GetAttributeValue("src", "nothing"), @"\b(\d{4})\b");

                if (regex.Count > 0)
                {
                    string id = regex[0].Value;

                    if (!Utility.allItemIds.Contains(id))
                    {
                        itemIds.Add(id);

                        Utility.allItemIds.Add(id);
                    }
                }
            }

            return itemIds;
        }

        void AddItemIds(IEnumerable<string> itemIds)
        {
            foreach (string id in itemIds)
            {
                Item newItem = new Item();

                newItem.id = id;
                newItem.count = (id == "2003") ? 2 : 1;

                items.Add(newItem);
            }
        }
    }

    class Item
    {
        public string id { get; set; }
        public int count { get; set; }
    }

    static class Utility
    {
        public static Dictionary<string, string> Xpaths = new Dictionary<string, string>()
            {
                {"Starters", "//text()[contains(., 'Starter Items')]/ancestor::tr[1]//img"},
                {"Skill Order", "//table[@class='champion-skill-build__table']//td"},
                {"Upgrade Order",  "//table[@class='champion-skill-build__table']//td/ancestor::td[1]//ul//span"},
                {"Core Build","//text()[contains(., 'Recommended Builds')]/ancestor::tr[1]//img"},
                {"Most Common Boot", "//text()[contains(., 'Boots')]/ancestor::tr[1]//img"},
                {"Other Items", "//text()[contains(., 'Recommended Builds')]/following::tr[position()<9]//img"}
            };

        public static List<string> allItemIds;
    }
}
