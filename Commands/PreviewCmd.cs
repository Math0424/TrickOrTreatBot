using Discord;
using Discord.Interactions;
using DiscordBot.Objects;
using System.Threading.Tasks;
using TrickOrTreatBot.Objects;

namespace TrickOrTreatBot.Commands
{
    public class PreviewCmd : InteractionModuleBase<SocketInteractionContext>
    {

        [SlashCommand("preview", "preview a item or shopkeeper")]
        public async Task Preview([Choice("Shopkeeper", "sk"), Choice("Item", "it")] string type, string name)
        {
            //switch (type)
            //{
            //    case "sk":
            //        ShopKeeper? keeper = Storage.GetShopkeeper(name);
            //        if (keeper.HasValue)
            //        {
            //            await RespondAsync(embed: Utils.GenerateShopkeeperPreview(keeper.Value).Build());
            //            return;
            //        }
            //        await RespondAsync($"Unknown shopkeeper {name}", ephemeral:true);
            //        break;
            //    case "it":
            //        Item? item = Storage.GetItem(name);
            //        if (item.HasValue)
            //        {
            //            await RespondAsync(embed: Utils.GenerateItemPreview(item.Value).Build());
            //            return;
            //        }
            //        await RespondAsync($"Unknown item {name}", ephemeral: true);
            //        break;
            //}
        }

        [SlashCommand("list", "list items or shopkeepers")]
        public async Task List([Choice("Shopkeeper", "sk"), Choice("Item", "it")] string type)
        {
            EmbedBuilder builder = new EmbedBuilder();
            switch (type)
            {
                case "sk":
                    builder.Title = $"Shopkeepers ({Storage.GetShopkeepers().Count})";
                    foreach (var x in Storage.GetShopkeepers())
                    {
                        builder.Description += $"- **{x.Name}**\n";
                    }
                    break;
                case "it":
                    builder.Title = $"Items ({Storage.GetItems().Count})";
                    foreach (var x in Storage.GetItems())
                    {
                        builder.Description += $"- **{x.Name}** : {x.Rarity}\n";
                    }
                    break;
            }
            await RespondAsync(embed: builder.Build(), ephemeral: true);
        }

    }
}
