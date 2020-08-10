using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BotDiscord.RPG
{
    public class AnimaCharacter
    {
        public static List<string> StatGroups = new List<string>() { "Caractéristique", "Résistance", "Champs principal", "Champs secondaire" };
       

        public AnimaCharacter(ExcelWorksheet excelWorksheet, string player)
        {
            Player = player;
            IsCurrent = false;

            //Character info
            Name = excelWorksheet.Cells["E1"].Text;
            Origine = excelWorksheet.Cells["P1"].Text;
            Class = excelWorksheet.Cells["F3"].Text;
            Level = Convert.ToInt32(excelWorksheet.Cells["E5"].Value);
            Hp = Convert.ToInt32(excelWorksheet.Cells["B12"].Value);
            Regeneration = Convert.ToInt32(excelWorksheet.Cells["J18"].Value);
            Exhaust = Convert.ToInt32(excelWorksheet.Cells["B18"].Value);
            Movement = Convert.ToInt32(excelWorksheet.Cells["F18"].Value);

            TotalKiPoints = Convert.ToInt32(excelWorksheet.Cells["V39"].Value);
            ArmorPoint = Convert.ToInt32(excelWorksheet.Cells["AC55"].Value);

            ZeonPoints = Convert.ToInt32(excelWorksheet.Cells["U15"].Value);
            Amr = Convert.ToInt32(excelWorksheet.Cells["U21"].Value);
            AmrRegen = Convert.ToInt32(excelWorksheet.Cells["U24"].Value);
            InnateMagic = Convert.ToInt32(excelWorksheet.Cells["U27"].Value);
            MagicLevel = Convert.ToInt32(excelWorksheet.Cells["AD8"].Value);

            PppFree = Convert.ToInt32(excelWorksheet.Cells["Q21"].Value);
            Luck = Convert.ToBoolean(excelWorksheet.Cells["DC30"].Value);
            Unluck = Convert.ToBoolean(excelWorksheet.Cells["DC153"].Value);
            DestinFuneste = Convert.ToBoolean(excelWorksheet.Cells["DC165"].Value);
            //Base stats
            foreach (var cell in excelWorksheet.Cells[22, 2, 30, 2])
            {
                BaseStats.Add(new Roll10Stat(StatGroups[0], cell.Text, Convert.ToInt32(cell.Offset(0, 9).Value)));
            }
            
            //Resistances
            foreach (var cell in excelWorksheet.Cells[32, 2, 36, 2])
            {
                Resistances.Add(new Roll100Stat(StatGroups[1], cell.Text, Convert.ToInt32(cell.Offset(0, 2).Value)));
            }

            //Battle stats
            BattleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["B14"].Text, Convert.ToInt32(excelWorksheet.Cells["B15"].Value)));
            BattleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["B52"].Text, Convert.ToInt32(excelWorksheet.Cells["AC52"].Value)));
            BattleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["B53"].Text, Convert.ToInt32(excelWorksheet.Cells["AC53"].Value)));
            BattleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["B54"].Text, Convert.ToInt32(excelWorksheet.Cells["AC54"].Value)));
            Roll100Stat defence = BattleStats.Where(x => x.Name == "Esquive" || x.Name == "Parade").OrderByDescending(x => x.Value).First();
            BattleStats.Add(new Roll100Stat(StatGroups[2], $"Défense : {defence.Name}", defence.Value));
            foreach (var cell in excelWorksheet.Cells[64, 2, 68, 2])
            {
                BattleStats.Add(new Roll100Stat(StatGroups[2], cell.Text, Convert.ToInt32(cell.Offset(0, 27).Value)));
            }
            
            BattleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["B71"].Text, Convert.ToInt32(excelWorksheet.Cells["AC71"].Value)));
            try
            {//TODO pourquoi ????
                BattleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["Q23"].Text, Convert.ToInt32(excelWorksheet.Cells["Q24"].Value)));
            }
            catch (Exception ex)
            {
                
            }

            //Secondary stats
            foreach (var cell in excelWorksheet.Cells[75,2,142,2])
            {
                if (!cell.Style.Font.Bold)
                {
                    SecondaryStats.Add(new Roll100Stat(StatGroups[3], cell.Text, Convert.ToInt32(cell.Offset(0, 27).Value)));
                }
            }
            SecondaryStats.Add(new Roll100Stat(StatGroups[3], excelWorksheet.Cells["G34"].Text, Convert.ToInt32(excelWorksheet.Cells["N34"].Value)));
            SecondaryStats.Add(new Roll100Stat(StatGroups[3], excelWorksheet.Cells["G35"].Text, Convert.ToInt32(excelWorksheet.Cells["N35"].Value)));
            SecondaryStats.RemoveAll(x => string.IsNullOrWhiteSpace(x.Name));

            AllStats.AddRange(BaseStats);
            AllStats.AddRange(Resistances);
            AllStats.AddRange(BattleStats);
            AllStats.AddRange(SecondaryStats);
        }

        public string Roll(string rawStat, int bonus)
        {
            string resultMsg = string.Empty;
            RollableStat rollableStat;
            try
            {
                rollableStat = this.AllStats.First(x => x.Name.ToLower() == rawStat.ToLower() || x.Aliases.Any(y => y.ToLower() == rawStat.ToLower()));
            }
            catch (Exception)
            {
                return "Error 404: Stat not found (" + rawStat + ")";
            }

            DiceResult resultDice = rollableStat.Roll(bonus, this.DestinFuneste);
            this.AddRollStatistics(rollableStat, resultDice);

            if (rollableStat is Roll100Stat)
            {
                resultMsg = string.Format("rolled : {0}{1}",
                    resultDice.ResultText,
                    (!string.IsNullOrEmpty(rollableStat.Name) ? " (" + rollableStat.Name + ")" : ""));

                // test si le jet et une maladress
                int failValue = GenericTools.CheckFailValue(this.Luck, this.Unluck, rollableStat.Value);
                if (resultDice.DiceResults.Last() <= failValue)
                {
                    // si oui lance le jet de maladress
                    int tempFail = resultDice.DiceResults.Last();
                    resultDice = rollableStat.FailRoll(tempFail);

                    // et affiche le resultat de maladress
                    resultMsg += string.Format("\nmaladress : {0}{1}",
                    resultDice.ResultText,
                    (!string.IsNullOrEmpty(rollableStat.Name) ? " (" + rollableStat.Name + ")" : ""));
                }
            }
            else
            {
                string rollOutcome = null;
                if (resultDice.DiceResults.First() - (rollableStat.Value + bonus) < 0)
                    rollOutcome = "won";
                else rollOutcome = "failed";

                resultMsg = string.Format("rolled : {0} against {1}{2}, {3} by {4}",
                    resultDice.DiceResults.First(),
                    rollableStat.Value + bonus,
                    (!string.IsNullOrEmpty(rollableStat.Name) ? " (" + rollableStat.Name + ")" : ""),
                    rollOutcome,
                    resultDice.DiceResults.First() - (rollableStat.Value + bonus));
            }

            return resultMsg;
        }

        private void AddRollStatistics(RollableStat stat, DiceResult dice)
        {
            string statName = stat.Name.Replace(" ", "").Replace("Défense:", "");
            if (!RollStatistics.ContainsKey(statName))
            {
                RollStatistics.Add(statName, new List<DiceResult>());
            }

            RollStatistics[statName].Add(dice);
        }


        //Utiliser uniquement pour les events reactionAdded reactionDeleted
        public static string StaticRoll(AnimaCharacter character, string rawStat, int bonus)
        {
            return character.Roll(rawStat, bonus);
        }

        #region Properties

        public string Player { get; set; }
        public bool IsCurrent { get; set; }
        public string Name { get; set; }
        public string Origine { get; set; }
        public string Class { get; set; }
        public int Level { get; set; }
        public int Hp { get; set; }
        public int Regeneration { get; set; }
        public int Exhaust { get; set; }
        public int Movement { get; set; }
        public int TotalKiPoints { get; set; }
        public int ArmorPoint { get; set; }
        public int ZeonPoints { get; set; }
        public int Amr { get; set; }
        public int AmrRegen { get; set; }
        public int InnateMagic { get; set; }
        public int MagicLevel { get; set; }
        public int PppFree { get; set; }
        public Boolean Luck { get; set; }
        public Boolean Unluck { get; set; }
        public Boolean DestinFuneste { get; set; }
        public List<RollableStat> AllStats { get; set; } = new List<RollableStat>();
        public List<Roll10Stat> BaseStats { get; set; } = new List<Roll10Stat>();
        public List<Roll100Stat> Resistances { get; set; } = new List<Roll100Stat>();
        public List<Roll100Stat> BattleStats { get; set; } = new List<Roll100Stat>();
        public List<Roll100Stat> SecondaryStats { get; set; } = new List<Roll100Stat>();
        public Dictionary<string, List<DiceResult>> RollStatistics { get; set; } = new Dictionary<string, List<DiceResult>>();

        #endregion
    }
}
