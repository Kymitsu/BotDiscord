using BotDiscord.RPG;
using BotDiscord.Services;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotDiscord
{
    public class CharacterLoadedPermission : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            var result = await Task.Run(() => 
            {
                var character = services.GetRequiredService<CharacterService>().FindCurrentByMention<PlayableCharacter>(context.User.Mention);

                if (character == null)
                {
                    return PreconditionResult.FromError("Aucun personnnage chargé");
                }
                else
                {
                    return PreconditionResult.FromSuccess();
                }
            });

            return result;
        }

        
    }
}
