﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BotDiscord.RPG
{
    public class ResistanceStat : RollableStat
    {
        public ResistanceStat(string group, string name, int value) : base(group, name, value)
        {
        }

        public override DiceResult FailRoll(int score)
        {
            throw new NotImplementedException();
        }

        public override DiceResult Roll(int temporaryBonus, bool destinFuneste)
        {
            return DiceHelper.SimpleRoll(100, Value, temporaryBonus);
        }
    }
}
