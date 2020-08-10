using System;
using System.Collections.Generic;
using System.Text;

namespace League_Itemset_Generator
{
    class RunePage
    {

    }

    public class SummonerInfo
    {
        public long summonerId { get; set; }
    }
    
    public class Session
    {
        public List<MyTeam> myTeam { get; set; }
    }

    public class MyTeam
    {
        public int championId { get; set; }
        public int championPickIntent { get; set; }
        public long summonerId { get; set; }
    }

}
