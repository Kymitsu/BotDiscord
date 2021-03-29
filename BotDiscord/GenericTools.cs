using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Http;
using OfficeOpenXml;
using BotDiscord.RPG;
using System.Linq;
using BotDiscord.RPG.Anima;
using BotDiscord.RPG.L5R;
using System.Threading.Tasks;

namespace BotDiscord
{
    public static class GenericTools
    {

        public static T FindByRawStat<T>(this List<T> list, string rawStat) where T : RollableStat
        {
            return list.First(x => x.Name.ToLower() == rawStat.ToLower() || x.Aliases.Any(y => y.ToLower() == rawStat.ToLower()));
        }

        public async static Task HandleFile(IAttachment attachment, string mention)
        {
            if (Path.GetExtension(attachment.Filename) != ".xlsx") return;

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
                    PlayableCharacter character = null;
                    ExcelWorkbook workbook = package.Workbook;
                    ExcelWorksheet worksheet = workbook.Worksheets["Feuille de personnage"];
                    if(worksheet != null)//Fiche de perso anima
                    {
                        character = new AnimaCharacter(worksheet, mention);
                    }
                    worksheet = workbook.Worksheets["Stat"];
                    if(worksheet != null)//Fiche de perso L5R => peut encore changer
                    {
                        character = new L5RCharacter(worksheet, mention);
                    }

                    int charIndex = CharacterRepository.Characters.FindIndex(x => x.Player == mention && x.Name == character.Name);

                    if (charIndex == -1)
                    {
                        CharacterRepository.Characters.Add(character); 
                    }
                    else
                    {
                        CharacterRepository.Characters[charIndex] = character;
                    }

                    CharacterRepository.SaveExcelCharacter(package, mention, character.Name);
                }
            }

        }
    }

    
}
