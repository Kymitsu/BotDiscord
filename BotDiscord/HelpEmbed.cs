﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotDiscord
{
    public class HelpEmbed
    {
        public int CurrentPage { get; set; }
        public Dictionary<int, EmbedBuilder> Pages { get; set; }

        public HelpEmbed(IEnumerable<ModuleInfo> modules)
        {
            CurrentPage = 1;
            Pages = new Dictionary<int, EmbedBuilder>();

            EmbedBuilder page1 = new EmbedBuilder();
            page1.WithTitle("Catégories");
            page1.AddField("Général", $"Commandes générales{Environment.NewLine}\u200b", true);
            page1.AddField("Personnages", $"Commandes pour la gestion des feuilles de personnage{Environment.NewLine}\u200b", true);
            page1.AddField("Anima", $"Commandes pour le jeu de rôle Anima{Environment.NewLine}\u200b", true);
            page1.AddField("L5R", $"Commandes pour le jeu de rôle Legends of the five Rings{Environment.NewLine}\u200b", true);
            page1.AddField("Audio", $"Lancer des sons et de la musique. `Ne fonctionne plus`{Environment.NewLine}\u200b", true);
            page1.AddField("\u200b", "\u200b", true);//field vide pour que ça soit plus jolie
            page1.Color = Color.DarkTeal;
            page1.WithFooter("page 1/6");

            Pages.Add(1, page1);

            //Surement possible de faire mieux avec un foreach, mais il faut trouver un moyen de garder un ordre spécifique
            EmbedBuilder page2 = new EmbedBuilder();
            page2.WithTitle("Général");
            page2.WithDescription("Commandes générales");
            var commands = modules.First(x => x.Name == "Module").Commands;
            foreach (CommandInfo command in commands)
            {
                string fieldValue = command.Summary ?? "no description";
                fieldValue += $"{Environment.NewLine}\u200b";
                page2.AddField(command.Name, fieldValue, true);
            }
            if(commands.Count % 3 != 0)
                page2.AddField("\u200b", "\u200b", true);
            page2.Color = Color.DarkTeal;
            page2.WithFooter("page 2/6");

            Pages.Add(2, page2);

            EmbedBuilder page3 = new EmbedBuilder();
            page3.WithTitle("Personnages");
            page3.WithDescription("Commandes pour la gestion des feuilles de personnage");
            var charCommands = modules.First(x => x.Name == "CharacterModule").Commands;
            foreach (CommandInfo command in charCommands)
            {
                string fieldValue = command.Summary ?? "no description";
                fieldValue += $"{Environment.NewLine}\u200b";
                page3.AddField(command.Name, fieldValue, true);
            }
            if (charCommands.Count % 3 != 0)
                page3.AddField("\u200b", "\u200b", true);
            page3.Color = Color.DarkTeal;
            page3.WithFooter("page 3/6");

            Pages.Add(3, page3);

            EmbedBuilder page4 = new EmbedBuilder();
            page4.WithTitle("Anima");
            page4.WithDescription("Commandes pour le jeu de rôle Anima");
            var animaCommands = modules.First(x => x.Name == "AnimaModule").Commands;
            foreach (CommandInfo command in modules.First(x => x.Name == "AnimaModule").Commands)
            {
                string fieldValue = command.Summary ?? "no description";
                fieldValue += $"{Environment.NewLine}\u200b";
                page4.AddField(command.Name, fieldValue, true);
            }
            if (animaCommands.Count % 3 != 0)
                page4.AddField("\u200b", "\u200b", true);
            page4.Color = Color.DarkTeal;
            page4.WithFooter("page 4/6");

            Pages.Add(4, page4);

            EmbedBuilder page5 = new EmbedBuilder();
            page5.WithTitle("L5R");
            page5.WithDescription("Commandes pour le jeu de rôle Legends of the five Rings");
            var lCommands = modules.First(x => x.Name == "L5RModule").Commands;
            foreach (CommandInfo command in lCommands)
            {
                string fieldValue = command.Summary ?? "no description";
                fieldValue += $"{Environment.NewLine}\u200b";
                page5.AddField(command.Name, fieldValue, true);
            }
            if (lCommands.Count % 3 != 0)
                page5.AddField("\u200b", "\u200b", true);
            page5.Color = Color.DarkTeal;
            page5.WithFooter("page 5/6");

            Pages.Add(5, page5);

            EmbedBuilder page6 = new EmbedBuilder();
            page6.WithTitle("Audio");
            page6.WithDescription("Lancer des sons et de la musique. `Ne fonctionne plus`");
            var audioCommands = modules.First(x => x.Name == "AudioModule").Commands;
            foreach (CommandInfo command in modules.First(x => x.Name == "AudioModule").Commands)
            {
                string fieldValue = command.Summary ?? "no description";
                fieldValue += $"{Environment.NewLine}\u200b";
                page6.AddField(command.Name, fieldValue, true);
            }
            if (audioCommands.Count % 3 != 0)
                page6.AddField("\u200b", "\u200b", true);
            page6.Color = Color.DarkTeal;
            page6.WithFooter("page 6/6");

            Pages.Add(6, page6);
        }

        public EmbedBuilder GetCurrentPage()
        {
            return Pages[CurrentPage];
        }

        public EmbedBuilder GetNextPage()
        {
            if (CurrentPage < Pages.Count)
                CurrentPage++;

            return Pages[CurrentPage];
        }

        public EmbedBuilder GetPreviousPage()
        {
            if (CurrentPage != 1)
                CurrentPage--;

            return Pages[CurrentPage];
        }
    }
}
