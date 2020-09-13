using System;
using System.Collections.Generic;
using System.Text;

namespace BotDiscord.RPG
{
    public static class DiceHelper
    {
        private static Random random = null;

        public static DiceResult SimpleRoll(int dieSize, params int[] bonus)
        {
            if (random == null)
                random = new Random();

            return new DiceResult(new List<int> { random.Next(1, dieSize + 1) }, bonus);
        }

        public static int SimpleRoll(int dieSize)
        {
            if (random == null)
                random = new Random();

            return random.Next(1, dieSize + 1);
        }
    }
}
