using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace LoL_Generator
{
    class RunePage
    {
        public string name;

        public int primaryStyleId;

        public List<int> selectedPerkIds;

        public int subStyleId;

        public RunePage(string champion, string role)
        {
            name = champion + " " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role.ToLower()) + " (LoL Gen)";
            selectedPerkIds = new List<int>();

            HtmlDocument htmlDoc = new HtmlWeb().Load($"https://na.op.gg/champion/{champion}/statistics/{role}");

            foreach (string path in RuneUtility.Xpaths)
            {
                foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes(path))
                {
                    MatchCollection regex = Regex.Matches(node.GetAttributeValue("src", "nothing"), @"\b(\d{4})\b");

                    int id = int.Parse(regex[0].Value);

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
                        selectedPerkIds.Add(id);
                    }
                }
            }
        }
    }

    public class RuneUtility
    {
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
