using System;
using System.Collections.Generic;
using System.Text;

namespace BotDiscord.RPG
{
    public static class DiceHelper
    {
        private static Random random = new Random();

        public static DiceResult SimpleRoll(int dieSize, params int[] bonus)
        {
            return new DiceResult(new List<int> { random.Next(1, dieSize + 1) }, bonus);
        }

        public static int SimpleRoll(int dieSize)
        {
            return random.Next(1, dieSize + 1);
        }
    }
}
