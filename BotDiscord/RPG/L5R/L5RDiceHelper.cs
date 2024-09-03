using BotDiscord.RPG;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotDiscord.RPG.L5R
{
    public static class L5RDiceHelper
    {
        public static Dictionary<int, string> WhiteDiceMapping { get; set; }
        public static Dictionary<int, string> BlackDiceMapping { get; set; }
        public static IReadOnlyCollection<GuildEmote> GuildEmotes { get; set; }

        static L5RDiceHelper()
        {
            WhiteDiceMapping = new Dictionary<int, string>
            {
                { 1, "blanc_1_2" },
                { 2, "blanc_1_2" },
                { 3, "blanc_345" },
                { 4, "blanc_345" },
                { 5, "blanc_345" },
                { 6, "blanc_67" },
                { 7, "blanc_67" },
                { 8, "blanc_89" },
                { 9, "blanc_89" },
                { 10, "blanc_10" },
                { 11, "blanc_11" },
                { 12, "blanc_12" }
            };

            BlackDiceMapping = new Dictionary<int, string>
            {
                { 1, "noir_1" },
                { 2, "noir_2" },
                { 3, "noir_3" },
                { 4, "noir_4" },
                { 5, "noir_5" },
                { 6, "noir_6" }
            };
        }

        public static string Roll(int whiteDiceNum, int blackDiceNum)
        {
            string whiteDiceResult = "";
            for (int i = 0; i < whiteDiceNum; i++)
            {
                int roll = DiceHelper.SimpleRoll(12);
                whiteDiceResult += $"<:{WhiteDiceMapping[roll]}:{GuildEmotes.FirstOrDefault(x => x.Name == WhiteDiceMapping[roll])?.Id}>";
            }
            string blackDiceResult = "";
            for (int i = 0; i < blackDiceNum; i++)
            {
                int roll = DiceHelper.SimpleRoll(6);
                blackDiceResult += $"<:{BlackDiceMapping[roll]}:{GuildEmotes.FirstOrDefault(x => x.Name == BlackDiceMapping[roll])?.Id}>";
            }

            return whiteDiceResult + blackDiceResult;
        }
    }
}
