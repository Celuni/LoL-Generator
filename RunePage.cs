using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace LoL_Generator
{
    //this class creates a rune page object and aggregates data for it
    class RunePage
    {
        public string name;

        public int primaryStyleId;

        public List<int> selectedPerkIds;

        public int subStyleId;

        public RunePage(string champion, string role)
        {
            //convert format of name to a traditionally formatted name (e.g. ThisISaNaMe -> Thisisaname) and specify it was created by this program
            name = champion + " " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.ToLower()) + " (LoL Gen)";
            //initiate the list of runes
            selectedPerkIds = new List<int>();

            //load the statistics page from op.gg of the champion and the role
            HtmlDocument htmlDoc = new HtmlWeb().Load($"https://na.op.gg/champion/{champion}/statistics/{role}");

            //go through each XPath query
            foreach (string path in RuneUtility.Xpaths)
            {
                foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes(path))
                {
                    //parse each id from the image source using regex
                    //ex: source = "//opgg-static.akamaized.net/images/lol/perk/8351.png?image=q_auto&amp;v=1596679559" and we want to get 8351, all rune ids are exactly 4 numbers
                    MatchCollection regex = Regex.Matches(node.GetAttributeValue("src", "nothing"), @"\b(\d{4})\b");

                    int id = int.Parse(regex[0].Value);

                    //specify the category of each rune (e.g. Precision, Inspiration)
                    if (primaryStyleId == default(int))
                    {
                        primaryStyleId = id;
                    }
                    else if (subStyleId == default(int))
                    {
                        subStyleId = id;
                    }
                    else
                    { 
                        //add the rune to the rune list
                        selectedPerkIds.Add(id);
                    }
                }
            }
        }
    }

    public class RuneUtility
    {
        //Xpath query to get ids for various parts of a rune page
        public static List<string> Xpaths = new List<string>()
        {
            {"//tbody[@class='tabItem ChampionKeystoneRune-1']//tr[1]//div[@class='perk-page__item perk-page__item--mark']//img"},
            {"//tbody[@class='tabItem ChampionKeystoneRune-1']//tr[1]//div[@class='perk-page__item perk-page__item--keystone perk-page__item--active']//img"},
            {"//tbody[@class='tabItem ChampionKeystoneRune-1']//tr[1]//div[@class='perk-page__item  perk-page__item--active']//img"},
            {"//tbody[@class='tabItem ChampionKeystoneRune-1']//tr[1]//div[@class='perk-page__item perk-page__item--active']//img"},
            {"//tbody[@class='tabItem ChampionKeystoneRune-1']//tr[1]//img[@class='active tip']"}

        };
    }
}
