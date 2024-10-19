using Discord.Interactions;
using System.Threading.Tasks;
using DiscordBot.Objects;
using TrickOrTreatBot.Objects;
using System;

namespace DiscordBot.Commands
{
    [RegisterToGuilds]
    public class RegisterCmd : InteractionModuleBase<SocketInteractionContext>
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
                Utils.Log(ex);
            }
            await RespondAsync($"Set your costume to '{name}'", ephemeral: true);
        }
    }
}