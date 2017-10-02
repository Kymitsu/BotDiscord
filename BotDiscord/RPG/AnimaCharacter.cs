using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotDiscord.RPG
{
    public class AnimaCharacter
    {
        private string _name;
        private string _class;
        private int _level;

        private Dictionary<string, int> _baseStat = new Dictionary<string, int>();

        private int _hp;
        private int _initiative;
        private int _regeneration;
        private int _exhaust;
        private int _movement;

        private Dictionary<string, int> _resistance = new Dictionary<string, int>();

        //private int _attack;
        //private int _block;
        //private int _evasion;
        //private int _kiPoints;
        //private int _armorPoint;

        public AnimaCharacter(ExcelWorksheet excelWorksheet)
        {
            _name = excelWorksheet.Cells["E1"].Text;
            _class = excelWorksheet.Cells["F3"].Text;
            _level = Convert.ToInt32(excelWorksheet.Cells["E5"].Value);
            _hp = Convert.ToInt32(excelWorksheet.Cells["B12"].Value);
            _initiative = Convert.ToInt32(excelWorksheet.Cells["B15"].Value);
            _regeneration = Convert.ToInt32(excelWorksheet.Cells["J18"].Value);
            _exhaust = Convert.ToInt32(excelWorksheet.Cells["B18"].Value);
            _movement = Convert.ToInt32(excelWorksheet.Cells["F18"].Value);
        }

        public string Name { get => _name; set => _name = value; }
        public string Class { get => _class; set => _class = value; }
        public int Level { get => _level; set => _level = value; }
        public int Hp { get => _hp; set => _hp = value; }
        public int Initiative { get => _initiative; set => _initiative = value; }
        public int Regeneration { get => _regeneration; set => _regeneration = value; }
        public int Exhaust { get => _exhaust; set => _exhaust = value; }
        public int Movement { get => _movement; set => _movement = value; }
    }
}
