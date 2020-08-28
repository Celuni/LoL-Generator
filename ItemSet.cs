using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace LoL_Generator
{
    //this class creates an item page object and aggregates data for it
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
            //assign the item set to a champion
            associatedChampions = new List<int>() { id };

            //initiate a list which will contain each block (category) of an item set
            blocks = new List<Block>();

            //load the statistics page from op.gg of the champion and the role
            HtmlDocument htmlDoc = new HtmlWeb().Load($"https://na.op.gg/champion/{champion}/statistics/{role}");

            string xpath = "//h1[@class='champion-stats-header-info__name']";
            HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes(xpath);

            title = HttpUtility.HtmlDecode(nodes[0].InnerText) + " " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.ToLower()) + " (LoL Gen)";

            //initiate a list which will contain the ids of all items added, this list is used to make sure no duplicate items are added
            ItemUtility.allItemIds = new List<string>();

            //add each block of items
            blocks.Add(new Block("Consumables", champion, role, htmlDoc));
            blocks.Add(new Block("Starters", champion, role, htmlDoc));
            blocks.Add(new Block("Core Build", champion, role, htmlDoc));
            blocks.Add(new Block("Other Items", champion, role, htmlDoc));
        }

        //tell the Json object constructor to use this empty constructor as the one above requires parameters
        [JsonConstructor]
        public ItemSet(){}
    }

    public class Block
    {
        public string hideIfSummonerSpell;

        public List<Item> items;

        public string showIfSummonerSpell;

        public string type;
               
        public Block(string name, string champion, string role, HtmlDocument htmlDoc)
        {
            //initiate list to hold items for this block
            items = new List<Item>();
            type = name;

            //this part is where XPath queries are getting parsed to get the items
            switch (type)
            {
                //this is the first block in an item list, it contains the skill order upgrade and all consumables and starting items
                case "Consumables":
                    //get the level by level skills upgrade order from the skills section of the webpage using the specified XPath query
                    HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes(ItemUtility.Xpaths["Skill Order"]);

                    //skill order will be the title of the block
                    //output the first three skills formatted like this: "Q.W.E | Skills: "
                    type += " | Skills: ";
                    for (int i = 0; i < 4; i++)
                    {
                        type += nodes[i].InnerText.Trim();
                        if (i < 3)
                        {
                            type += ".";
                        }
                    }

                    //get the overall skills upgrade order from the skills section of the webpage using the specified XPath query
                    nodes = htmlDoc.DocumentNode.SelectNodes(ItemUtility.Xpaths["Upgrade Order"]);

                    //output the skills upgrade order like this: "Q>W>E"
                    type += " - ";
                    for (int i = 0; i < 3; i++)
                    {
                        type += nodes[i].InnerText;
                        if (i < 2)
                        {
                            type += ">";
                        }
                    }

                    //add all consumble items and starting items e.g. Corrupting Potion, Long Sword etc.
                    AddItemIds(new List<string>() { "2003", "2031", "2033", "2055", "3364", "3363", "2138", "2140", "2139" });

                    break;
                //this is the second block in an item list, it contains the the starting items for the champion and role
                case "Starters":
                    //get starting items and reverse the order so it starts with a healing potion(s)
                    AddItemIds(GetItemIds(htmlDoc, name).Reverse<string>());

                    //add a warding totem
                    Item ward = new Item();
                    ward.id = "3340";
                    ward.count = 1;

                    items.Add(ward);

                    AddItemIds(GetItemIds(htmlDoc, "AltStarters").Reverse<string>());

                    break;

                //this is the third block in an item list, it contains the core items to build for the champion and role
                case "Core Build":
                    //first get the most common boot, then get the rest of the "regular" core items
                    AddItemIds(GetItemIds(htmlDoc, "Most Common Boot").Concat(GetItemIds(htmlDoc, name)));

                    break;

                //this is the fourth block in an item list, it contains the all other non starting items a champion and the role might use
                case "Other Items":
                    AddItemIds(GetItemIds(htmlDoc, name));

                    //swap the items so that boots go first and the rest of the items go last
                    Item temp = items[0];
                    items[0] = items[items.Count - 2];
                    items[items.Count - 2] = temp;

                    temp = items[1];
                    items[1] = items[items.Count - 1];
                    items[items.Count - 1] = temp;

                    break;
            }
        }

        //retrieves item ids using XPath queries
        List<string> GetItemIds(HtmlDocument htmlDoc, string itemtype)
        {
            //list to hold item ids
            List<String> itemIds = new List<String>();
            HtmlNodeCollection nodes = default;

            try
            {
                nodes = htmlDoc.DocumentNode.SelectNodes(ItemUtility.Xpaths[itemtype]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            if (nodes != default)
            {
                foreach (HtmlNode node in nodes)
                {
                    //parse each id from the image source using regex
                    //ex: source = "//opgg-static.akamaized.net/images/lol/item/3071.png?image=q_auto,w_42&amp;v=1596679559" and we want to get 3071, all item ids are exactly 4 numbers
                    MatchCollection regex = Regex.Matches(node.GetAttributeValue("src", "nothing"), @"\b(\d{4})\b");

                    //check if an id was found
                    if (regex.Count > 0)
                    {
                        string id = regex[0].Value;

                        //check if id is already in the item set
                        if (!ItemUtility.allItemIds.Contains(id))
                        {
                            itemIds.Add(id);

                            ItemUtility.allItemIds.Add(id);
                        }
                    }
                }
            }
            
            return itemIds;
        }

        //adds items item set list for each block
        void AddItemIds(IEnumerable<string> itemIds)
        {
            foreach (string id in itemIds)
            {
                Item newItem = new Item();

                newItem.id = id;

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
        //Xpath queries to get ids for various parts of an item page
        public static Dictionary<string, string> Xpaths = new Dictionary<string, string>()
            {
                {"Starters", "//text()[contains(., 'Starter Items')]/ancestor::tr[1]//img"},
                {"AltStarters", "//text()[contains(., 'Recommended Builds')]/preceding::tr[1]//img"},
                {"Skill Order", "//table[@class='champion-skill-build__table']//td"},
                {"Upgrade Order",  "//table[@class='champion-skill-build__table']//td/ancestor::td[1]//ul//span"},
                {"Core Build","//text()[contains(., 'Recommended Builds')]/ancestor::tr[1]//img"},
                {"Most Common Boot", "//text()[contains(., 'Boots')]/ancestor::tr[1]//img"},
                {"Other Items", "//text()[contains(., 'Recommended Builds')]/following::tr[position()<9]//img"}
            };

        //holds ids for all items added
        public static List<string> allItemIds;
    }
}
