using System;
using System.Collections.Generic;
using System.Text;

namespace BotDiscord.RPG.L5R
{
    public static class L5RDiceHelper
    {
        public static Dictionary<int, string> WhiteDiceMapping { get; set; }
        public static Dictionary<int, string> BlackDiceMapping { get; set; }


        static L5RDiceHelper()
        {
            WhiteDiceMapping = new Dictionary<int, string>();
            WhiteDiceMapping.Add(1, "blanc_1_2");
            WhiteDiceMapping.Add(2, "blanc_1_2");
            WhiteDiceMapping.Add(3, "blanc_345");
            WhiteDiceMapping.Add(4, "blanc_345");
            WhiteDiceMapping.Add(5, "blanc_345");
            WhiteDiceMapping.Add(6, "blanc_67");
            WhiteDiceMapping.Add(7, "blanc_67");
            WhiteDiceMapping.Add(8, "blanc_89");
            WhiteDiceMapping.Add(9, "blanc_89");
            WhiteDiceMapping.Add(10, "blanc_10");
            WhiteDiceMapping.Add(11, "blanc_11");
            WhiteDiceMapping.Add(12, "blanc_12");

            BlackDiceMapping = new Dictionary<int, string>();
            BlackDiceMapping.Add(1, "noir_1");
            BlackDiceMapping.Add(2, "noir_2");
            BlackDiceMapping.Add(3, "noir_3");
            BlackDiceMapping.Add(4, "noir_4");
            BlackDiceMapping.Add(5, "noir_5");
            BlackDiceMapping.Add(6, "noir_6");
        }
    }
}
