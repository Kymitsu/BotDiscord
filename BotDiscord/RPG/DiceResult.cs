using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotDiscord.RPG
{
    public class DiceResult
    {
        private List<int> _bonus = new List<int>();


        public DiceResult(List<int> diceResults, params int[] bonus)
        {
            DiceResults = diceResults;
            _bonus = bonus.ToList();
            _bonus.Remove(0);
            SumRolls();
            BuildResultText();
        }

        private void SumRolls()
        {
            Total = DiceResults.Sum() + _bonus.Sum();
        }

        private void BuildResultText()
        {
            if (DiceResults.Count > 1)
                ResultText = $"`({string.Join(" + ", DiceResults)}) + {string.Join(" + ", _bonus)} = {Total}`";
            else
                ResultText = $"`{string.Join(" + ", DiceResults)} + {string.Join(" + ", _bonus)} = {Total}`";
        }

        public List<int> DiceResults { get; set; }
        public int Total { get; set; } = 0;
        public string ResultText { get; set; } = string.Empty;
    }
}
