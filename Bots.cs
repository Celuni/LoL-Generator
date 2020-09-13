using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoL_Generator
{
    public class BotInfo
    {
        public int id;
    }

    public class Bot
    {
        public string botDifficulty = "MEDIUM";

        public int championId;

        public string teamId = (Properties.Settings.Default.team == "one") ? "200" : "100";

        public Bot(List<BotInfo> botsList)
        {
            Random random = new Random();

            int index = random.Next(botsList.Count);

            championId = botsList[index].id;

            botsList.RemoveAt(index);
        }
    }
}
