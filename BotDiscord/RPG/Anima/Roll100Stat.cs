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

        public override DiceResult Roll(int temporaryBonus, Boolean destinFuneste)
        {
            return AnimaDiceHelper.AnimaRoll(destinFuneste, Value, temporaryBonus);
        }
        public override DiceResult FailRoll(int score)
        {
            List<int> temp = new List<int> ();
            if (this.Group == "Champs principal")
            {
                if (this.Name == "Initiative")
                {
                    temp.Add(this.Value);
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
                else if (this.Name == "Attaque")
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
                    temp.Add(this.Value);
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
            else if (this.Group == "Champs secondaire")
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
