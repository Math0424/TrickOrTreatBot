using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TrickOrTreatBot.Objects
{
    public class Config
    {
        public List<ulong> ValidChannels = new List<ulong>();
        public bool AddAllowed;
        public bool Drops;
        public int Chance;
    }

    public struct User
    {
        public ulong DiscordId;
        public string Character;
    }

    public struct InventoryItem
    {
        ulong OwnerId;
        int ItemId;
    }

    public struct ShopKeeper
    {
        public string ImageFile;
        public string Name;
        public string FlavorText;
        public ulong CreatorId;
    }

    public struct Item
    {
        public int ItemId;
        public string Name;
        public Rarity Rarity;
        public string ImageFile;
        public string FlavorText;
        public ulong CreatorId;
    }

    public enum Rarity
    {
        Common = 1,
        Rare = 3,
        Epic = 7,
        Mythic = 10,
    }

    public enum ClaimStatus
    {
        Incorrect,
        NothingToClaim,
        AlreadyClaimed,
        Claimed,
    }

    public class SQLIgnore : Attribute { }

    public class Drop
    {
        public RestUserMessage Message;
        public ShopKeeper Shopkeeper;
        public bool Trick;

        public ulong InteractUser;
        public bool Claimed = false;
        public bool Failed = false;
        public int TimeRemaining = 13;
    }

}
