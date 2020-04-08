using BotDiscord.RPG;
using Discord;
using Discord.Commands;
using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotDiscord.Modules
{
    [Group("!c")]
    public class CharacterModules : ModuleBase
    {
        [Command("help")]
        public async Task Help()
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync("upload, load, info, r");
        }

        [Command("list"), Summary("Admin uniquement")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task List()
        {
            string msg = "";
            foreach (var character in AnimaCharacterRepository.animaCharacters)
            {
                msg += character.Name + "\n";
            }
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(msg);
        }

        [Command("upload"), Summary("Charge les données d'un personnage depuis sa feuille Excel")]
        public async Task Upload()
        {
            if (Context.Message.Attachments.Any())
            {
                try
                {
                    GenericTools.HandleFile(Context.Message.Attachments.First(), Context.Message.Author.Mention);
                }
                catch (Exception ex)
                {
                    await Context.Channel.SendMessageAsync($"Could not download file.\n{ex.Message}");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("File not found. Please upload an Excel file while using this command.");
            }
        }

        [Command("new"), Summary("Lance les dés pour un nouveau perso. Caractéristique la plus basse n'est pas modifiée.")]
        public async Task New([Summary("Relance pour les valeurs inférieures ou égales")]int rerollVal = 0)
        {
            string msg = $"{Context.Message.Author.Mention}\n```Caractéristiques : ";
            List<int> caract = new List<int>();
            for (int i = 0; i < 8; i++)
            {
                caract.Add(GenericTools.CaractRoll(rerollVal));
            }
            caract.Sort();
            msg += string.Join(" | ", caract);
            msg += $"\nApparence : {GenericTools.CaractRoll(0)}";
            msg += "```";
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(msg);
        }

        [Command("load"), Summary("Charge le personnage à jouer")]
        public async Task Load(params string[] s)
        {
            string name = string.Join(" ", s);
            if(string.IsNullOrEmpty(name))
            {
                var characterString = "```";
                var characters = AnimaCharacterRepository.animaCharacters.Where(x => x.Player == Context.Message.Author.Mention);
                foreach (AnimaCharacter character in characters)
                {
                    characterString += character.Name + " - LVL " + character.Level + (character.IsCurrent ? " (loaded)" : "") +"\n";
                }
                characterString += "```";

                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync("List of playable characters :\n" + characterString);
            }
            else
            {
                AnimaCharacter character = null;
                try
                {
                    AnimaCharacterRepository.animaCharacters.Where(x => x.Player == Context.Message.Author.Mention).ToList().ForEach(x => x.IsCurrent = false);
                    character = AnimaCharacterRepository.animaCharacters.First(x => x.Name.ToLower() == name.ToLower());
                    character.IsCurrent = true;
                    
                    //await (Context.Message.Author as IGuildUser).ModifyAsync(x => x.Nickname = character.Name);
                }
                catch (InvalidOperationException ex)
                {
                    await Context.Channel.SendMessageAsync("Error 404: Character not found!");
                    throw;
                }
                catch (Exception)
                {
                    await Context.Channel.SendMessageAsync("Nickname could not be changed for " + Context.Message.Author.Mention);
                }

                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync($"Character : {character.Name} successfully loaded !"); 
                
            }
        }

        [Command("info"), Summary("Informations sur le personnage")]
        public async Task Info(params string[] s)
        {
            AnimaCharacter character;
            try
            {
                character = (from ac in AnimaCharacterRepository.animaCharacters where ac.Player == Context.Message.Author.Mention && ac.IsCurrent select ac).First();
            }
            catch (Exception ex)
            {
                await Context.Channel.SendMessageAsync("Error 404: Character not found or not loaded!");
                return;
            }

            string stat = string.Join(" ", s).ToLower();
            if (stat == null || stat == string.Empty)
            {
                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync(KeywordsHelp(character));
            }
            else
            {
                RollableStat rollableStat;
                try
                {
                    rollableStat = character.AllStats.First(x => x.Name.ToLower() == stat || x.Aliases.Any(y => y.ToLower() == stat));

                    await Context.Message.DeleteAsync();
                    await Context.Channel.SendMessageAsync(Context.User.Mention + " " + rollableStat.Name + " : " + rollableStat.Value);
                }
                catch (Exception)
                {
                    await Context.Channel.SendMessageAsync("Error 404: Stat not found (" + stat + ")");
                }

            }
        }

        [Command("r"), Summary("Lance les dées pour la stat passée en paramètre")]
        public async Task Roll(params string[] s)
        {
            AnimaCharacter character;
            try
            {
                character = (from ac in AnimaCharacterRepository.animaCharacters where ac.Player == Context.Message.Author.Mention && ac.IsCurrent select ac).First();
            }
            catch (Exception)
            {
                await Context.Channel.SendMessageAsync("Error 404: Character not found or not loaded!");
                return;
            }

            string statBonusStr = s.FirstOrDefault(x => x.StartsWith("+") || x.StartsWith("-"));
            int statBonus = Convert.ToInt32(statBonusStr);
            string stat = string.Join(" ", s).ToLower();
            if (!string.IsNullOrWhiteSpace(statBonusStr))
            {
                stat = stat.Replace($" {statBonusStr}", ""); 
            }

            if (stat == null || stat == string.Empty)
            {
                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync(KeywordsHelp(character));
            }
            else
            {
                RollableStat rollableStat;
                try
                {
                    rollableStat = character.AllStats.First(x => x.Name.ToLower() == stat || x.Aliases.Any(y => y.ToLower() == stat));
                }
                catch (Exception)
                {
                    await Context.Channel.SendMessageAsync("Error 404: Stat not found (" + stat + ")");
                    return;
                }
                await Context.Message.DeleteAsync();

                DiceResult tempDice = rollableStat.Roll(statBonus,character.DestinFuneste);

                if(rollableStat is Roll100Stat)
                {
                    await Context.Channel.SendMessageAsync(string.Format("{0} rolled : {1}{2}",
                        Context.User.Mention,
                        tempDice.ResultText,
                        (!string.IsNullOrEmpty(rollableStat.Name) ? " (" + rollableStat.Name + ")" : "")));

                    // test si le jet et une maladress
                    int failValue = GenericTools.CheckFailValue(character.Luck, character.Unluck, rollableStat.Value);
                    if (tempDice.DiceResults.Last() <= failValue)
                    {
                        // si oui lance le jet de maladress
                        int tempFail = tempDice.DiceResults.Last();
                        tempDice = rollableStat.FailRoll(tempFail);
                        // et affiche le resultat de maladress
                        await Context.Channel.SendMessageAsync(String.Format("{0} maladress : {1}{2}",
                        Context.User.Mention,
                        tempDice.ResultText,
                        (!string.IsNullOrEmpty(rollableStat.Name) ? " (" + rollableStat.Name + ")" : "")));
                    }
                }
                else
                {
                    string resultMessage = null;
                    if (tempDice.DiceResults.First() - (rollableStat.Value + statBonus) < 0)
                        resultMessage = "won";
                    else resultMessage = "failled";

                    await Context.Channel.SendMessageAsync(string.Format("{0} rolled : {1} against {2}{3}, {4} by {5}",
                        Context.User.Mention,
                        tempDice.DiceResults.First(),
                        rollableStat.Value + statBonus,
                        (!string.IsNullOrEmpty(rollableStat.Name) ? " (" + rollableStat.Name + ")" : ""),
                        resultMessage,
                        tempDice.DiceResults.First() - (rollableStat.Value + statBonus)));
                }
            }
        }

        public string KeywordsHelp(AnimaCharacter character)
        {
            string helpText = "";
            foreach (string group in AnimaCharacter.StatGroups)
            {
                helpText += group + " :";
                helpText += "\n```";
                helpText += string.Join(", ", character.AllStats.Where(x => x.Group == group).Select(x => x.Name));
                helpText += "```\n";
            }
            
            return string.Format("Available keywords for !c info/r :\n{0}", helpText);
        }
    }
}
