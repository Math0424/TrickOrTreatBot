﻿using Discord;
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
                    CreateBottomText(d.Trick)
                    ), "shopkeeper.png");

            d.Message = msg;
            _drops.Add(channelID, d);
        }

        private string CreateBottomText(bool trick)
        {
            switch(rand.Next(12))
            {
                case 0:
                    return $"Dont type /{(trick ? "treat" : "trick")} to claim!";
                case 1:
                    return $"Please dont /{(trick ? "treat" : "trick")} me!";
                case 2:
                    return $"I would like a /{(trick ? "trick" : "treat")}!";
                case 3:
                    return $"Give me a /{(trick ? "trick" : "treat")}!";
                case 4:
                    return $"/{(trick ? "trick" : "treat")} please!";
                case 5:
                    return $"/{(trick ? "trick" : "treat")} not /{(trick ? "treat" : "trick")}";
                case 6:
                    return $"Want to /{(trick ? "trick" : "treat")} me?";
                case 7:
                    return $"Ready for a /{(trick ? "trick" : "treat")}!";
                case 8:
                    return $"No /{(trick ? "treat" : "trick")} please!";
                case 9:
                    return $"Would love a /{(trick ? "trick" : "treat")}!";
                default:
                    return $"type /{(trick ? "trick" : "treat")} to claim!";
            }
        }

        private async Task GetDrop(Drop d, User user)
        {
            d.Message.DeleteAsync();

            var score = Storage.GetScore(user.DiscordId).Item1;
            var highestScore = Storage.GetHighestScore();

            var maxRarity = Rarity.Mythic;
            var minRarity = Rarity.Common;

            float disparity = score / (float)highestScore;
            double randN = rand.NextDouble() - disparity;
            if (randN > 0.9f)
                minRarity = Rarity.Mythic;
            else if (randN > 0.8f || disparity < 0.2)
                minRarity = Rarity.Epic;
            else if (randN > 0.5f)
                minRarity = Rarity.Rare;

            if (disparity > .9)
                maxRarity = Rarity.Common;
            else if(disparity > .8)
                maxRarity = Rarity.Rare;
            else if (disparity > .7)
                maxRarity = Rarity.Epic;

            Item item = Storage.GetRandomItemRarity(minRarity, maxRarity);
            Storage.AddInventoryItem(user.DiscordId, item.ItemId);

            Utils.Log($"{GetName(user.DiscordId)} has claimed prize '{item.Name}'");
            User u = Storage.GetUser(user.DiscordId);

            var msg = await ((SocketTextChannel)d.Message.Channel).SendFileAsync(
                Utils.GetShopkeeperPreview(
                    d.Shopkeeper.ImageFile,
                    $"{d.Shopkeeper.Name}",
                    $"{d.Shopkeeper.FlavorText}",
                    $"{d.Shopkeeper.Name} loved {GetName(u.DiscordId)}'s {u.Character} costume so much they gave you {item.Name}",
                    item
                    ), "prize.png");

            d.Message = msg;
            d.TimeRemaining = 4;
        }

    }
}
