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
            switch (type)
            {
                case "sk":
                    ShopKeeper keeper = Storage.GetShopkeeper(name);
                    if (keeper != null)
                    {
                        await DeferAsync(true);
                        await FollowupWithFileAsync(Utils.GetShopkeeperPreview(keeper.ImageFile, keeper.Name, $"'{keeper.FlavorText}'", null), "card.png", ephemeral: true);
                        return;
                    }
                    await RespondAsync($"Unknown shopkeeper {name}", ephemeral:true);
                    break;
                case "it":
                    Item item = Storage.GetItem(name);
                    if (item != null)
                    {
                        await DeferAsync(true);
                        await FollowupWithFileAsync(Utils.GetItemPreview(item.ImageFile, item.Name, (Rarity)item.Rarity), "item.png", ephemeral: true);
                        return;
                    }
                    await RespondAsync($"Unknown item {name}", ephemeral: true);
                    break;
            }
        }

        [SlashCommand("list", "list items or shopkeepers")]
        public async Task List([Choice("Shopkeeper", "sk"), Choice("Item", "it")] string type)
        {
            EmbedBuilder builder = new EmbedBuilder();
            switch (type)
            {
                case "sk":
                    var shopkeepers = Storage.GetShopkeepers();
                    builder.Title = $"Shopkeepers ({shopkeepers.Count})";
                    foreach (var x in shopkeepers)
                    {
                        builder.Description += $"- **{x.Name}**  -  '{x.FlavorText}'\n";
                    }
                    break;
                case "it":
                    var items = Storage.GetItems();
                    builder.Title = $"Items ({items.Count})";
                    foreach (var x in items)
                    {
                        builder.Description += $"- **{x.Name}** ({x.ItemId}) : {(Rarity)x.Rarity}\n";
                    }
                    break;
            }
            await RespondAsync(embed: builder.Build(), ephemeral: true);
        }

    }
}
