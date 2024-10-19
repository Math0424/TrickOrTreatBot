using Discord.Interactions;
using System.Threading.Tasks;
using DiscordBot.Objects;
using DiscordBot.Services;
using Discord;
using System;
using TrickOrTreatBot.Objects;

namespace DiscordBot.Commands
{
    [RegisterToGuilds]
    public class AdminCmd(TrickOrTreatService _trick) : InteractionModuleBase<SocketInteractionContext>
    {
        private ulong Allowed = 242785518763376641;

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
                case "removeitem":
                    if (args.Length >= 2)
                    {
                        string arg = string.Join(" ", args, 1, args.Length - 1);
                        Storage.RemoveItem(arg);
                        await RespondAsync($"Removed item '{arg}'", ephemeral: true);
                    }
                    break;
                case "removeshopkeeper":
                    if (args.Length >= 2)
                    {
                        string arg = string.Join(" ", args, 1, args.Length - 1);
                        Storage.RemoveShopkeeper(arg);
                        await RespondAsync($"Removed shopkeeper '{arg}'", ephemeral: true);
                    }
                    break;
                case "echo":
                    if (args.Length >= 2)
                    {
                        string arg = string.Join(" ", args, 1, args.Length - 1);
                        await RespondAsync($"Echoing", ephemeral: true);
                        await ((ITextChannel)Context.Channel).SendMessageAsync(arg);
                    }
                    break;
                case "toggle":
                    bool enabled = !bool.Parse(Storage.GetConfig("drops", "false"));
                    Storage.SetConfig("drops", enabled.ToString());
                    await RespondAsync($"Set drops to {(enabled ? "true" : "false")}", ephemeral: true);
                    return;
                case "spawn":
                    await RespondAsync("Spawned", ephemeral: true);
                    await _trick.SpawnDrop(Context.Channel.Id);
                    return;
                case "drops":
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
                case "help":
                    await RespondAsync($"removeitem [name]\n" +
                        $"removeshopkeeper [name]\n" +
                        $"toggle (toggle the bot drops)\n" +
                        $"echo [text] (repeat in this channel)\n" +
                        $"spawn (spawn a drop in this channel)\n" +
                        $"drops (enable/disable drops in this channel)\n" +
                        $"rate [n] (1/n % chance to drop a drop)\n", ephemeral: true);
                    return;
            }
            await RespondAsync($"Unknown, use 'help'", ephemeral: true);
        }


    }
}
