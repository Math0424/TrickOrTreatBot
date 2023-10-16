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
                    Utils.Log($"Failed to spawn drop");
                    Utils.Log(ex.Message);
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

            if (drop.FailedUsers.Contains(user.DiscordId))
            {
                return ClaimStatus.AlreadyFailed;
            }

            if (drop.Trick != Trick)
            {
                drop.FailedUsers.Add(user.DiscordId);
                drop.TimeRemaining += 4;
                Storage.RemoveRandomItem(user.DiscordId);
                return ClaimStatus.Incorrect;
            }

            drop.InteractUser = user.DiscordId;
            drop.Claimed = true;

            GetDrop(drop, user);
            return ClaimStatus.Claimed;
        }

        public async Task SpawnDrop(ulong channelID)
        {
            Utils.Log($"Spawning drop at {channelID}");
            
            Drop d = new Drop()
            {
                Shopkeeper = Storage.GetRandomShopkeeper(),
                Trick = rand.NextDouble() > .5,
            };

            var msg = await ((SocketTextChannel)_client.GetChannel(channelID)).SendFileAsync(
                Utils.GetShopkeeperPreview(
                    d.Shopkeeper.ImageFile,
                    $"Happy halloween!\n{d.Shopkeeper.Name} has appeared",
                    $"'{d.Shopkeeper.FlavorText}'",
                    $"type /{(d.Trick ? "trick" : "treat")} to claim!"
                    ), "shopkeeper.png");

            d.Message = msg;
            _drops.Add(channelID, d);
        }

        private async Task GetDrop(Drop d, User user)
        {
            d.Message.DeleteAsync();

            var rank = Storage.GetScore(user.DiscordId).Item2 + 1;

            var maxRarity = Rarity.Mythic;
            var minRarity = Rarity.Common;
            if (rank != -1)
            {
                if (rank <= 2)
                    maxRarity = Rarity.Common;
                else if (rank <= 3)
                    maxRarity = Rarity.Rare;
                else if (rank <= 7)
                    maxRarity = Rarity.Epic;
                else
                    minRarity = rand.NextDouble() > .5 ? Rarity.Rare : Rarity.Common;
            }

            Item item = Storage.GetRandomItemRarity(minRarity, maxRarity);
            Storage.AddInventoryItem(user.DiscordId, item.ItemId);

            Utils.Log($"{GetName(user.DiscordId)} has claimed prize '{item.Name}'");
            User u = Storage.GetUser(user.DiscordId);

            var msg = await ((SocketTextChannel)d.Message.Channel).SendFileAsync(
                Utils.GetShopkeeperPreview(
                    d.Shopkeeper.ImageFile,
                    $"{d.Shopkeeper.Name}",
                    $"{d.Shopkeeper.FlavorText}",
                    $"{d.Shopkeeper.Name} loved your {u.Character} costume so much they gave you {item.Name}",
                    item
                    ), "prize.png");

            d.Message = msg;
            d.TimeRemaining = 4;
        }

    }
}
