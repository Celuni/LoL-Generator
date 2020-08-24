using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace LoL_Generator
{
    public class ItemSet
    {
        public List<int> associatedChampions;

        public List<int> associatedMaps = new List<int>() { 11 };

        public List<Block> blocks;

        public string map = "SR";

        public string mode;

        public List<PreferredItemSlot> preferredItemSlots;

        public int sortrank;

        public string startedFrom;

        public string title;

        public string type;

        public string uid;

        public ItemSet(string champion, string role, int id)
        {
            associatedChampions = new List<int>() { id };

            blocks = new List<Block>();
            title = champion + " " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.ToLower()) + " (LoL Gen)";

            ItemUtility.allItemIds = new List<string>();

            blocks.Add(new Block(name: "Consumables", champion: champion, role: role));
            blocks.Add(new Block(name: "Starters", champion: champion, role: role));
            blocks.Add(new Block(name: "Core Build", champion: champion, role: role));
            blocks.Add(new Block(name: "Other Items", champion: champion, role: role));
        }

        [JsonConstructor]
        public ItemSet(){}
    }

    public class Block
    {
        public string hideIfSummonerSpell;

        public List<Item> items;

        public string showIfSummonerSpell;

        public string type;
               
        public Block(string name, string champion, string role)
        {
            items = new List<Item>();
            type = name;

            HtmlDocument htmlDoc = new HtmlWeb().Load($"https://na.op.gg/champion/{champion}/statistics/{role}");

            switch (type)
            {
                case "Consumables":
                    HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes(ItemUtility.Xpaths["Skill Order"]);

                    type += " | Skills: ";
                    for (int i = 0; i < 4; i++)
                    {
                        type += nodes[i].InnerText.Trim();
                        if (i < 3)
                        {
                            type += ".";
                        }
                    }

                    nodes = htmlDoc.DocumentNode.SelectNodes(ItemUtility.Xpaths["Upgrade Order"]);

                    type += " - ";
                    for (int i = 0; i < 3; i++)
                    {
                        type += nodes[i].InnerText;
                        if (i < 2)
                        {
                            type += ">";
                        }
                    }

                    AddItemIds(new List<string>() { "2003", "2031", "2033", "2055", "3364", "3363", "2138", "2140", "2139" });

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

            foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes(ItemUtility.Xpaths[itemtype]))
            {
                MatchCollection regex = Regex.Matches(node.GetAttributeValue("src", "nothing"), @"\b(\d{4})\b");

                if (regex.Count > 0)
                {
                    string id = regex[0].Value;

                    if (!ItemUtility.allItemIds.Contains(id))
                    {
                        itemIds.Add(id);

                        ItemUtility.allItemIds.Add(id);
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

        [JsonConstructor]
        public Block(){}
    }

    public class Item
    {
        public string id;
        public int count;
    }

    public class PreferredItemSlot
    {
        public string id;
        public int preferredItemSlot;
    }

    public static class ItemUtility
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
