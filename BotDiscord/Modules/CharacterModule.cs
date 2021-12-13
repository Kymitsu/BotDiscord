using BotDiscord.RPG;
using BotDiscord.RPG.Anima;
using BotDiscord.RPG.L5R;
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
    [Summary("Personnage")]
    public class CharacterModule : ModuleBase
    {
        [Command("!c upload")]
        [Summary("Charge les données d'un personnage depuis une feuille Excel")]
        public async Task Upload()
        {
            if (Context.Message.Attachments.Any())
            {
                try
                {
                    var msg = await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} Téléchargement en cour...");
                    await GenericTools.HandleFile(Context.Message.Attachments.First(), Context.Message.Author.Mention);
                    await msg.ModifyAsync(x => x.Content = $"{Context.Message.Author.Mention} Personnage uploadé avec succès");
                }
                catch (Exception ex)
                {
                    await Context.Channel.SendMessageAsync($"Could not download file.{Environment.NewLine}{ex.Message}");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("File not found. Please upload an Excel file while using this command.");
            }
        }

        [Command("!c load")]
        [Summary("Charge le personnage renseigné sinon affiche les personnages disponibles")]
        public async Task Load(params string[] s)
        {
            string name = string.Join(" ", s);
            if (string.IsNullOrEmpty(name))
            {
                var characterString = "```yaml" + Environment.NewLine;

                var animaChars = CharacterRepository.Characters.OfType<AnimaCharacter>().Where(x => x.Player == Context.Message.Author.Mention);
                if (animaChars.Any())
                {
                    characterString += "Anima:" + Environment.NewLine;
                    foreach (AnimaCharacter character in animaChars)
                    {
                        characterString += "   " + character.Name + " - LVL " + character.Level + (character.IsCurrent ? " (loaded)" : "") + Environment.NewLine;
                    } 
                }

                var lChars = CharacterRepository.Characters.OfType<L5RCharacter>().Where(x => x.Player == Context.Message.Author.Mention);
                if (lChars.Any())
                {
                    characterString += "Legend of the five Rings:" + Environment.NewLine;
                    foreach (var character in lChars)
                    {
                        characterString += "   " + character.Name + " - " + character.Clan + (character.IsCurrent ? " (loaded)" : "") + Environment.NewLine;
                    } 
                }
                characterString += Environment.NewLine + "\u200b```";

                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync("List of playable characters :" + Environment.NewLine + characterString);
            }
            else
            {
                PlayableCharacter character = null;
                try
                {
                    CharacterRepository.Characters.Where(x => x.Player == Context.Message.Author.Mention).ToList().ForEach(x => x.IsCurrent = false);
                    character = CharacterRepository.Find<PlayableCharacter>(Context.Message.Author.Mention, name);
                    character.IsCurrent = true;

                    //await (Context.Message.Author as IGuildUser).ModifyAsync(x => x.Nickname = character.Name);
                }
                catch (InvalidOperationException ex)
                {
                    await Context.Channel.SendMessageAsync("Error 404: Character not found!");
                    throw;
                }
                //catch (Exception)
                //{
                //    await Context.Channel.SendMessageAsync("Nickname could not be changed for " + Context.Message.Author.Mention);
                //}

                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync($"Character : {character.Name} successfully loaded !");

            }
        }

        [Command("!c delete")]
        [Summary("Supprime définitvement un personnage!")]
        public async Task DeleteCharacter(params string[] s)
        {
            _ = Context.Message.DeleteAsync();
            string expr = string.Join(' ', s);
            PlayableCharacter character = CharacterRepository.Find<PlayableCharacter>(Context.Message.Author.Mention, expr);
            if (character == null)
            {
                await Context.Channel.SendMessageAsync("Error 404: Character not found or currently loaded!");
                return;
            }

            try
            {
                CharacterRepository.DeleteExcelCharacter(character);
            }
            catch (Exception ex)
            {
                await Context.Channel.SendMessageAsync($"Erreur lors de la suppression du fichier.{Environment.NewLine}{ex.Message}");
                throw;
            }
            await Context.Channel.SendMessageAsync("Suppression effectuée.");
        }

        [Command("!c image"), Summary("Upload une image pour le personnage chargé")]
        [Alias("!c img", "!c Image")]
        public async Task SetCharImage()
        {
            PlayableCharacter character = CharacterRepository.FindCurrentByMention<PlayableCharacter>(Context.Message.Author.Mention);
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
                await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} Image updated!");
            }
            else
            {
                _ = Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync("File not found. Please upload an image while using this command.");
            }
        }

        [Command("!c info")]
        [Summary("Informations sur une statistique ou compétence du perso chargé `!c info attaque`")]
        public async Task Info(params string[] s)
        {
            PlayableCharacter character = CharacterRepository.FindCurrentByMention<PlayableCharacter>(Context.Message.Author.Mention);
            if (character == null)
            {
                await Context.Channel.SendMessageAsync("Error 404: Character not found or not loaded!");
                return;
            }

            string stat = string.Join(" ", s).ToLower();
            if (stat == null || stat == string.Empty)
            {
                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync(character.KeywordsHelp());
            }
            else
            {
                RollableStat rollableStat;
                try
                {
                    rollableStat = character.AllStats.FindByRawStat(stat);

                    await Context.Message.DeleteAsync();
                    await Context.Channel.SendMessageAsync(Context.User.Mention + " " + rollableStat.Name + " : " + rollableStat.Value);
                }
                catch (Exception)
                {
                    await Context.Channel.SendMessageAsync("Error 404: Stat not found (" + stat + ")");
                }

            }
        }
    }
}
