using Discord;
using Discord.Interactions;
using DiscordBot.Objects;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TrickOrTreatBot.Objects;

namespace TrickOrTreatBot.Commands
{
    public class CreateCmd : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("createitem", "Add a new item to get")]
        public async Task CreateItem(string name, string flavorText, Rarity rarity, string ImageURL = null, Attachment attachment = null)
        {
            if (ImageURL != null && attachment != null)
            {
                await RespondAsync($"Please choose either an image upload or url upload.", ephemeral: true);
                return;
            }

            string fileName = null;
            if (ImageURL != null)
            {
                if (!await ValidateImageURL(ImageURL))
                    return;
                fileName = await Utils.DownloadFile(ImageURL);
            }
            else if (attachment != null)
            {
                if (!attachment.ContentType.StartsWith("image/"))
                {
                    await RespondAsync($"Must be Image not '{attachment.ContentType}'", ephemeral: true);
                    return;
                }
                fileName = await Utils.DownloadFile(attachment.Url);
            }

            if (Storage.GetItem(name).HasValue)
            {
                await RespondAsync($"Item with {name} already added", ephemeral: true);
                return;
            }

            Item item = new Item()
            {
                Name = name,
                CreatorId = Context.User.Id,
                FlavorText = flavorText,
                Rarity = rarity,
                ImageFile = fileName,
            };
            Storage.AddItem(item);
            Utils.Log($"{Context.User.Username} added item {name}");
            //await RespondAsync(embed: Utils.GenerateItemPreview(item).Build(), ephemeral: true);
            //TODO show image of item
        }

        [SlashCommand("createshopkeeper", "Add a new shopkeeper")]
        public async Task CreateShopkeeper(string name, string flavorText, string ImageURL = null, Attachment attachment = null)
        {
            if ((ImageURL != null && attachment != null) || (attachment == null && ImageURL == null))
            {
                await RespondAsync($"Shopkeeper must have an image, one or more", ephemeral: true);
                return;
            }

            string fileName = null;
            if (ImageURL != null)
            {
                if (!await ValidateImageURL(ImageURL))
                    return;
                fileName = await Utils.DownloadFile(ImageURL);
            }
            else if (attachment != null)
            {
                if (!attachment.ContentType.StartsWith("image/"))
                {
                    await RespondAsync($"Must be Image not '{attachment.ContentType}'", ephemeral: true);
                    return;
                }
                fileName = await Utils.DownloadFile(attachment.Url);
            }

            if (Storage.GetShopkeeper(name).HasValue)
            {
                await RespondAsync($"Shopkeeper with {name} already added", ephemeral: true);
                return;
            }

            ShopKeeper shopkeeper = new ShopKeeper()
            {
                CreatorId = Context.User.Id,
                FlavorText = flavorText,
                ImageFile = fileName,
                Name = name
            };

            Utils.Log($"{Context.User.Username} added shopkeeper {name}");
            Storage.AddShopkeeper(shopkeeper);
            await RespondAsync($"Completed", ephemeral: true);
            //await RespondAsync(embed: Utils.GenerateShopkeeperPreview(shopkeeper).Build(), ephemeral: true);
        }

        private async Task<bool> ValidateImageURL(string imageUrl)
        {
            Uri url = new Uri(imageUrl);
            if (!(url.Host.Equals("cdn.discordapp.com") || url.Host.Equals("media.discordapp.net")))
            {
                await RespondAsync("Not an image from discord servers!", ephemeral: true);
                return false;
            }

            using var client = new HttpClient();
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, imageUrl));
            if (!response.Content.Headers.ContentType.MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                await RespondAsync("Not an image!", ephemeral: true);
                return false;
            }
            return true;
        }

    }
}
