using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrickOrTreatBot.Objects;

namespace TrickOrTreatBot.Commands
{
    public class InventoryCmd : InteractionModuleBase<SocketInteractionContext>
    {
        DiscordSocketClient client;

        public InventoryCmd(DiscordSocketClient client)
        {
            this.client = client;
        }

        public string GetName(ulong id)
        {
            var user = client.GetUser(id);
            return user?.GlobalName ?? user?.Username ?? id.ToString();
        }

        [SlashCommand("inventory", "My Inventory")]
        public async Task Inventory()
        {
            var items = Storage.GetInventory(Context.User.Id).GroupBy(item => item.ItemId)
                       .Select(group => (Item: group.First(), Count: group.Count()))
                       .ToList();

            EmbedBuilder builder = new EmbedBuilder();
            builder.Title = "Your Inventory";
            string value = "";
            foreach (var x in items)
            {
                value += $"'{x.Item.Name}' ({(Rarity)x.Item.Rarity}) - {x.Count}\n";
            }
            builder.Description = value;
            await RespondAsync(embed: builder.Build(), ephemeral: true);
        }

    }
}
