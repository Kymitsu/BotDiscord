using System;
using System.Collections.Generic;
using System.Text;

namespace BotDiscord.RPG.Anima
{
    public class Roll100Stat : AnimaRollableStat
    {
        public Roll100Stat(string group, string name, int value) : base(group, name, value)
        {
        }

        public override DiceResult Roll(int temporaryBonus, bool destinFuneste)
        {
            return AnimaDiceHelper.AnimaRoll(destinFuneste, Value, temporaryBonus);
        }
        public override DiceResult FailRoll(int score)
        {
            List<int> temp = new List<int>();
            if (Group == "Champs principal")
            {
                if (Name == "Initiative")
                {
                    temp.Add(Value);
                    switch (score)
                    {
                        case 1:
                            return new DiceResult(temp, -125);
                        case 2:
                            return new DiceResult(temp, -100);
                        // default is for 3 and more as fail result
                        default:
                            return new DiceResult(temp, -75);
                    }
                }
                else if (Name == "Attaque")
                {
                    switch (score)
                    {
                        case 1:
                            temp.Add(DiceHelper.SimpleRoll(100));
                            return new DiceResult(temp, +15);
                        case 2:
                            temp.Add(DiceHelper.SimpleRoll(100));
                            return new DiceResult(temp, 0);
                        // default is for 3 and more as fail result
                        default:
                            temp.Add(DiceHelper.SimpleRoll(100));
                            return new DiceResult(temp, -15);
                    }
                }
                else
                {
                    temp.Add(Value);
                    switch (score)
                    {
                        case 1:
                            temp.Add(-DiceHelper.SimpleRoll(100));
                            return new DiceResult(temp, -15);
                        case 2:
                            temp.Add(-DiceHelper.SimpleRoll(100));
                            return new DiceResult(temp, 0);
                        // default is for 3 and more as fail result
                        default:
                            temp.Add(-DiceHelper.SimpleRoll(100));
                            return new DiceResult(temp, +15);
                    }
                }
            }
            else if (Group == "Champs secondaire")
            {
                // all secondary stat are done the same way
                switch (score)
                {
                    case 1:
                        temp.Add(DiceHelper.SimpleRoll(100));
                        return new DiceResult(temp, +15);
                    case 2:
                        temp.Add(DiceHelper.SimpleRoll(100));
                        return new DiceResult(temp, 0);
                    default:
                        temp.Add(DiceHelper.SimpleRoll(100));
                        return new DiceResult(temp, -15);
                }
            }

            throw new Exception("Should not happen");
        }
    }
}
