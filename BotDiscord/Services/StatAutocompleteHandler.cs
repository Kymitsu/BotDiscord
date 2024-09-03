using BotDiscord.RPG;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotDiscord.Services
{
    public class StatAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var result = await Task.Run(() =>
            {
                string userInput = (context.Interaction as SocketAutocompleteInteraction).Data.Current.Value.ToString().Trim();

                PlayableCharacter character;
                IEnumerable<RollableStat> test;

                try
                {
                    character = services.GetRequiredService<CharacterService>().FindCurrentByMention<PlayableCharacter>(context.User.Mention);
                    if (character != null)
                    {
                        test = character.AllStats.Where(x => x.Name.StartsWith(userInput, StringComparison.InvariantCultureIgnoreCase));
                        if (!test.Any())
                        {
                            test = character.AllStats.Where(x => x.Name.Contains(userInput, StringComparison.InvariantCultureIgnoreCase));
                        }
                    }
                    else
                    {
                        return AutocompletionResult.FromError(InteractionCommandError.Unsuccessful, "AutoComplete fail: Aucun personnage chargé.");
                    }
                }
                catch (Exception ex)
                {
                    return AutocompletionResult.FromError(ex);
                }

                List<AutocompleteResult> suggestions = new List<AutocompleteResult>();
                foreach (var item in test)
                {
                    suggestions.Add(new AutocompleteResult(item.Name, item.Name));
                }

                return AutocompletionResult.FromSuccess(suggestions.Take(25));
            });

            return result;
        }
    }
}
