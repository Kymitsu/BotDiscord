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
            _bonus.Remove(0); //removes 0 values
            SumRolls();
            BuildResultText();
        }

        private void SumRolls()
        {
            Total = DiceResults.Sum() + _bonus.Sum();
        }

        private void BuildResultText()
        {
            var temp = new StringBuilder();
            if (DiceResults.Count > 1)
                temp.Append($"`({string.Join(" + ", DiceResults)})");
            else
                temp.Append($"`{string.Join(" + ", DiceResults)}");

            if (DiceResults.Any() && _bonus.Any())
                temp.Append(" + ");

            temp.Append($"{string.Join(" + ", _bonus)}");

            if(DiceResults.Count + _bonus.Count > 1)
                temp.Append($" = {Total}");

            temp.Append("`");

            ResultText = temp.ToString();
            //if (DiceResults.Count > 1)
            //    ResultText = $"`({string.Join(" + ", DiceResults)}) + {string.Join(" + ", _bonus)} = {Total}`";
            //else
            //    ResultText = $"`{string.Join(" + ", DiceResults)} + {string.Join(" + ", _bonus)} = {Total}`";
        }

        public List<int> DiceResults { get; set; }
        public int Total { get; set; } = 0;
        public string ResultText { get; set; } = string.Empty;
    }
}
