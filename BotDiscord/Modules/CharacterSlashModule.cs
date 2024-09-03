using BotDiscord.RPG;
using BotDiscord.RPG.Anima;
using BotDiscord.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BotDiscord.Modules
{
    [Group("character", "Character")]
    public class CharacterSlashModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly CharacterService _characterService;

        public CharacterSlashModule(CharacterService characterService)
        {
            _characterService = characterService;
        }

        [SlashCommand("upload", "Charge les données d'un personnage depuis une feuille Excel")]
        public async Task Upload([Summary(description:"Fiche de personnage .xls")]IAttachment file)
        {
            if (Path.GetExtension(file.Filename) != ".xlsx")
            {
                await RespondAsync("Mauvais format de fichier. Veuillez uploader une fiche excel.", ephemeral: true);
                return;
            }

            var character = await _characterService.HandleFile(file, Context.User.Mention);

            if (string.IsNullOrWhiteSpace(character.Name))
            {
                await RespondAsync("Nom du personnage manquant. Veuillez renseigner le nom du personnage dans la fiche du excel.", ephemeral: true);
            }
            else
            {
                _characterService.Characters.Where(x => x.Player == Context.User.Mention).ToList().ForEach(x => x.IsCurrent = false);
                character.IsCurrent = true;
                await RespondAsync($"Fiche de personnage {character.Name} uploadé avec succès");
            }
        }


        [SlashCommand("load", "Charge le personnage à jouer")]
        public async Task Load([Summary(description:"Nom du personnage à charger")]string name = "")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                var menuBuilder = new SelectMenuBuilder()
                    .WithPlaceholder("Séléctionner un personnage")
                    .WithCustomId("menu-select-character");

                var characters = _characterService.Characters.Where(x => x.Player == Context.User.Mention);
                foreach (var character in characters)
                {
                    menuBuilder.AddOption($"{character.Name}{(character.IsCurrent ? " - (chargé)" : "")}", character.Name);
                }

                var componentBuilder = new ComponentBuilder().WithSelectMenu(menuBuilder);

                await RespondAsync("", components: componentBuilder.Build(), ephemeral: true);
            }
            else
            {
                PlayableCharacter character = null;
                _characterService.Characters.Where(x => x.Player == Context.User.Mention).ToList().ForEach(x => x.IsCurrent = false);
                character = _characterService.Find<PlayableCharacter>(Context.User.Mention, name);
                if(character == null)
                    await RespondAsync($"Error 404: Character \"{name}\" not found!", ephemeral: true);
                else
                {
                    character.IsCurrent = true;
                    await RespondAsync($"Personnage {character.Name} chargé avec succès.");
                }
            }
        }

        [ComponentInteraction("menu-select-character", true)]
        public async Task CharacterMenu(string[] selectedChar)
        {
            var name = string.Join(", ", selectedChar);
            _characterService.Characters.Where(x => x.Player == Context.User.Mention).ToList().ForEach(x => x.IsCurrent = false);
            PlayableCharacter character = _characterService.Find<PlayableCharacter>(Context.User.Mention, name);
            character.IsCurrent = true;

            await RespondAsync($"Personnage {character.Name} chargé avec succès.");
        }

        [SlashCommand("delete", "Supprime définitivement un personnage")]
        public async Task DeleteCharacter()
        {
            var menuBuilder = new SelectMenuBuilder()
                .WithCustomId("menu-delete-character")
                .WithPlaceholder("Sélectionner un personnage");

            var characters = _characterService.Characters.Where(x => x.Player == Context.User.Mention);
            foreach (var character in characters)
            {
                menuBuilder.AddOption($"{character.Name}{(character.IsCurrent ? " - (chargé)" : "")}", character.Name);
            }

            var componentBuilder = new ComponentBuilder().WithSelectMenu(menuBuilder);

            await RespondAsync("Supprimer définitivement un personnage?", components: componentBuilder.Build(), ephemeral: true);
        }

        [ComponentInteraction("menu-delete-character", true)]
        public async Task DeleteMenu(string[] selectedChar)
        {
            await DeferAsync(true);
            var name = string.Join(", ", selectedChar);

            var character = _characterService.Find<PlayableCharacter>(Context.User.Mention, name);

            _characterService.DeleteExcelCharacter(character);

            await ModifyOriginalResponseAsync(x =>
            {
                x.Content = $"Personnage {name} supprimé.";
                x.Components = null;
            });

        }

        
        [CharacterLoadedPermission]
        [SlashCommand("roll", "Lance les dés pour la stat passée en paramètre")]
        public async Task Roll([Summary(description:"Stat à roll"), Autocomplete(typeof(StatAutocompleteHandler))] string stat, [Summary(description:"Bonus")] int bonus = 0)
        {
            await DeferAsync();

            AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(Context.User.Mention);

            await DeleteOriginalResponseAsync();
            await Context.Channel.SendMessageAsync($"{Context.User.Mention} {character.Roll(stat, bonus)}");
        }

        [CharacterLoadedPermission]
        [SlashCommand("summary", "Affiche les stats du personnage chargé")]
        public async Task Summary()
        {
            await DeferAsync(true);

            AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(Context.User.Mention);

            var embedBuilder = new EmbedBuilder()
                .WithTitle(character.Name)
                .WithDescription($"{character.Class} - LVL {character.Level}")
                .WithThumbnailUrl(character.ImageUrl);

            embedBuilder.AddField("Status​",
                $"Hp : {character.CurrentHp}/{character.Hp}{Environment.NewLine}" +
                $"Fatigue : {character.CurrentFatigue}/{character.Fatigue}{Environment.NewLine}" +
                $"Points de Ki : {character.CurrentKi}/{character.TotalKiPoints}{Environment.NewLine}" +
                $"Zéon : {character.CurrentZeon}/{character.ZeonPoints}{Environment.NewLine}" +
                $"Ppp libres : {character.CurrentPpp}/{character.PppFree}{Environment.NewLine}",
                true);

            embedBuilder.AddField("Général​",
                $"Mouvement : {character.Movement}{Environment.NewLine}" +
                $"Régénération : {character.Regeneration}{Environment.NewLine}" +
                $"Port d'armure : {character.ArmorPoint}{Environment.NewLine}"
                , true);

            embedBuilder.AddField("\u200b", "\u200b", true);//field vide pour que ça soit plus jolie

            var temp = new StringBuilder();
            foreach (var stat in character.BaseStats)
            {
                temp.AppendLine($"{stat.Name} : {stat.Value}");
            }
            embedBuilder.AddField("Caractéristique​", temp.ToString(), true);

            temp = new StringBuilder();
            foreach (var stat in character.Resistances)
            {
                temp.AppendLine($"{stat.Name} : {stat.Value}");
            }
            embedBuilder.AddField("Résistance​", temp.ToString(), true);

            temp = new StringBuilder();
            foreach (var stat in character.BattleStats)
            {
                if (!stat.Name.StartsWith("défense", StringComparison.InvariantCultureIgnoreCase))
                    temp.AppendLine($"{stat.Name} : {stat.Value}");
            }
            embedBuilder.AddField("Champ Principal", temp.ToString(), true);


            var test = character.SecondaryStats.Chunk((int)Math.Ceiling(character.SecondaryStats.Count / 3d));
            bool isFirstField = true;
            foreach (var chunk in test)
            {
                temp = new StringBuilder();
                foreach (var stat in chunk)
                {
                    temp.AppendLine($"{stat.Name} : {stat.Value}");
                }
                if (isFirstField)
                {
                    embedBuilder.AddField("Champs Secondaire", temp.ToString(), true);
                    isFirstField = false;
                }
                else
                    embedBuilder.AddField("\u200b", temp.ToString(), true);

            }
            //string.Format("{0}                         {1}", "Caractéristique​", "\u200b​")

            await ModifyOriginalResponseAsync(x =>
            {
                x.Content = $"";
                x.Embed = embedBuilder.Build();
            });
        }

        [CharacterLoadedPermission]
        [SlashCommand("image", "Upload une image pour le personnage chargé")]
        public async Task UploadImage([Summary(description:"Image du personnage (.jpg, .png, .webp)")] IAttachment img)
        {
            PlayableCharacter character = _characterService.FindCurrentByMention<PlayableCharacter>(Context.User.Mention);

            character.ImageUrl = img.Url;

            await RespondAsync("Image saved!", ephemeral:true);
        }


        [Group("status", "Status")]
        public class StatusGroupModule : InteractionModuleBase<SocketInteractionContext>
        {
            private readonly CharacterService _characterService;

            public StatusGroupModule(CharacterService characterService)
            {
                _characterService = characterService;
            }

            [CharacterLoadedPermission]
            [SlashCommand("show", "Affiche le status de ton personnage")]
            public async Task Show()
            {
                await DeferAsync(true);

                AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(Context.User.Mention);

                var embedBuilder = new EmbedBuilder()
                    .WithTitle(character.Name)
                    .WithThumbnailUrl(character.ImageUrl)
                    .AddField("Hp", $"{character.CurrentHp}/{character.Hp}", true)
                    .AddField("Fatigue", $"{character.CurrentFatigue}/{character.Fatigue}", true)
                    .AddField("Points de Ki", $"{character.CurrentKi}/{character.TotalKiPoints}", true)
                    .AddField("Zéon", $"{character.CurrentZeon}/{character.ZeonPoints}", true)
                    .AddField("Ppp libres", $"{character.CurrentPpp}/{character.PppFree}", true);

                await ModifyOriginalResponseAsync(x =>
                {
                    x.Content = $"";
                    x.Embed = embedBuilder.Build();
                });
            }

            [CharacterLoadedPermission]
            [SlashCommand("reset", "Réinitialise les stats du personnage")]
            public async Task Reset()
            {
                AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(Context.User.Mention);

                character.CurrentHp = character.Hp;
                character.CurrentFatigue = character.Fatigue;
                character.CurrentZeon = character.ZeonPoints;
                character.CurrentPpp = character.PppFree;
                character.CurrentKi = character.TotalKiPoints;

                await RespondAsync($"{Context.User.Mention} Stats de {character.Name} réinitialisé", ephemeral: true);
            }

            [CharacterLoadedPermission]
            [SlashCommand("hp", "Définit les HP actuels du personnage")]
            public async Task SetHp([Summary(description: "Valeur ou bonus (80, +10, -5)")] string value)
            {
                AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(Context.User.Mention);

                if(value.Contains('+') || value.Contains("-"))
                    character.CurrentHp += Convert.ToInt32(value);
                else
                    character.CurrentHp = Convert.ToInt32(value);

                await RespondAsync($"{Context.User.Mention} {character.Name} Hp : {character.CurrentHp}/{character.Hp}", ephemeral: true);

            }

            [CharacterLoadedPermission]
            [SlashCommand("fatigue", "Définit les points de fatigue actuels du personnage")]
            public async Task SetFatigue([Summary(description: "Valeur ou bonus (6, -1)")] string value)
            {
                AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(Context.User.Mention);

                if (value.Contains('+') || value.Contains("-"))
                    character.CurrentFatigue += Convert.ToInt32(value);
                else
                    character.CurrentFatigue = Convert.ToInt32(value);

                await RespondAsync($"{Context.User.Mention} {character.Name} Fatigue : {character.CurrentFatigue}/{character.Fatigue}", ephemeral: true);
            }

            [CharacterLoadedPermission]
            [SlashCommand("zéon", "Définit le Zéon actuel du personnage")]
            public async Task SetZeon([Summary(description: "Valeur ou bonus (400, -50)")] string value)
            {
                AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(Context.User.Mention);

                if (value.Contains('+') || value.Contains("-"))
                    character.CurrentZeon += Convert.ToInt32(value);
                else
                    character.CurrentZeon = Convert.ToInt32(value);

                await RespondAsync($"{Context.User.Mention} {character.Name} Zéon : {character.CurrentZeon}/{character.ZeonPoints}", ephemeral: true);
            }

            [CharacterLoadedPermission]
            [SlashCommand("ppp", "Définit les PPP actuels du personnage")]
            public async Task SetPpp([Summary(description: "Valeur ou bonus (4, +1)")] string value)
            {
                AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(Context.User.Mention);

                if (value.Contains('+') || value.Contains("-"))
                    character.CurrentPpp += Convert.ToInt32(value);
                else
                    character.CurrentPpp = Convert.ToInt32(value);

                await RespondAsync($"{Context.User.Mention} {character.Name} Ppp : {character.CurrentPpp}/{character.PppFree}", ephemeral: true);
            }

            [CharacterLoadedPermission]
            [SlashCommand("ki", "Définit le Ki actuel du personnage")]
            public async Task SetKi([Summary(description: "Valeur ou bonus (80, +10, -5)")] string value)
            {
                AnimaCharacter character = _characterService.FindCurrentByMention<AnimaCharacter>(Context.User.Mention);

                if (value.Contains('+') || value.Contains("-"))
                    character.CurrentKi += Convert.ToInt32(value);
                else
                    character.CurrentKi = Convert.ToInt32(value);

                await RespondAsync($"{Context.User.Mention} {character.Name} Ki : {character.CurrentKi}/{character.TotalKiPoints}", ephemeral: true);
            }
        }
    }
}
