using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using BotDiscord.RPG.L5R;

namespace BotDiscord.RPG.Anima
{
    public static class CharacterRepository
    {
        private static readonly string savePath = Directory.GetCurrentDirectory();
        public static List<PlayableCharacter> Characters { get; private set; } = new List<PlayableCharacter>();

        public static void LoadFromCurrentDirectory()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string[] allFiles = Directory.GetFiles(savePath, "@*");
            int cCount = 0;
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss", CultureInfo.CurrentCulture)} Loading characters: {cCount}/{allFiles.Length}");
            foreach (string file in allFiles)
            {
                string mention = "<" + Path.GetFileNameWithoutExtension(file).Split('_').First() + ">";
                using (Stream strm = File.Open(file, FileMode.Open)) 
                {
                    MemoryStream memoryStream = new MemoryStream();
                    strm.CopyTo(memoryStream);
                    using (ExcelPackage package = new ExcelPackage(memoryStream))
                    {
                        PlayableCharacter character = null;
                        ExcelWorkbook workbook = package.Workbook;
                        ExcelWorksheet worksheet = workbook.Worksheets["Feuille de personnage"];
                        if (worksheet != null)//Fiche de perso anima
                        {
                            character = new AnimaCharacter(worksheet, mention);
                        }
                        worksheet = workbook.Worksheets["Stat"];
                        if (worksheet != null)//Fiche de perso L5R => peut encore changer
                        {
                            character = new L5RCharacter(worksheet, mention);
                        }
                        Characters.Add(character);
                    }
                }
                cCount++;
                Console.Clear();
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss", CultureInfo.CurrentCulture)} Loading characters: {cCount}/{allFiles.Length}");
            }
            sw.Stop();
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss", CultureInfo.CurrentCulture)} All characters loaded in : {sw.ElapsedMilliseconds}ms");
        }

        //public static AnimaCharacter FindCurrentByMention(string mention)
        //{
        //    return (from ac in Characters where ac.Player == mention && ac.IsCurrent select ac).First();
        //}

        public static T FindCurrentByMention<T>(string mention) where T : PlayableCharacter
        {
            return Characters.OfType<T>().First(x => x.Player == mention && x.IsCurrent);
        }

        public static IEnumerable<T> FindByMention<T>(string mention) where T : PlayableCharacter
        {
            return Characters.OfType<T>().Where(x => x.Player == mention);
        }

        public static T Find<T>(string mention, string name) where T : PlayableCharacter
        {
            return Characters.OfType<T>().First(x => x.Player == mention && x.Name.ToLower() == name.ToLower());
        }

        public static void SaveExcelCharacter(ExcelPackage package, string mention, string characterName)
        {
            string fileName = mention.Replace("<", "").Replace(">", "");
            string extension = ".xlsx";
            package.SaveAs(new FileInfo(savePath + Path.DirectorySeparatorChar + fileName + "_" + characterName + extension));
        }

        public static void SaveLoadedCharacters()
        {
            foreach (PlayableCharacter character in Characters.Where(x => x.IsCurrent))
            {
                string fileName = $"{character.Player.Replace("<", "").Replace(">", "")}_{character.Name}.xlsx";
                FileInfo file = new FileInfo($"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}{fileName}");

                if (character is AnimaCharacter animaCharacter)
                    SaveAnimaCharacter(animaCharacter, file);

                //if (character is L5RCharacter)
                //    SaveL5RCharacter(character as L5RCharacter, file);
            }
        }

        private static void SaveAnimaCharacter(AnimaCharacter character, FileInfo file)
        {
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorkbook workbook = package.Workbook;
                ExcelWorksheet worksheet = workbook.Worksheets["Feuille de personnage"];
                worksheet.Cells["B13"].Value = character.CurrentHp;
                worksheet.Cells["B19"].Value = character.CurrentFatigue;
                worksheet.Cells["U16"].Value = character.CurrentZeon;
                worksheet.Cells["Q22"].Value = character.CurrentPpp;
                worksheet.Cells["Z39"].Value = character.CurrentKi;
                worksheet.Cells["AK1"].Value = character.ImageUrl;

                package.Save();
            }
        }

        private static bool SaveL5RCharacter(L5RCharacter character, FileInfo file)
        {
            throw new NotImplementedException();
        }

        public static void UnloadCharacters()
        {
            Characters.ForEach(x => x.IsCurrent = false);
        }
        
        public static void DeleteExcelCharacter(PlayableCharacter character)
        {
            string mention = character.Player.Replace("<", "").Replace(">", "");
            string filename = mention + "_" + character.Name + ".xlsx";

            File.Delete(savePath + Path.DirectorySeparatorChar + filename);
            Characters.Remove(character);
        }
    }
}
