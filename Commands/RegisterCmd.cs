using Discord.Interactions;
using System.Threading.Tasks;
using DiscordBot.Objects;
using TrickOrTreatBot.Objects;
using System;
using DiscordBot.Services;

namespace DiscordBot.Commands
{
    [RegisterToGuilds]
    public class RegisterCmd(ILogger<RegisterCmd> logger) : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("costume", "What costume are you wearing?")]
        public async Task Costume([Summary("costume", "who are you going as")] string name)
        {
            if (name.Length > 15)
                name = name.Substring(0, 15);

            User u = new User()
            {
                DiscordId = Context.User.Id,
                Character = name,
            };
            try
            {
                Storage.AddUser(u);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "error setting user {}", Context.User);
            }
            await RespondAsync($"Set your costume to '{name}'", ephemeral: true);
        }
    }
}