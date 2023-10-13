using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using TrickOrTreatBot.Objects;

namespace DiscordBot.Objects
{
    internal static class Storage
    {
        private static Random rand = new Random();
        private static SQLiteConnection connection;
        private static List<ulong> validChannelCache = new List<ulong>();

        static Storage()
        {
            string databaseLocation = Path.Combine(Utils.GetDataFolder(), "database.db");
            if (!File.Exists(databaseLocation))
                SQLiteConnection.CreateFile(databaseLocation);

            Utils.Log($"Writing data to {databaseLocation}");

            connection = new SQLiteConnection($"Data Source={databaseLocation};Version=3;");
            connection.Open();

            string sql = @"
                CREATE TABLE IF NOT EXISTS Config (
                    Key TEXT PRIMARY KEY,
                    Value TEXT
                )";
            using (var command = new SQLiteCommand(sql, connection)) command.ExecuteNonQuery();

            sql = @"
                CREATE TABLE IF NOT EXISTS Users (
                    DiscordId INTEGER PRIMARY KEY,
                    Character TEXT
                )";
            using (var command = new SQLiteCommand(sql, connection)) command.ExecuteNonQuery();

            sql = @"
                CREATE TABLE IF NOT EXISTS ItemInventory (
                    OwnerId INTEGER REFERENCES Users(DiscordId),
                    ItemId INTEGER REFERENCES Items(ItemId)
                )";
            using (var command = new SQLiteCommand(sql, connection)) command.ExecuteNonQuery();

            sql = @"
                CREATE TABLE IF NOT EXISTS ShopKeepers (
                    ImageFile TEXT,
                    Name TEXT,
                    FlavorText TEXT,
                    CreatorId INTEGER
                )";
            using (var command = new SQLiteCommand(sql, connection)) command.ExecuteNonQuery();

            sql = @"
                CREATE TABLE IF NOT EXISTS Items (
                    ItemId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT,
                    Rarity INTEGER,
                    ImageFile TEXT,
                    CreatorId INTEGER
                )";
            using (var command = new SQLiteCommand(sql, connection)) command.ExecuteNonQuery();

            // update our list from config
            if (GetConfig("validchannels", "") != "")
                validChannelCache = Array.ConvertAll(GetConfig("validchannels", "").Split("-"), ulong.Parse).ToList();
        }

        private static void PushValidChannels()
        {
            string result = string.Join("-", validChannelCache);
            SetConfig("validchannels", result);
        }

        public static List<ulong> GetValidChannels()
        {
            return validChannelCache;
        }

        public static bool ContainsChannel(ulong channel)
        {
            return validChannelCache.Contains(channel);
        }

        public static void AddValidChannel(ulong channel)
        {
            validChannelCache.Add(channel);
            PushValidChannels();
        }

        public static void RemoveValidChannel(ulong channel)
        {
            validChannelCache.Remove(channel);
            PushValidChannels();
        }

        private static string GenerateStructInsertStatement<T>(T obj, SQLiteCommand cmd, string table)
        {
            var fields = obj.GetType().GetFields().Where(f => f.GetCustomAttributes(true).Count(e => e.GetType() == typeof(SQLIgnore)) == 0);
            var fieldNames = fields.Select(f => f.Name).ToArray();
            var paramNames = fields.Select(f => "@" + f.Name).ToArray();

            string sql = $"INSERT INTO {table} ({string.Join(", ", fieldNames)}) VALUES ({string.Join(", ", paramNames)})";

            foreach (var f in fields)
                cmd.Parameters.AddWithValue("@" + f.Name, f.GetValue(obj));

            return sql;
        }

        private static T PopulateFromReader<T>(SQLiteDataReader reader) where T : new()
        {
            T result = new T();
            Type type = typeof(T);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string fieldName = reader.GetName(i);
                FieldInfo field = type.GetField(fieldName);

                if (field != null && !reader.IsDBNull(i))
                {
                    object value = reader.GetValue(i);
                    field.SetValue(result, Convert.ChangeType(value, field.FieldType));
                }
                else
                {
                    Utils.Log($"Cannot get '{fieldName}' from struct type '{type}'");
                }
            }
            return result;
        }

        public static void AddItem(Item item)
        {
            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = GenerateStructInsertStatement(item, command, "Items");
                command.ExecuteNonQuery();
            }
        }

        public static void RemoveItem(string name)
        {
            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = $"DELETE FROM Items WHERE Name = @Name;";
                command.Parameters.AddWithValue("@Name", name);
                command.ExecuteNonQuery();
            }
        }

        public static void AddShopkeeper(ShopKeeper keeper)
        {
            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = GenerateStructInsertStatement(keeper, command, "ShopKeepers");
                command.ExecuteNonQuery();
            }
        }

        public static void RemoveShopkeeper(string name) 
        {
            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = $"DELETE FROM ShopKeepers WHERE Name = '{name}';";
                command.ExecuteNonQuery();
            }
        }

        public static ShopKeeper GetShopkeeper(string name)
        {
            string query = $"SELECT * FROM ShopKeepers WHERE Name COLLATE NOCASE = @Name;";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", name);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return PopulateFromReader<ShopKeeper>(reader);
                    }
                }
            }
            return null;
        }

        public static List<ShopKeeper> GetShopkeepers()
        {
            string query = "SELECT * FROM ShopKeepers;";
            List<ShopKeeper> items = new List<ShopKeeper>();
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(PopulateFromReader<ShopKeeper>(reader));
                    }
                }
            }
            return items;
        }

        public static Item GetRandomItemRarity(Rarity highest)
        {
            List<Item> items = new List<Item>();

            Dictionary<Rarity, int> raities = new Dictionary<Rarity, int> { 
                {Rarity.Common, 15 },
                {Rarity.Rare, 7 }, 
                {Rarity.Epic, 3 }, 
                {Rarity.Mythic, 1 }, 
            };

            string query = "SELECT * FROM Items;";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Item randomItem = PopulateFromReader<Item>(reader);
                        if ((int)randomItem.Rarity < (int)highest)
                        {
                            for (int i = 0; i < raities[(Rarity)randomItem.Rarity]; i++)
                                items.Add(randomItem);
                        }
                    }
                }
            }
            return items[rand.Next(items.Count)];
        }

        public static Item GetItem(string name)
        {
            string query = $"SELECT * FROM Items WHERE Name COLLATE NOCASE = @Name;";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", name);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return PopulateFromReader<Item>(reader);
                    }
                }
            }
            return null;
        }

        public static Item GetItem(int id)
        {
            string query = $"SELECT * FROM Items WHERE ItemId = {id};";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return PopulateFromReader<Item>(reader);
                    }
                }
            }
            return null;
        }

        public static List<Item> GetItems()
        {
            string query = "SELECT * FROM Items;";
            List<Item> items = new List<Item>();
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(PopulateFromReader<Item>(reader));
                    }
                }
            }
            return items;
        }

        public static ShopKeeper GetRandomShopkeeper()
        {
            string query = $"SELECT * FROM ShopKeepers ORDER BY RANDOM() LIMIT 1;";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return PopulateFromReader<ShopKeeper>(reader);
                    }
                }
            }
            return null;
        }

        public static List<Tuple<ulong, int>> GetScores()
        {
            string query = @"
                    SELECT Users.DiscordId, SUM(Items.Rarity) as TotalPoints
                    FROM Users
                    JOIN ItemInventory ON Users.DiscordId = ItemInventory.OwnerId
                    JOIN Items ON ItemInventory.ItemId = Items.ItemId
                    GROUP BY Users.DiscordId
                    ORDER BY TotalPoints DESC";

            List<Tuple<ulong, int>> points = new List<Tuple<ulong, int>>();
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        points.Add(new Tuple<ulong, int>((ulong)reader.GetInt64(0), reader.GetInt32(1)));
                    }
                }
            }
            return points;
        }

        public static int GetScore(ulong discordId)
        {
            string query = $@"
                    SELECT SUM(Items.Rarity) as TotalPoints
                    FROM Users
                    JOIN ItemInventory ON Users.DiscordId = ItemInventory.OwnerId
                    JOIN Items ON ItemInventory.ItemId = Items.ItemId
                    WHERE Users.DiscordId = {discordId}
                    GROUP BY Users.DiscordId";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetInt32(0);
                    }
                }
            }
            return 0;
        }

        public static User GetUser(ulong discordId)
        {
            string query = $"SELECT * FROM Users WHERE DiscordId = {discordId};";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return PopulateFromReader<User>(reader);
                    }
                }
            }
            return null;
        }

        public static void AddUser(User user)
        {
            string query = $@"
                    INSERT OR REPLACE INTO Users (DiscordId, Character)
                    VALUES (@DiscordId, @Character);";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@DiscordId", user.DiscordId);
                command.Parameters.AddWithValue("@Character", user.Character);
                command.ExecuteNonQuery();
            }
        }

        public static string GetConfig(string key, string defaul)
        {
            string query = "SELECT Value FROM Config WHERE Key = @Key;";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Key", key);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        return reader.GetString(0);
                }
            }
            return defaul;
        }

        public static void SetConfig(string key, string value)
        {
            string query = $@"
                    INSERT OR REPLACE INTO Config (Key, Value)
                    VALUES (@Key, @Value);";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Value", value);
                command.Parameters.AddWithValue("@Key", key);
                command.ExecuteNonQuery();
            }
        }

        public static void AddInventoryItem(ulong userId, int itemId)
        {
            string query = $@"
                INSERT INTO ItemInventory (OwnerId, ItemId)
                VALUES ({userId}, {itemId});";
            using (var command = new SQLiteCommand(query, connection)) command.ExecuteNonQuery();
        }

    }
}
