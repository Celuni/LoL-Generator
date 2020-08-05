using HtmlAgilityPack;
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
            var html = @"https://na.op.gg/champion/aatrox/statistics/top";

            HtmlWeb web = new HtmlWeb();

            var htmlDoc = web.Load(html);

            var nodes = htmlDoc.DocumentNode.SelectNodes("//tr[@class='champion-overview__row champion-overview__row--first']//img");

            foreach (HtmlNode node in nodes)
            {
                string value = node.GetAttributeValue("src", "nothin");
                MatchCollection mc = Regex.Matches(value, @"\b(\d{4})\b");

                Console.WriteLine(value);

                foreach (Match m in mc)
                {
                    Console.WriteLine(m);
                }
                
            }
            Console.WriteLine(nodes.Count());
        }
    }
}
