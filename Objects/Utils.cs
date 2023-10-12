using Discord;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TrickOrTreatBot.Objects;

namespace DiscordBot.Objects
{
    internal static class Utils
    {
        public static string[] champions = new string[]
        {
            "Aatrox",
            "Ahri",
            "Akali",
            "Akshan",
            "Alistar",
            "Amumu",
            "Anivia",
            "Annie",
            "Aphelios",
            "Ashe",
            "Aurelion Sol",
            "Azir",
            "Bard",
            "BelVeth",
            "Blitzcrank",
            "Brand",
            "Braum",
            "Caitlyn",
            "Camille",
            "Cassiopeia",
            "ChoGath",
            "Corki",
            "Darius",
            "Diana",
            "Dr. Mundo",
            "Draven",
            "Ekko",
            "Elise",
            "Evelynn",
            "Ezreal",
            "Fiddlesticks",
            "Fiora",
            "Fizz",
            "Galio",
            "Gangplank",
            "Garen",
            "Gnar",
            "Gragas",
            "Graves",
            "Gwen",
            "Hecarim",
            "Heimerdinger",
            "Illaoi",
            "Irelia",
            "Ivern",
            "Janna",
            "Jarvan IV",
            "Jax",
            "Jayce",
            "Jhin",
            "Jinx",
            "Kalista",
            "Karma",
            "Karthus",
            "Kassadin",
            "Katarina",
            "Kayle",
            "Kayn",
            "Kennen",
            "KaiSa",
            "KhaZix",
            "Kindred",
            "Kled",
            "KogMaw",
            "LeBlanc",
            "Lee Sin",
            "Leona",
            "Lissandra",
            "Lillia",
            "Lucian",
            "Lulu",
            "Lux",
            "Malphite",
            "Malzahar",
            "Maokai",
            "Master Yi",
            "Miss Fortune",
            "Mordekaiser",
            "Morgana",
            "Nami",
            "Nasus",
            "Nautilus",
            "Neeko",
            "Nidalee",
            "Nocturne",
            "Nunu",
            "Olaf",
            "Orianna",
            "Ornn",
            "Pantheon",
            "Poppy",
            "Pyke",
            "Quinn",
            "Qiyana",
            "Rammus",
            "RekSai",
            "Renata Glasc",
            "Rakan",
            "Renekton",
            "Rengar",
            "Rell",
            "Riven",
            "Rumble",
            "Ryze",
            "Samira",
            "Senna",
            "Sett",
            "Sejuani",
            "Seraphine",
            "Shaco",
            "Shen",
            "Shyvana",
            "Singed",
            "Sion",
            "Sivir",
            "Skarner",
            "Sona",
            "Soraka",
            "Swain",
            "Sylas",
            "Syndra",
            "Tahm Kench",
            "Talon",
            "Taliyah",
            "Taric",
            "Teemo",
            "Thresh",
            "Tristana",
            "Trundle",
            "Tryndamere",
            "Twisted Fate",
            "Twitch",
            "Udyr",
            "Urgot",
            "Varus",
            "Vayne",
            "Veigar",
            "VelKoz",
            "Vex",
            "Vi",
            "Viego",
            "Viktor",
            "Vladimir",
            "Volibear",
            "Warwick",
            "Wukong",
            "Xayah",
            "Xerath",
            "Xin Zhao",
            "Yasuo",
            "Yorick",
            "Yone",
            "Yuumi",
            "Zac",
            "Zed",
            "Zeri",
            "Ziggs",
            "Zilean",
            "Zoe",
            "Zyra",
            "Briar",
            "Naafiri",
            "Milio",
            "K'Sante",
            "Nilah",
            "Renata Glasc",
        };

        public static string GetDataFolder()
        {
            string str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TrickOrTreat");

            if (!Directory.Exists(str))
                Directory.CreateDirectory(str);

            return str;
        }

        public static void Log(object message)
        {
            Console.WriteLine($"[General/Info] {message ?? "null"}");
        }

        public static async Task<string> DownloadFile(string url)
        {
            string imageName = null;
            using (WebClient client = new WebClient())
            {
                var data = await client.DownloadDataTaskAsync(new Uri(url));

                using (MD5 md5Hash = MD5.Create())
                {
                    byte[] hashBytes = md5Hash.ComputeHash(data);
                    imageName = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
                imageName += Path.GetExtension(url);

                using var file = File.Create(imageName);
                file.Write(data);
            }
            return imageName;
        }

        public static Stream GetFileFromCache(string fileName, string url)
        {
            string cacheFolder = Path.Combine(Path.GetTempPath(), "PinBot");
            if (!Directory.Exists(cacheFolder))
                Directory.CreateDirectory(cacheFolder);
            string filePath = Path.Combine(cacheFolder, fileName);

            if (File.Exists(filePath))
            {
                return File.OpenRead(filePath);
            }
            using (WebClient client = new WebClient())
            {
                var data = client.DownloadData(url);
                using var file = File.Create(filePath);
                file.Write(data);
            }
            return File.OpenRead(filePath);
        }

    }
}
