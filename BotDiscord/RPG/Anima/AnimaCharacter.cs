using BotDiscord;
using BotDiscord.RPG;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BotDiscord.RPG.Anima
{
    public class AnimaCharacter : PlayableCharacter
    {
        public static List<string> StatGroups = new List<string>() { "Caractéristique", "Résistance", "Champs principal", "Champs secondaire" };


        public AnimaCharacter(ExcelWorksheet excelWorksheet, string player)
        {
            Player = player;
            IsCurrent = false;
            ImageUrl = excelWorksheet.Cells["AK1"].Text;

            //Character info
            Name = excelWorksheet.Cells["E1"].Text;
            Origine = excelWorksheet.Cells["P1"].Text;
            Class = excelWorksheet.Cells["F3"].Text;
            Level = Convert.ToInt32(excelWorksheet.Cells["E5"].Value);
            Hp = Convert.ToInt32(excelWorksheet.Cells["B12"].Value);
            string temp = excelWorksheet.Cells["B13"].Text;
            CurrentHp = string.IsNullOrEmpty(temp) ? Hp : Convert.ToInt32(temp);

            Regeneration = Convert.ToInt32(excelWorksheet.Cells["J18"].Value);
            Fatigue = Convert.ToInt32(excelWorksheet.Cells["B18"].Value);
            temp = excelWorksheet.Cells["B19"].Text;
            CurrentFatigue = string.IsNullOrEmpty(temp) ? Fatigue : Convert.ToInt32(temp);
            Movement = Convert.ToInt32(excelWorksheet.Cells["F18"].Value);

            TotalKiPoints = Convert.ToInt32(excelWorksheet.Cells["V39"].Value);
            temp = excelWorksheet.Cells["Z39"].Text;
            CurrentKi = string.IsNullOrEmpty(temp) ? TotalKiPoints : Convert.ToInt32(temp);

            ArmorPoint = Convert.ToInt32(excelWorksheet.Cells["AC55"].Value);

            ZeonPoints = Convert.ToInt32(excelWorksheet.Cells["U15"].Value);
            temp = excelWorksheet.Cells["U16"].Text;
            CurrentZeon = string.IsNullOrEmpty(temp) ? ZeonPoints : Convert.ToInt32(temp);
            Amr = Convert.ToInt32(excelWorksheet.Cells["U21"].Value);
            AmrRegen = Convert.ToInt32(excelWorksheet.Cells["U24"].Value);
            InnateMagic = Convert.ToInt32(excelWorksheet.Cells["U27"].Value);
            MagicLevel = Convert.ToInt32(excelWorksheet.Cells["AD8"].Value);

            PppFree = Convert.ToInt32(excelWorksheet.Cells["Q21"].Value);
            temp = excelWorksheet.Cells["Q22"].Text;
            CurrentPpp = string.IsNullOrEmpty(temp) ? PppFree : Convert.ToInt32(temp);
            IsLucky = Convert.ToBoolean(excelWorksheet.Cells["DC30"].Value);
            IsUnlucky = Convert.ToBoolean(excelWorksheet.Cells["DC153"].Value);
            DestinFuneste = Convert.ToBoolean(excelWorksheet.Cells["DC165"].Value);
            //Base stats
            foreach (var cell in excelWorksheet.Cells[22, 2, 30, 2])
            {
                BaseStats.Add(new Roll10Stat(StatGroups[0], cell.Text, Convert.ToInt32(cell.Offset(0, 9).Value)));
            }

            //Resistances
            foreach (var cell in excelWorksheet.Cells[32, 2, 36, 2])
            {
                Resistances.Add(new ResistanceStat(StatGroups[1], cell.Text, Convert.ToInt32(cell.Offset(0, 2).Value)));
            }

            //Battle stats
            BattleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["B14"].Text, Convert.ToInt32(excelWorksheet.Cells["B15"].Value)));
            BattleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["B52"].Text, Convert.ToInt32(excelWorksheet.Cells["AC52"].Value)));
            BattleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["B53"].Text, Convert.ToInt32(excelWorksheet.Cells["AC53"].Value)));
            BattleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["B54"].Text, Convert.ToInt32(excelWorksheet.Cells["AC54"].Value)));
            //Roll100Stat defence = BattleStats.Where(x => x.Name == "Esquive" || x.Name == "Parade").OrderByDescending(x => x.Value).First();
            //BattleStats.Add(new Roll100Stat(StatGroups[2], $"Défense : {defence.Name}", defence.Value));
            foreach (var cell in excelWorksheet.Cells[64, 2, 68, 2])
            {
                BattleStats.Add(new Roll100Stat(StatGroups[2], cell.Text, Convert.ToInt32(cell.Offset(0, 27).Value)));
            }

            BattleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["B71"].Text, Convert.ToInt32(excelWorksheet.Cells["AC71"].Value)));
            try
            {//TODO pourquoi ???? talent psy
                BattleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["Q23"].Text, Convert.ToInt32(excelWorksheet.Cells["Q24"].Value)));
            }
            catch (Exception ex)
            {

            }

            //Secondary stats
            foreach (var cell in excelWorksheet.Cells[75, 2, 143, 2])
            {
                if (!cell.Style.Font.Bold)
                {
                    if (!cell.Text.Contains('('))
                    {
                        SecondaryStats.Add(new Roll100Stat(StatGroups[3], cell.Text, Convert.ToInt32(cell.Offset(0, 27).Value)));
                    }
                    else
                    {
                        if(!string.IsNullOrEmpty(cell.Offset(0, 10).Text))
                            SecondaryStats.Add(new Roll100Stat(StatGroups[3], cell.Text, Convert.ToInt32(cell.Offset(0, 27).Value)));
                    }
                }
            }
            SecondaryStats.Add(new Roll100Stat(StatGroups[3], excelWorksheet.Cells["G34"].Text, Convert.ToInt32(excelWorksheet.Cells["N34"].Value)));
            SecondaryStats.Add(new Roll100Stat(StatGroups[3], excelWorksheet.Cells["G35"].Text, Convert.ToInt32(excelWorksheet.Cells["N35"].Value)));
            SecondaryStats.RemoveAll(x => string.IsNullOrWhiteSpace(x.Name));

            base.AllStats.AddRange(BaseStats);
            base.AllStats.AddRange(Resistances);
            base.AllStats.AddRange(BattleStats);
            base.AllStats.AddRange(SecondaryStats);
        }

        public string Roll(string rawStat, int bonus)
        {
            string resultMsg = string.Empty;
            AnimaRollableStat rollableStat;
            try
            {
                rollableStat = AllStats.FindByRawStat(rawStat);
            }
            catch (Exception)
            {
                return "Error 404: Stat not found (" + rawStat + ")";
            }

            DiceResult resultDice = rollableStat.Roll(bonus, DestinFuneste);
            
            if (rollableStat is Roll100Stat)
            {
                AddRollStatistics(rollableStat, resultDice);
                string bonusSymbol = bonus > 0 ? "+" : "";
                resultMsg = $"rolled : {resultDice.ResultText} {rollableStat.Name} {(bonus != 0 ? bonusSymbol + bonus : "")}";

                // test si le jet et une maladress
                int failValue = AnimaDiceHelper.CheckFailValue(IsLucky, IsUnlucky, rollableStat.Value);
                if (resultDice.DiceResults.Last() <= failValue)
                {
                    // si oui lance le jet de maladress
                    int tempFail = resultDice.DiceResults.Last();
                    resultDice = rollableStat.FailRoll(tempFail);

                    // et affiche le resultat de maladress
                    resultMsg += Environment.NewLine;
                    resultMsg += string.Format("maladress : {0} {1}",
                        resultDice.ResultText,
                        !string.IsNullOrEmpty(rollableStat.Name) ? rollableStat.Name : "");
                }
            }
            else if (rollableStat is ResistanceStat)
            {
                AddRollStatistics(rollableStat, resultDice);
                string bonusSymbol = bonus > 0 ? "+" : "";
                resultMsg = $"rolled : {resultDice.ResultText} {rollableStat.Name} {(bonus != 0 ? bonusSymbol + bonus : "")}";
            }
            else
            {
                string rollOutcome = null;
                int outcome = resultDice.DiceResults.First() - (rollableStat.Value + bonus);
                if (outcome < 0)
                    rollOutcome = "won";
                else 
                    rollOutcome = "failed";

                resultMsg = string.Format("rolled : {0} against {1} {2}, {3} by {4}",
                    resultDice.DiceResults.First(),
                    rollableStat.Value + bonus,
                    rollableStat.Name,
                    rollOutcome,
                    Math.Abs(outcome));
            }

            return resultMsg;
        }

        public override string KeywordsHelp()
        {
            string helpText = "";
            foreach (string group in StatGroups)
            {
                helpText += group + " :" + Environment.NewLine;
                helpText += "```";
                helpText += string.Join(", ", AllStats.Where(x => x.Group == group).Select(x => x.Name));
                helpText += "```";
            }

            return $"Available keywords for !c info/r :{Environment.NewLine}{helpText}";
        }

        private void AddRollStatistics(RollableStat stat, DiceResult dice)
        {
            //string statName = stat.Name.Replace(" ", "").Replace("Défense:", "");
            if (!RollStatistics.ContainsKey(stat))
            {
                RollStatistics.Add(stat, new List<DiceResult>());
            }

            RollStatistics[stat].Add(dice);
        }


        //Utiliser uniquement pour les events reactionAdded reactionDeleted
        //public static string StaticRoll(AnimaCharacter character, string rawStat, int bonus)
        //{
        //    return character.Roll(rawStat, bonus);
        //}

        #region Properties
        public string Origine { get; set; }
        public string Class { get; set; }
        public int Level { get; set; }
        public int Hp { get; set; }
        public int CurrentHp { get; set; }
        public int Regeneration { get; set; }
        public int Fatigue { get; set; }
        public int CurrentFatigue { get; set; }
        public int Movement { get; set; }
        public int TotalKiPoints { get; set; }
        public int CurrentKi { get; set; }
        public int ArmorPoint { get; set; }
        public int ZeonPoints { get; set; }
        public int CurrentZeon { get; set; }
        public int Amr { get; set; }
        public int AmrRegen { get; set; }
        public int InnateMagic { get; set; }
        public int MagicLevel { get; set; }
        public int PppFree { get; set; }
        public int CurrentPpp { get; set; }
        public bool IsLucky { get; set; }
        public bool IsUnlucky { get; set; }
        public bool DestinFuneste { get; set; }

        public new List<AnimaRollableStat> AllStats { get { return base.AllStats.Cast<AnimaRollableStat>().ToList(); } }
        public List<Roll10Stat> BaseStats { get; set; } = new List<Roll10Stat>();
        public List<ResistanceStat> Resistances { get; set; } = new List<ResistanceStat>();
        public List<Roll100Stat> BattleStats { get; set; } = new List<Roll100Stat>();
        public List<Roll100Stat> SecondaryStats { get; set; } = new List<Roll100Stat>();
        public Dictionary<RollableStat, List<DiceResult>> RollStatistics { get; set; } = new Dictionary<RollableStat, List<DiceResult>>();

        #endregion
    }
}
