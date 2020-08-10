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
        public static int CheckFailValue(Boolean luck,Boolean unluck,int bonus)
        {
            int failValue = 3;
            if (luck == true) failValue -= 1;
            if (unluck == true) failValue += 2;
            if (bonus >= 200) failValue -= 1;
            return failValue;
        }
        public static DiceResult SimpleRoll(int dieSize, int bonus = 0)
        {
            return new DiceResult(new List<int> { random.Next(1, dieSize+1) }, bonus);
        }
        public static DiceResult BaseStatRoll(int bonus = 0)
        {
            int result = random.Next(1, 11);
            switch (result)
            {
                case 1:
                    result = -2;
                    break;
                case 10:
                    result = 13;
                    break;
            }
            return new DiceResult(new List<int> { result }, bonus);
        }
        public static int CaractRoll(int rerollVal)
        {
            int result = random.Next(1, 11);
            if(result <= rerollVal)
            {
                return CaractRoll(rerollVal);
            }

            return result;
        }

        public static DiceResult AnimaRoll(Boolean destinFuneste,int bonus = 0)
        {
            if (destinFuneste == true)
                return AnimaRoll(bonus, new List<int>(), 101);
            else
                return AnimaRoll(bonus, new List<int>(), 90);
        }
        public static DiceResult AnimaRoll(string group, Boolean destinFuneste,int bonus = 0)
        {
            if (group == "Résistance")
            {
                return SimpleRoll(100, bonus);
            }
            else
            {
                return AnimaRoll(destinFuneste,bonus);
            }
        }
        public static DiceResult FaillRoll(string group,string name,int bonus,int score)
        {
            List<int> temp= new List<int> { };
            switch (group )
            {
                case "Caractéristique":
                    // faill and succes already handled in roll10stat
                    return null;
                case "Résistance":
                // no faill  on resistance
                    return null;
                case "Champs principal":
                    // we have to make 3 possibility, initiative atack and the other main champ
                    switch (name)
                    {
                        case "Initiative":
                            temp.Add(bonus);
                            switch (score)
                            {
                                case 1:
                                    return new DiceResult (temp,-125);
                                case 2:
                                    return new DiceResult(temp, -100);
                                    // default is for 3 and more as faill result
                                default:
                                    return new DiceResult(temp, -75);
                            }
                        case "Attaque":
                            switch (score)
                            {
                                case 1:
                                    temp.Add(random.Next(1, 101));
                                    return new DiceResult(temp, +15);
                                case 2:
                                    temp.Add(random.Next(1, 101));
                                    return new DiceResult(temp, 0);
                                // default is for 3 and more as faill result
                                default:
                                    temp.Add(random.Next(1, 101));
                                    return new DiceResult(temp, -15);
                            }
                            // default case is for defense fail
                        default:
                            temp.Add(bonus);
                            switch (score)
                            {
                                case 1:
                                    temp.Add(-(random.Next(1, 101)));
                                    return new DiceResult(temp, -15);
                                case 2:
                                    temp.Add(-(random.Next(1, 101)));
                                    return new DiceResult(temp, 0);
                                // default is for 3 and more as faill result
                                default:
                                    temp.Add(-(random.Next(1, 101)));
                                    return new DiceResult(temp, +15);
                            }
                    }
                    // default is for secondary stat
                default:
                    // all secondary stat are done the same way
                    switch (score)
                    {
                        case 1:
                            temp.Add(random.Next(1, 101));
                            return new DiceResult(temp, +15);
                        case 2:
                            temp.Add(random.Next(1, 101));
                            return new DiceResult(temp, 0);
                        default:
                            temp.Add(random.Next(1, 101));
                            return new DiceResult(temp, -15);
                    }

            }
        }
        private static DiceResult AnimaRoll(int bonus, List<int> diceResults, int openRollValue)
        {
            int temp = random.Next(1, 100+1);
            diceResults.Add(temp);
            if (temp >= openRollValue)
            {
                openRollValue++;
                openRollValue = openRollValue > 100 ? 100 : openRollValue;
                return AnimaRoll(bonus, diceResults, openRollValue);
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
        private int _bonus = 0;

        public DiceResult(List<int> diceResults, int bonus)
        {
            DiceResults = diceResults;
            _bonus = bonus;
            SumRolls();
            BuildResultText();
        }

        private void SumRolls()
        {
            Total = 0;
            foreach (int roll in DiceResults)
            {
                Total += roll;
            }
            Total += _bonus;
        }

        private void BuildResultText()
        {
            ResultText = string.Format("`{0} + {1} = {2}`", string.Join(" + ", DiceResults), _bonus, Total);
        }

        public List<int> DiceResults { get; set; }
        public int Total { get; set; } = 0;
        public string ResultText { get; set; } = string.Empty;
    }
}
