using Discord;
using Discord.WebSocket;
using DiscordBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrickOrTreatBot.Objects;

namespace DiscordBot.Services
{
    public class TrickOrTreatModule
    {
        private Random rand = new Random();
        private Dictionary<ulong, Drop> _drops = new Dictionary<ulong, Drop>();

        private readonly DiscordSocketClient _client;

        public string GetName(ulong id)
        {
            var user = _client.GetUser(id);
            return user?.GlobalName ?? user?.Username ?? id.ToString();
        }

        public TrickOrTreatModule(DiscordSocketClient Client)
        {
            _client = Client;
            new Task(() => Loop()).Start();
        }

        private async void Loop()
        {
            while(true)
            {
                Thread.Sleep(5000);
                try
                {
                    foreach (var x in _drops.Values)
                    {
                        x.TimeRemaining -= 1;
                        if (x.TimeRemaining <= 0)
                        {
                            await x.Message.DeleteAsync();
                        }
                    }

                    foreach (var s in _drops.Where(x => x.Value.TimeRemaining <= 0).ToList())
                    {
                        _drops.Remove(s.Key);
                    }

                    if (bool.Parse(Storage.GetConfig("drops", "false")))
                    {
                        foreach (var x in Storage.GetValidChannels())
                        {
                            if (!_drops.ContainsKey(x))
                            {
                                if (rand.Next(0, int.Parse(Storage.GetConfig("chance", "50"))) == 1)
                                {
                                    await SpawnDrop(x);
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    //Utils.Log($"Failed to spawn drop");
                    //Utils.Log(ex.Message);
                }

            }
        }

        public ClaimStatus ClaimDrop(ulong channelID, User user, bool Trick)
        {
            if (!_drops.ContainsKey(channelID))
            {
                return ClaimStatus.NothingToClaim;
            }

            Drop drop = _drops[channelID];

            if (drop.Claimed)
            {
                return ClaimStatus.AlreadyClaimed;
            }

            drop.InteractUser = user.DiscordId;
            drop.Claimed = true;

            if (drop.Trick != Trick)
            {
                drop.Failed = true;
                FailDrop(drop, user);
                return ClaimStatus.Incorrect;
            }

            GetDrop(drop, user);

            return ClaimStatus.Claimed;
        }

        public async Task SpawnDrop(ulong channelID)
        {
            Drop d = new Drop()
            {
                Shopkeeper = Storage.GetRandomShopkeeper().Value,
                Trick = (rand.Next(0, 2) == 0 ? true : false),
            };

            EmbedBuilder builder = new EmbedBuilder();
            builder.Title = "Happy Halloween!";
            //builder.ImageUrl = d.Shopkeeper.ImgURL;
            builder.Description = $"{d.Shopkeeper.Name} has appeared, type **/{(d.Trick ? "trick" : "treat")}** to claim a reward!";
            builder.Footer = new EmbedFooterBuilder();
            builder.Footer.Text = $"\"{d.Shopkeeper.FlavorText}\"";

            var msg = await ((SocketTextChannel)_client.GetChannel(channelID)).SendMessageAsync(embed: builder.Build());
            d.Message = msg;

            _drops.Add(channelID, d);
        }

        private async Task FailDrop(Drop drop, User user)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.Title = "Oh no!";
            builder.Description = $"<@{user.DiscordId}> used the wrong command and scared them off.";
            await drop.Message.ModifyAsync(x => { x.Embed = builder.Build(); });

            drop.TimeRemaining = 4;
        }

        private async Task GetDrop(Drop drop, User user)
        {
            //Item item = Storage.GetRandomItem(user);
            //user.Inventory.Add(item.ItemID);

            //await _core.LogAsync(new LogMessage(LogSeverity.Info, "Bot", $"{GetName(user.DiscordId)} has claimed prize '{item.Name}'"));

            EmbedBuilder builder = new EmbedBuilder();
            builder.Title = "Happy Halloween!";
            //builder.ImageUrl = drop.Shopkeeper.ImgURL;
            //builder.Description = $"{drop.Shopkeeper.Name} liked <@{user.DiscordId}> {user.Character} costume so much they gave them one **{item.Name}**";
            //builder.ThumbnailUrl = item.ImgURL;
            builder.Footer = new EmbedFooterBuilder();
            //builder.Footer.Text = $"This item is of rarity {item.Rarity}, it has been added to your score";

            drop.TimeRemaining = 4;

            await drop.Message.ModifyAsync(x => { x.Embed = builder.Build(); });
        }

    }
}
