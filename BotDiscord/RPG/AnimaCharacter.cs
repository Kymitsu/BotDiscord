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
        #region Fields

        private string _player;
        private bool _isCurrent;

        //Character info
        private string _name;
        private string _origine;
        private string _class;
        private int _level;
        private int _hp;
        private int _regeneration;
        private int _exhaust;
        private int _movement;

        private int _totalKiPoints;
        private int _armorPoint;

        private int _zeonPoints;
        private int _amr;
        private int _amrRegen;
        private int _innateMagic;
        private int _magicLevel;

        private int _pppFree;

        private List<RollableStat> _allRollableStats = new List<RollableStat>();

        //Base stats
        private List<Roll10Stat> _baseStats = new List<Roll10Stat>();

        //Resistances
        private List<Roll100Stat> _resistances = new List<Roll100Stat>();

        //Battle stats
        private List<Roll100Stat> _battleStats = new List<Roll100Stat>();

        //Secondary stats
        private List<Roll100Stat> _secondaryStats = new List<Roll100Stat>();

        #endregion Fields

        public AnimaCharacter(ExcelWorksheet excelWorksheet, string player)
        {
            _player = player;
            _isCurrent = false;

            long time = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //Character info
            _name = excelWorksheet.Cells["E1"].Text;
            _origine = excelWorksheet.Cells["P1"].Text;
            _class = excelWorksheet.Cells["F3"].Text;
            _level = Convert.ToInt32(excelWorksheet.Cells["E5"].Value);
            _hp = Convert.ToInt32(excelWorksheet.Cells["B12"].Value);
            _regeneration = Convert.ToInt32(excelWorksheet.Cells["J18"].Value);
            _exhaust = Convert.ToInt32(excelWorksheet.Cells["B18"].Value);
            _movement = Convert.ToInt32(excelWorksheet.Cells["F18"].Value);

            _totalKiPoints = Convert.ToInt32(excelWorksheet.Cells["V39"].Value);
            _armorPoint = Convert.ToInt32(excelWorksheet.Cells["AC55"].Value);

            _zeonPoints = Convert.ToInt32(excelWorksheet.Cells["U15"].Value);
            _amr = Convert.ToInt32(excelWorksheet.Cells["U21"].Value);
            _amrRegen = Convert.ToInt32(excelWorksheet.Cells["U24"].Value);
            _innateMagic = Convert.ToInt32(excelWorksheet.Cells["U27"].Value);
            _magicLevel = Convert.ToInt32(excelWorksheet.Cells["AD8"].Value);

            _pppFree = Convert.ToInt32(excelWorksheet.Cells["Q21"].Value);

            //Base stats
            foreach (var cell in excelWorksheet.Cells[22, 2, 30, 2])
            {
                _baseStats.Add(new Roll10Stat(StatGroups[0], cell.Text, Convert.ToInt32(cell.Offset(0, 9).Value)));
            }
            
            //Resistances
            foreach (var cell in excelWorksheet.Cells[32, 2, 36, 2])
            {
                _resistances.Add(new Roll100Stat(StatGroups[1], cell.Text, Convert.ToInt32(cell.Offset(0, 2).Value)));
            }

            //Battle stats
            _battleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["B14"].Text, Convert.ToInt32(excelWorksheet.Cells["B15"].Value)));
            _battleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["B52"].Text, Convert.ToInt32(excelWorksheet.Cells["AC52"].Value)));
            _battleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["B53"].Text, Convert.ToInt32(excelWorksheet.Cells["AC53"].Value)));
            _battleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["B54"].Text, Convert.ToInt32(excelWorksheet.Cells["AC54"].Value)));
            Roll100Stat defence = _battleStats.Where(x => x.Name == "Esquive" || x.Name == "Parade").OrderByDescending(x => x.Value).First();
            _battleStats.Add(new Roll100Stat(StatGroups[2], $"Défense : {defence.Name}", defence.Value));
            foreach (var cell in excelWorksheet.Cells[64, 2, 68, 2])
            {
                _battleStats.Add(new Roll100Stat(StatGroups[2], cell.Text, Convert.ToInt32(cell.Offset(0, 27).Value)));
            }
            
            _battleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["B71"].Text, Convert.ToInt32(excelWorksheet.Cells["AC71"].Value)));
            try
            {//TODO pourquoi ????
                _battleStats.Add(new Roll100Stat(StatGroups[2], excelWorksheet.Cells["Q23"].Text, Convert.ToInt32(excelWorksheet.Cells["Q24"].Value)));
            }
            catch (Exception ex)
            {
                
            }

            //Secondary stats
            foreach (var cell in excelWorksheet.Cells[75,2,142,2])
            {
                if (!cell.Style.Font.Bold)
                {
                    _secondaryStats.Add(new Roll100Stat(StatGroups[3], cell.Text, Convert.ToInt32(cell.Offset(0, 27).Value)));
                }
            }
            _secondaryStats.Add(new Roll100Stat(StatGroups[3], excelWorksheet.Cells["G34"].Text, Convert.ToInt32(excelWorksheet.Cells["N34"].Value)));
            _secondaryStats.Add(new Roll100Stat(StatGroups[3], excelWorksheet.Cells["G35"].Text, Convert.ToInt32(excelWorksheet.Cells["N35"].Value)));
            _secondaryStats.RemoveAll(x => string.IsNullOrWhiteSpace(x.Name));

            _allRollableStats.AddRange(_baseStats);
            _allRollableStats.AddRange(_resistances);
            _allRollableStats.AddRange(_battleStats);
            _allRollableStats.AddRange(_secondaryStats);
            sw.Stop();
            time = sw.ElapsedMilliseconds;
        }

        #region Properties

        public string Player { get => _player; set => _player = value; }
        public bool IsCurrent { get => _isCurrent; set => _isCurrent = value; }
        public string Name { get => _name; set => _name = value; }
        public string Origine { get => _origine; set => _origine = value; }
        public string Class { get => _class; set => _class = value; }
        public int Level { get => _level; set => _level = value; }
        public int Hp { get => _hp; set => _hp = value; }
        public int Regeneration { get => _regeneration; set => _regeneration = value; }
        public int Exhaust { get => _exhaust; set => _exhaust = value; }
        public int Movement { get => _movement; set => _movement = value; }
        public int TotalKiPoints { get => _totalKiPoints; set => _totalKiPoints = value; }
        public int ArmorPoint { get => _armorPoint; set => _armorPoint = value; }
        public int ZeonPoints { get => _zeonPoints; set => _zeonPoints = value; }
        public int Amr { get => _amr; set => _amr = value; }
        public int AmrRegen { get => _amrRegen; set => _amrRegen = value; }
        public int InnateMagic { get => _innateMagic; set => _innateMagic = value; }
        public int MagicLevel { get => _magicLevel; set => _magicLevel = value; }
        public int PppFree { get => _pppFree; set => _pppFree = value; }
        public List<RollableStat> AllStats { get => _allRollableStats; set => _allRollableStats = value; }
        public List<Roll10Stat> BaseStats { get => _baseStats; set => _baseStats = value; }
        public List<Roll100Stat> Resistances { get => _resistances; set => _resistances = value; }
        public List<Roll100Stat> BattleStats { get => _battleStats; set => _battleStats = value; }
        public List<Roll100Stat> SecondaryStats { get => _secondaryStats; set => _secondaryStats = value; }
        
        #endregion
    }
}
