using Discord.Interactions;
using System.Threading.Tasks;
using DiscordBot.Objects;
using DiscordBot.Services;
using Discord;
using System;

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
                case "toggle":
                    bool enabled = !bool.Parse(Storage.GetConfig("drops", "false"));
                    Storage.SetConfig("drops", enabled.ToString());
                    await RespondAsync($"Set drops to {(enabled ? "true" : "false")}", ephemeral: true);
                    return;
                case "spawn":
                    await _trick.SpawnDrop(Context.Channel.Id);
                    await RespondAsync("Spawned", ephemeral: true);
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
                    await RespondAsync($"removeitem [name]\nremoveshopkeeper [name]\ntoggle (toggle the bot drops)\nspawn (spawn a drop in this channel)\ndrops (enable/disable drops in this channel)\nrate [n] (1/n % chance to drop a drop)", ephemeral: true);
                    return;
            }
            await RespondAsync($"Unknown, use 'help'", ephemeral: true);
        }


    }
}
