using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace BotDiscord.RPG
{
    public static class AnimaCharacterRepository
    {
        private static readonly string savePath = Directory.GetCurrentDirectory();
        public static List<AnimaCharacter> animaCharacters = new List<AnimaCharacter>();

        public static void LoadFromCurrentDirectory()
        {
            string[] allFiles = Directory.GetFiles(savePath, "@*");

            foreach (string file in allFiles)
            {
                string mention = "<" + Path.GetFileNameWithoutExtension(file).Split('_').First() + ">";
                Stream strm = File.Open(file, FileMode.Open);
                MemoryStream memoryStream = new MemoryStream();
                strm.CopyTo(memoryStream);
                using(ExcelPackage package = new ExcelPackage(memoryStream))
                {
                    ExcelWorkbook workbook = package.Workbook;
                    ExcelWorksheet worksheet = workbook.Worksheets["Feuille de personnage"];
                    AnimaCharacter animaCharacter = new AnimaCharacter(worksheet, mention);
                    animaCharacters.Add(animaCharacter);
                }
            }
        }

        public static void SaveExcelCharacter(ExcelPackage package, string mention, string characterName)
        {
            string fileName = mention.Replace("<", "").Replace(">", "");
            string extension = ".xlsx";
            package.SaveAs(new FileInfo(savePath + @"\" + fileName + "_" + characterName + extension));
        }
        
        public static void DeleteExcelCharacter()
        {

        }
    }
}
