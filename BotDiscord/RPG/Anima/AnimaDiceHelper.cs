using System;
using System.Collections.Generic;
using System.Text;

namespace BotDiscord.RPG.Anima
{
    public static class AnimaDiceHelper
    {
        public static DiceResult AnimaRoll(bool destinFuneste, params int[] bonus)
        {
            if (destinFuneste == true)
                return AnimaRoll(bonus, new List<int>(), 101);
            else
                return AnimaRoll(bonus, new List<int>(), 90);
        }

        private static DiceResult AnimaRoll(int[] bonus, List<int> diceResults, int openRollValue)
        {
            int temp = DiceHelper.SimpleRoll(100);
            diceResults.Add(temp);
            if (temp >= openRollValue)
            {
                openRollValue++;
                openRollValue = openRollValue > 100 ? 100 : openRollValue;
                return AnimaRoll(bonus, diceResults, openRollValue);
            }
            else
            {
                return new DiceResult(diceResults, bonus);
            }
        }

        public static DiceResult BaseStatRoll(params int[] bonus)
        {
            int result = DiceHelper.SimpleRoll(10);
            switch (result)
            {
                case 1:
                    result = -2;
                    break;
                case 10:
                    result = 13;
                    break;
            }
            return new DiceResult(new List<int> { result }, bonus);
        }

        public static int CheckFailValue(bool luck, bool unluck, int bonus)
        {
            int failValue = 3;
            if (luck == true) failValue -= 1;
            if (unluck == true) failValue += 2;
            if (bonus >= 200) failValue -= 1;
            return failValue;
        }

        public static int CaractRoll(int rerollVal)
        {
            int result = DiceHelper.SimpleRoll(10);
            if (result <= rerollVal)
            {
                return CaractRoll(rerollVal);
            }

            return result;
        }
    }
}
