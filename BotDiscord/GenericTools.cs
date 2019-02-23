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

namespace BotDiscord
{
    public static class GenericTools
    {
        private static Random random = new Random();

        public static DiceResult SimpleRoll(int dieSize, int bonus = 0)
        {
            return new DiceResult(new List<int> { random.Next(1, dieSize+1) }, bonus);
        }

        public static DiceResult AnimaRoll(int bonus = 0)
        {
            return AnimaRoll(bonus, new List<int>(), 90);
        }

        private static DiceResult AnimaRoll(int bonus, List<int> diceResults, int openRollValue)
        {

            openRollValue = openRollValue > 100 ? 100 : openRollValue;
            int temp = random.Next(1, 100+1);
            diceResults.Add(temp);
            if (temp >= openRollValue)
            {
                return AnimaRoll(bonus, diceResults, openRollValue + 1);
            }
            else
            {
                return new DiceResult(diceResults, bonus);
            }
        }

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

    public class DiceResult
    {
        private List<int> _diceResults;
        private int _bonus = 0;
        private int _total = 0;
        private string _resultText = string.Empty;

        public DiceResult(List<int> diceResults, int bonus)
        {
            _diceResults = diceResults;
            _bonus = bonus;
            SumRolls();
            BuildResultText();
        }

        private void SumRolls()
        {
            _total = 0;
            foreach (int roll in _diceResults)
            {
                _total += roll;
            }
            _total += _bonus;
        }

        private void BuildResultText()
        {
            _resultText = string.Format("`{0} + {1} = {2}`", string.Join(" + ", _diceResults), _bonus, _total);
        }

        public List<int> DiceResults { get => _diceResults; set => _diceResults = value; }
        public int Total { get => _total; set => _total = value; }
        public string ResultText { get => _resultText; set => _resultText = value; }
    }
}
