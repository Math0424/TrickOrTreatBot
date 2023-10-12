using Discord.Interactions;
using System.Threading.Tasks;
using DiscordBot.Objects;
using DiscordBot.Services;
using Discord;

namespace DiscordBot.Commands
{
    public class AdminCmd : InteractionModuleBase<SocketInteractionContext>
    {
        private ulong Allowed = 242785518763376641;

        private readonly TrickOrTreatModule _trick;

        public AdminCmd(TrickOrTreatModule trick)
        {
            this._trick = trick;
        }

        [RequireOwner]
        [SlashCommand("admin", "Admin only")]
        public async Task Toggle(string value)
        {
            if (Context.User.Id != Allowed)
            {
                await RespondAsync("Not allowed", ephemeral: true);
                return;
            }

            string[] args = value.Split(" ");
            switch (args[0])
            {
                case "enabled":
                    bool enabled = !bool.Parse(Storage.GetConfig("drops", "false"));
                    Storage.SetConfig("drops", enabled.ToString());
                    await RespondAsync($"Toggled {value} to {(enabled ? "true" : "false")}", ephemeral: true);
                    return;
                case "spawn":
                    await _trick.SpawnDrop(Context.Channel.Id);
                    await RespondAsync("Spawned", ephemeral: true);
                    return;
                case "echo":
                    await RespondAsync("Echoed", ephemeral: true);
                    string msg = "";
                    for (int i = 1; i < args.Length; i++)
                    {
                        msg += args[i] + " ";
                    }
                    await ((ITextChannel)Context.Channel).SendMessageAsync(msg);
                    return;
                case "channeldrops":
                    if (Storage.ContainsChannel(Context.Channel.Id))
                    {
                        Storage.RemoveValidChannel(Context.Channel.Id);
                        await RespondAsync($"Removed channel from drop pool", ephemeral: true);
                    }
                    else
                    {
                        Storage.AddValidChannel(Context.Channel.Id);
                        await RespondAsync($"Added channel to drop pool", ephemeral: true);
                    }
                    return;
                case "rate":
                    if (args.Length == 2)
                    {
                        if (int.TryParse(args[1], out int x))
                        {
                            Storage.SetConfig("chance", x.ToString());
                            await RespondAsync($"Set drop rate to 1/{x}", ephemeral: true);
                            return;
                        }
                    }
                    break;
                default:
                    await RespondAsync($"Unknown", ephemeral: true);
                    return;
            }

        }


    }
}
