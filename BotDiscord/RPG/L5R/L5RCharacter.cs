using BotDiscord;
using BotDiscord.RPG;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotDiscord.RPG.L5R
{
    public class L5RCharacter : PlayableCharacter
    {
        public string Clan { get; set; }
        public string FamilyName { get; set; }
        public string CurrentStance { get; set; }
        public Dictionary<string, int> Rings { get; set; }
        public new List<L5RStat> AllStats { get { return base.AllStats.Cast<L5RStat>().ToList(); } }

        public L5RCharacter(ExcelWorksheet excelWorksheet, string player)
        {
            Player = player;
            IsCurrent = false;
            ImageUrl = "";
            Name = excelWorksheet.Cells["I2"].Text;
            FamilyName = excelWorksheet.Cells["D2"].Text;
            Clan = excelWorksheet.Cells["D1"].Text;

            Rings = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            Rings.Add("Air", Convert.ToInt32(excelWorksheet.Cells["C6"].Value));
            Rings.Add("Eau", Convert.ToInt32(excelWorksheet.Cells["F6"].Value));
            Rings.Add("Vide", Convert.ToInt32(excelWorksheet.Cells["I6"].Value));
            Rings.Add("Feu", Convert.ToInt32(excelWorksheet.Cells["C8"].Value));
            Rings.Add("Terre", Convert.ToInt32(excelWorksheet.Cells["F8"].Value));

            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B18"].Text, excelWorksheet.Cells["C19"].Text, Convert.ToInt32(excelWorksheet.Cells["H19"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B18"].Text, excelWorksheet.Cells["C20"].Text, Convert.ToInt32(excelWorksheet.Cells["H20"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B18"].Text, excelWorksheet.Cells["C21"].Text, Convert.ToInt32(excelWorksheet.Cells["H21"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B18"].Text, excelWorksheet.Cells["C22"].Text, Convert.ToInt32(excelWorksheet.Cells["H22"].Value)));

            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B23"].Text, excelWorksheet.Cells["C24"].Text, Convert.ToInt32(excelWorksheet.Cells["H24"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B23"].Text, excelWorksheet.Cells["C25"].Text, Convert.ToInt32(excelWorksheet.Cells["H24"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B23"].Text, excelWorksheet.Cells["C26"].Text, Convert.ToInt32(excelWorksheet.Cells["H26"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B23"].Text, excelWorksheet.Cells["C27"].Text, Convert.ToInt32(excelWorksheet.Cells["H27"].Value)));

            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B28"].Text, excelWorksheet.Cells["C29"].Text, Convert.ToInt32(excelWorksheet.Cells["H29"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B28"].Text, excelWorksheet.Cells["C30"].Text, Convert.ToInt32(excelWorksheet.Cells["H30"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B28"].Text, excelWorksheet.Cells["C31"].Text, Convert.ToInt32(excelWorksheet.Cells["H31"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B28"].Text, excelWorksheet.Cells["C32"].Text, Convert.ToInt32(excelWorksheet.Cells["H32"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B28"].Text, excelWorksheet.Cells["C33"].Text, Convert.ToInt32(excelWorksheet.Cells["H33"].Value)));

            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B34"].Text, excelWorksheet.Cells["C35"].Text, Convert.ToInt32(excelWorksheet.Cells["H35"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B34"].Text, excelWorksheet.Cells["C36"].Text, Convert.ToInt32(excelWorksheet.Cells["H36"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B34"].Text, excelWorksheet.Cells["C37"].Text, Convert.ToInt32(excelWorksheet.Cells["H37"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B34"].Text, excelWorksheet.Cells["C38"].Text, Convert.ToInt32(excelWorksheet.Cells["H38"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B34"].Text, excelWorksheet.Cells["C39"].Text, Convert.ToInt32(excelWorksheet.Cells["H39"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B34"].Text, excelWorksheet.Cells["C40"].Text, Convert.ToInt32(excelWorksheet.Cells["H40"].Value)));

            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B41"].Text, excelWorksheet.Cells["C42"].Text, Convert.ToInt32(excelWorksheet.Cells["H42"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B41"].Text, excelWorksheet.Cells["C43"].Text, Convert.ToInt32(excelWorksheet.Cells["H43"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B41"].Text, excelWorksheet.Cells["C44"].Text, Convert.ToInt32(excelWorksheet.Cells["H44"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B41"].Text, excelWorksheet.Cells["C45"].Text, Convert.ToInt32(excelWorksheet.Cells["H45"].Value)));
            base.AllStats.Add(new L5RStat(excelWorksheet.Cells["B41"].Text, excelWorksheet.Cells["C46"].Text, Convert.ToInt32(excelWorksheet.Cells["H46"].Value)));
        }

        public string Roll(string rawStat, string ring)
        {
            string resultMsg;
            L5RStat stat;
            try
            {
                stat = AllStats.FindByRawStat(rawStat);
            }
            catch (Exception)
            {
                return "Error 404: Stat not found (" + rawStat + ")";
            }
            if (string.IsNullOrEmpty(ring) && string.IsNullOrEmpty(CurrentStance))
                return "Posture ou anneau manquant";

            if (!string.IsNullOrEmpty(ring))
                resultMsg = stat.Roll(Rings[ring]);
            else
                resultMsg = stat.Roll(Rings[CurrentStance]);

            return resultMsg;
        }

        public void SetCurrentStance(string ring)
        {
            if (string.IsNullOrEmpty(ring))
                CurrentStance = ring;
            else if (Rings.ContainsKey(ring))
                CurrentStance = ring;
            else
                throw new ArgumentException($"Changement de posture: Anneau en paramètre incorrect : {ring}");
        }

        public override string KeywordsHelp()
        {
            string helpText = "";
            foreach (var groupedStat in AllStats.GroupBy(x => x.Group).Select(grp => grp.ToList()))
            {
                helpText += groupedStat.First().Group + " :" + Environment.NewLine;
                helpText += "```";
                helpText += string.Join(", ", groupedStat.Select(x => x.Name));
                helpText += "```";
            }

            return $"Available keywords for !l r :{Environment.NewLine}{helpText}";
        }
    }
}
