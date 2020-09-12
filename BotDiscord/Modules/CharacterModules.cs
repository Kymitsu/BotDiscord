using BotDiscord.RPG;
using BotDiscord.Services;
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
                caract.Add(AnimaDiceHelper.CaractRoll(rerollVal));
            }
            caract.Sort();
            msg += string.Join(" | ", caract);
            msg += $"\nApparence : {AnimaDiceHelper.CaractRoll(0)}";
            msg += "```";
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(msg);
        }

        [Command("load"), Summary("Charge le personnage à jouer")]
        public async Task Load(params string[] s)
        {
            string name = string.Join(" ", s);
            if (string.IsNullOrEmpty(name))
            {
                var characterString = "```";
                var characters = AnimaCharacterRepository.animaCharacters.Where(x => x.Player == Context.Message.Author.Mention);
                foreach (AnimaCharacter character in characters)
                {
                    characterString += character.Name + " - LVL " + character.Level + (character.IsCurrent ? " (loaded)" : "") + "\n";
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

        [Command("UImage"), Summary("Upload une image pour le personnage chargé")]
        [Alias("image", "img", "Image")]
        public async Task SetCharImage()
        {
            AnimaCharacter character = AnimaCharacterRepository.FindCurrentByMention(Context.Message.Author.Mention);
            if (character == null)
            {
                _ = Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync("Error 404: Character not found or not loaded!");
                return;
            }

            if (Context.Message.Attachments.Any())
            {
                var url = Context.Message.Attachments.First().Url;
                character.ImageUrl = url;
            }
            else
            {
                _ = Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync("File not found. Please upload an image while using this command.");
            }
        }

        [Command("Status")]
        [Alias("status", "statut", "Statut")]
        public async Task Status()
        {
            AnimaCharacter character = AnimaCharacterRepository.FindCurrentByMention(Context.Message.Author.Mention);
            if (character == null)
            {
                await Context.Channel.SendMessageAsync("Error 404: Character not found or not loaded!");
                return;
            }

            _ = Context.Message.DeleteAsync();
            var embed = new EmbedBuilder();
            embed.Author = new EmbedAuthorBuilder();
            embed.Author.Name = character.Name;
            embed.ThumbnailUrl = character.ImageUrl;

            embed.AddField("Hp", $"{character.CurrentHp}/{character.Hp}");
            embed.AddField("Fatigue", $"{character.CurrentFatigue}/{character.Fatigue}");
            embed.AddField("Zéons", $"{character.CurrentZeon}/{character.ZeonPoints}");
            embed.AddField("Ppp libres", $"{character.CurrentPpp}/{character.PppFree}");
            embed.AddField("Points de Ki", $"{character.CurrentKi}/{character.TotalKiPoints}");

            var msg = await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("reset")]
        [Alias("Reset")]
        public async Task ResetCurrentStat()
        {
            AnimaCharacter character = AnimaCharacterRepository.FindCurrentByMention(Context.Message.Author.Mention);
            _ = Context.Message.DeleteAsync();

            character.CurrentHp = character.Hp;
            character.CurrentFatigue = character.Fatigue;
            character.CurrentZeon = character.ZeonPoints;
            character.CurrentPpp = character.PppFree;
            character.CurrentKi = character.TotalKiPoints;

            var msg = await Context.Channel.SendMessageAsync($"{Context.User.Mention} {character.Name} reset");
            await Task.Delay(3000);
            _ = msg.DeleteAsync();
        }

        [Command("hp")]
        [Alias("HP", "Hp")]
        public async Task SetHp(string s)
        {
            AnimaCharacter character = AnimaCharacterRepository.FindCurrentByMention(Context.Message.Author.Mention);
            _ = Context.Message.DeleteAsync();

            if (s == "reset")
                character.CurrentHp = character.Hp;
            else if (s.Contains("+") || s.Contains("-"))
                character.CurrentHp += Convert.ToInt32(s);
            else
                character.CurrentHp = Convert.ToInt32(s);

            var msg = await Context.Channel.SendMessageAsync($"{Context.User.Mention} {character.Name}'s hp set to {character.CurrentHp}");
            await Task.Delay(3000);
            _ = msg.DeleteAsync();
        }

        [Command("fatigue")]
        [Alias("Fatigue")]
        public async Task SetFatigue(string s)
        {
            AnimaCharacter character = AnimaCharacterRepository.FindCurrentByMention(Context.Message.Author.Mention);
            _ = Context.Message.DeleteAsync();

            if (s == "reset")
                character.CurrentFatigue = character.Fatigue;
            else if (s.Contains("+") || s.Contains("-"))
                character.CurrentFatigue += Convert.ToInt32(s);
            else
                character.CurrentFatigue = Convert.ToInt32(s);

            var msg = await Context.Channel.SendMessageAsync($"{Context.User.Mention} {character.Name}'s fatigue set to {character.CurrentFatigue}");
            await Task.Delay(3000);
            _ = msg.DeleteAsync();
        }

        [Command("zéon")]
        [Alias("zeon", "Zéon", "Zeon")]
        public async Task SetZeon(string s)
        {
            AnimaCharacter character = AnimaCharacterRepository.FindCurrentByMention(Context.Message.Author.Mention);
            _ = Context.Message.DeleteAsync();

            if (s == "reset")
                character.CurrentZeon = character.ZeonPoints;
            else if (s.Contains("+") || s.Contains("-"))
                character.CurrentZeon += Convert.ToInt32(s);
            else
                character.CurrentZeon = Convert.ToInt32(s);

            var msg = await Context.Channel.SendMessageAsync($"{Context.User.Mention} {character.Name}'s zeon set to {character.CurrentZeon}");
            await Task.Delay(3000);
            _ = msg.DeleteAsync();
        }

        [Command("ppp")]
        [Alias("Ppp")]
        public async Task SetPpp(string s)
        {
            AnimaCharacter character = AnimaCharacterRepository.FindCurrentByMention(Context.Message.Author.Mention);
            _ = Context.Message.DeleteAsync();

            if (s == "reset")
                character.CurrentPpp = character.PppFree;
            else if (s.Contains("+") || s.Contains("-"))
                character.CurrentPpp += Convert.ToInt32(s);
            else
                character.CurrentPpp = Convert.ToInt32(s);

            var msg = await Context.Channel.SendMessageAsync($"{Context.User.Mention} {character.Name}'s ppp set to {character.CurrentPpp}");
            await Task.Delay(3000);
            _ = msg.DeleteAsync();
        }

        [Command("Ki")]
        [Alias("ki")]
        public async Task SetKi(string s)
        {
            AnimaCharacter character = AnimaCharacterRepository.FindCurrentByMention(Context.Message.Author.Mention);
            _ = Context.Message.DeleteAsync();

            if (s == "reset")
                character.CurrentKi = character.TotalKiPoints;
            else if (s.Contains("+") || s.Contains("-"))
                character.CurrentKi += Convert.ToInt32(s);
            else
                character.CurrentKi = Convert.ToInt32(s);

            var msg = await Context.Channel.SendMessageAsync($"{Context.User.Mention} {character.Name}'s ki set to {character.CurrentKi}");
            await Task.Delay(3000);
            _ = msg.DeleteAsync();
        }

        [Command("info"), Summary("Informations sur le personnage")]
        public async Task Info(params string[] s)
        {
            AnimaCharacter character = AnimaCharacterRepository.FindCurrentByMention(Context.Message.Author.Mention);
            if (character == null)
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
            AnimaCharacter character = AnimaCharacterRepository.FindCurrentByMention(Context.Message.Author.Mention);
            if(character == null)
            {
                await Context.Channel.SendMessageAsync("Error 404: Character not found or not loaded!");
                return;
            }

            string statBonusStr = s.FirstOrDefault(x => x.StartsWith("+") || x.StartsWith("-"));
            int bonus = Convert.ToInt32(statBonusStr);
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
                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync(string.Format("{0} {1}",
                    Context.User.Mention,
                    character.Roll(stat, bonus)));
            }
        }

        [Command("fight")]
        public async Task FightMessage()
        {
            _ = Context.Message.DeleteAsync();
            var embed = new EmbedBuilder();
            embed.Title = "Vos actions de combat!";

            StringBuilder sb = new StringBuilder();
            foreach (var kvp in CommandHandlingService.EmotesAction)
            {
                sb.AppendLine($"{kvp.Key} : {kvp.Value}");
            }
            embed.Description = sb.ToString();

            var msg = await Context.Channel.SendMessageAsync("", false, embed);

            CommandHandlingService.ReactionMessages.Add(msg.Id);

            foreach (Emoji emoji in CommandHandlingService.EmotesAction.Keys)
            {
                await msg.AddReactionAsync(emoji);
                await Task.Delay(1000);
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
