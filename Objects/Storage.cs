﻿using Discord.WebSocket;
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

        static Storage()
        {
            string databaseLocation = Path.Combine(Utils.GetDataFolder(), "database.db");
            if (!File.Exists(databaseLocation))
                SQLiteConnection.CreateFile(databaseLocation);

            connection = new SQLiteConnection($"Data Source={databaseLocation};Version=3;");
            connection.Open();

            string sql = @"
                CREATE TABLE IF NOT EXISTS Config (
                    Value STRING,
                    Key STRING
                )";
            using (var command = new SQLiteCommand(sql, connection)) command.ExecuteNonQuery();

            sql = @"
                CREATE TABLE IF NOT EXISTS Users (
                    DiscordId INTEGER PRIMARY KEY,
                    Character STRING
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
                    CreatorId INTEGER REFERENCES Users(DiscordId)
                )";
            using (var command = new SQLiteCommand(sql, connection)) command.ExecuteNonQuery();

            sql = @"
                CREATE TABLE IF NOT EXISTS Items (
                    ItemId INTEGER PRIMARY KEY AUTOINCREMENTS,
                    Name TEXT,
                    Rarity INTEGER,
                    ImageFile TEXT,
                    FlavorText TEXT,
                    CreatorId INTEGER REFERENCES Users(DiscordId)
                )";
            using (var command = new SQLiteCommand(sql, connection)) command.ExecuteNonQuery();

        }

        private static string GenerateInsertStatement(object obj, SQLiteCommand cmd, string table)
        {
            var properties = obj.GetType().GetProperties().Where(p => p.GetCustomAttributes(true).Count(e => e.GetType() == typeof(SQLIgnore)) == 0);
            var fieldNames = properties.Select(p => p.Name).ToArray();
            var paramNames = properties.Select(p => "@" + p.Name).ToArray();
            string sql = $"INSERT INTO {table} ({string.Join(", ", fieldNames)}) VALUES ({string.Join(", ", paramNames)})";

            foreach (var p in properties)
                cmd.Parameters.AddWithValue("@" + p.Name, p.GetValue(obj));

            return sql;
        }

        private static T PopulateStructFromReader<T>(SQLiteDataReader reader) where T : struct
        {
            T result = new T();
            Type type = typeof(T);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string fieldName = reader.GetName(i);
                PropertyInfo property = type.GetProperty(fieldName);

                if (property != null && !reader.IsDBNull(i))
                {
                    object value = reader.GetValue(i);
                    property.SetValue(result, Convert.ChangeType(value, property.PropertyType), null);
                }
            }

            return result;
        }

        public static void AddItem(Item item)
        {
            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = GenerateInsertStatement(item, command, "Items");
                command.ExecuteNonQuery();
            }
        }

        public static void RemoveItem(string name)
        {
            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = $"DELETE FROM Items WHERE Name = '{name}';";
                command.ExecuteNonQuery();
            }
        }

        public static void AddShopkeeper(ShopKeeper keeper)
        {
            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = GenerateInsertStatement(keeper, command, "ShopKeepers");
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

        public static ShopKeeper? GetShopkeeper(string name)
        {
            string query = $"SELECT * FROM Shopkeepers WHERE Name COLLATE NOCASE = {name};";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return PopulateStructFromReader<ShopKeeper>(reader);
                    }
                }
            }
            return null;
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
                        Item randomItem = PopulateStructFromReader<Item>(reader);
                        if ((int)randomItem.Rarity < (int)highest)
                        {
                            for (int i = 0; i < raities[randomItem.Rarity]; i++)
                                items.Add(randomItem);
                        }
                    }
                }
            }
            return items[rand.Next(items.Count)];
        }

        public static Item? GetItem(string name)
        {
            string query = $"SELECT * FROM Items WHERE Name COLLATE NOCASE = {name};";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return PopulateStructFromReader<Item>(reader);
                    }
                }
            }
            return null;
        }

        public static Item? GetItem(int id)
        {
            string query = $"SELECT * FROM Items WHERE ItemId = {id};";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return PopulateStructFromReader<Item>(reader);
                    }
                }
            }
            return null;
        }

        public static ShopKeeper? GetRandomShopkeeper()
        {
            string query = $"SELECT * FROM ShopKeeper ORDER BY RANDOM() LIMIT 1;";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return PopulateStructFromReader<ShopKeeper>(reader);
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

        public static User? GetUser(ulong discordId)
        {
            string query = $"SELECT * FROM Users WHERE DiscordId = {discordId};";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return PopulateStructFromReader<User>(reader);
                    }
                }
            }
            return null;
        }

        public static void AddUser(User user)
        {
            string query = $@"
                    INSERT OR REPLACE INTO Users (DiscordId, Character)
                    VALUES ({user.DiscordId}, {user.Character});";
            using (var command = new SQLiteCommand(query, connection)) command.ExecuteNonQuery();
        }

        public static string GetConfig(string key, string defaul)
        {
            string query = $"SELECT Value FROM Config WHERE Key = {key};";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetString(1);
                    }
                }
            }
            return defaul;
        }

        public static void SetConfig(string key, string value)
        {
            string query = $@"
                    INSERT OR REPLACE INTO Config (Key, Value)
                    VALUES ({key}, {value});";
            using (var command = new SQLiteCommand(query, connection)) command.ExecuteNonQuery();
        }

        public static void AddInventoryItem(ulong userId, ulong itemId)
        {
            string query = $@"
                INSERT INTO ItemInventory (OwnerId, ItemId)
                VALUES ({userId}, {itemId});";
            using (var command = new SQLiteCommand(query, connection)) command.ExecuteNonQuery();
        }

    }
}
