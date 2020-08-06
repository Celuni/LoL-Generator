using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace League_Itemset_Generator
{
    

    class Generator
    {
        static void Main(string[] args)
        {
            var html = @"https://na.op.gg/champion/aatrox/statistics/mid";

            HtmlWeb web = new HtmlWeb();

            var htmlDoc = web.Load(html);


            List<Category> categories = new List<Category>()
            {
                new Category(htmlDoc: htmlDoc, title: "Most Frequent Starters", xpath: "//text()[contains(., 'Starter Items')]/ancestor::tr[1]//img"),
                new Category(htmlDoc: htmlDoc, title: "Most Frequent Core Build", xpath: "//text()[contains(., 'Recommended Builds')]/ancestor::tr[1]//img"),
                new Category(htmlDoc: htmlDoc, title: "Most Frequent Boots", xpath: "//text()[contains(., 'Boots')]/ancestor::tr[1]//img"),
                new Category(htmlDoc: htmlDoc, title: "Other Items", xpath: "//text()[contains(., 'Starter Items')]/ancestor::tbody[1]//img")
            };
                      
            foreach (Category category in categories)
            {
                Console.WriteLine(category.type);

                List<string> ids = category.ids;
                foreach (string id in ids)
                {
                    Console.WriteLine(id);
                }
            }
                        
        }
    }

    static class Ids
    {
        public static List<string> idList = new List<string>();
    }

    class Category
    {
        public string type;

        public List<string> ids;

        public Category(HtmlDocument htmlDoc, string title, string xpath)
        {
            type = title;

            ids = new List<string>();

            var nodes = htmlDoc.DocumentNode.SelectNodes(xpath);
            foreach (HtmlNode node in nodes)
            {
                string value = node.GetAttributeValue("src", "nothing");
                MatchCollection mc = Regex.Matches(value, @"\b(\d{4})\b");

                foreach (Match m in mc)
                {
                    string id = m.ToString();

                    if (!Ids.idList.Contains(id))
                    {
                        ids.Add(id);
                        Ids.idList.Add(id);
                    }
                    
                }
            }
        }
    }
}


