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

namespace BotDiscord
{
    public static class GenericTools
    {
        public async static void HandleFile(IAttachment attachment, string mention)
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
                    ExcelWorkbook workbook = package.Workbook;
                    ExcelWorksheet worksheet = workbook.Worksheets["Feuille de personnage"];
                    AnimaCharacter animaCharacter = new AnimaCharacter(worksheet, mention);

                    int charIndex = AnimaCharacterRepository.animaCharacters.FindIndex(x => x.Player == mention && x.Name == animaCharacter.Name);

                    if (charIndex == -1)
                    {
                        AnimaCharacterRepository.animaCharacters.Add(animaCharacter); 
                    }
                    else
                    {
                        AnimaCharacterRepository.animaCharacters[charIndex] = animaCharacter;
                    }

                    AnimaCharacterRepository.SaveExcelCharacter(package, mention, animaCharacter.Name);
                }
            }

        }
    }

    
}
