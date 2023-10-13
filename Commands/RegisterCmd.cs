using Discord.Interactions;
using System.Threading.Tasks;
using DiscordBot.Objects;
using TrickOrTreatBot.Objects;
using System;

namespace DiscordBot.Commands
{
    public class RegisterCmd : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("costume", "Set your favorite league character as your costume")]
        public async Task Costume([Summary("champion", "Your FAVORITE league champion")] string name)
        {
            foreach(var x in Utils.champions)
            {
                if (name.ToLower().Equals(x.ToLower()))
                {
                    User u = new User()
                    {
                        DiscordId = Context.User.Id,
                        Character = x
                    };
                    try
                    {
                        Storage.AddUser(u);
                    }
                    catch(Exception ex)
                    {
                        Utils.Log(ex);
                    }
                    await RespondAsync($"Set your favorite champion to '{x}'", ephemeral: true);
                    return;
                }
            }
            await RespondAsync($"Unknown champion '{name}'", ephemeral: true);
        }
    }
}