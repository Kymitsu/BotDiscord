using BotDiscord.RPG.Anima;
using BotDiscord.RPG.L5R;
using BotDiscord.RPG;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Discord;

namespace BotDiscord.Services
{
    public class CharacterService
    {
        private static readonly string savePath = Directory.GetCurrentDirectory();
        private readonly ILogger _logger;
        public List<PlayableCharacter> Characters { get; set; } = new List<PlayableCharacter>();


        public CharacterService(IServiceProvider provider) 
        {
            _logger = provider.GetRequiredService<ILogger<CharacterService>>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public void LoadFromCurrentDirectory()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string[] allFiles = Directory.GetFiles(savePath, "@*");
            int cCount = 0;
            _logger.LogInformation($"{DateTime.Now.ToString("HH:mm:ss", CultureInfo.CurrentCulture)} Loading characters: {cCount}/{allFiles.Length}");

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
                _logger.LogInformation($"{DateTime.Now.ToString("HH:mm:ss", CultureInfo.CurrentCulture)} Loading characters: {cCount}/{allFiles.Length}");
            }
            sw.Stop();
            _logger.LogInformation($"{DateTime.Now.ToString("HH:mm:ss", CultureInfo.CurrentCulture)} All characters loaded in : {sw.ElapsedMilliseconds}ms");
        }

        public async Task<PlayableCharacter> HandleFile(IAttachment attachment, string mention)
        {
            if (Path.GetExtension(attachment.Filename) != ".xlsx") throw new ArgumentException("Format de fichier incorrect.");

            PlayableCharacter character = null;
            using (HttpClient hclient = new HttpClient())
            {
                Stream stream;
                try
                {
                    stream = await hclient.GetStreamAsync(attachment.Url);
                }
                catch (Exception)
                {
                    try
                    {
                        stream = await hclient.GetStreamAsync(attachment.ProxyUrl);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                MemoryStream memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                using (ExcelPackage package = new ExcelPackage(memoryStream))
                {
                    
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

                    if (!string.IsNullOrWhiteSpace(character.Name))
                    {
                        int charIndex = Characters.FindIndex(x => x.Player == mention && x.Name == character.Name);

                        if (charIndex == -1)
                        {
                            Characters.Add(character);
                        }
                        else
                        {
                            //TODO: get Current stat before overriding char
                            if (character is AnimaCharacter)
                            {
                                (character as AnimaCharacter).CurrentHp = (Characters[charIndex] as AnimaCharacter).CurrentHp;
                                (character as AnimaCharacter).CurrentFatigue = (Characters[charIndex] as AnimaCharacter).CurrentFatigue;
                                (character as AnimaCharacter).CurrentZeon = (Characters[charIndex] as AnimaCharacter).CurrentZeon;
                                (character as AnimaCharacter).CurrentPpp = (Characters[charIndex] as AnimaCharacter).CurrentPpp;
                                (character as AnimaCharacter).CurrentKi = (Characters[charIndex] as AnimaCharacter).CurrentKi;
                                character.ImageUrl = Characters[charIndex].ImageUrl;

                            }
                            Characters[charIndex] = character;
                        }

                        SaveExcelCharacter(package, mention, character.Name); 
                    }
                }
            }

            return character;
        }

        public T FindCurrentByMention<T>(string mention) where T : PlayableCharacter
        {
            return Characters.OfType<T>().FirstOrDefault(x => x.Player == mention && x.IsCurrent);
        }

        public IEnumerable<T> FindByMention<T>(string mention) where T : PlayableCharacter
        {
            return Characters.OfType<T>().Where(x => x.Player == mention);
        }

        public T Find<T>(string mention, string name) where T : PlayableCharacter
        {
            return Characters.OfType<T>().FirstOrDefault(x => x.Player == mention && x.Name.Equals(name.Trim(), StringComparison.CurrentCultureIgnoreCase));
        }

        public List<T> GetAllActiveCharacter<T>() where T : PlayableCharacter
        {
            return Characters.OfType<T>().Where(x => x.IsCurrent).ToList();
        }

        public static void SaveExcelCharacter(ExcelPackage package, string mention, string characterName)
        {
            string fileName = mention.Replace("<", "").Replace(">", "");
            string extension = ".xlsx";
            package.SaveAs(new FileInfo(savePath + Path.DirectorySeparatorChar + fileName + "_" + characterName + extension));
        }

        public void SaveLoadedCharacters()
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

        private void SaveAnimaCharacter(AnimaCharacter character, FileInfo file)
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

        private bool SaveL5RCharacter(L5RCharacter character, FileInfo file)
        {
            throw new NotImplementedException();
        }

        public void UnloadCharacters()
        {
            Characters.ForEach(x => x.IsCurrent = false);
        }

        public void DeleteExcelCharacter(PlayableCharacter character)
        {
            string mention = character.Player.Replace("<", "").Replace(">", "");
            string filename = mention + "_" + character.Name + ".xlsx";

            File.Delete(savePath + Path.DirectorySeparatorChar + filename);
            Characters.Remove(character);
        }
    }
}
