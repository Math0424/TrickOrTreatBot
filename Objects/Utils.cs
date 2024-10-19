using Discord;
using ImageMagick;
using ImageMagick.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TrickOrTreatBot.Objects;

namespace DiscordBot.Objects
{
    internal static class Utils
    {
        private static Random r = new Random();

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
                Uri uri = new Uri(url);
                var data = await client.DownloadDataTaskAsync(uri);

                using (MD5 md5Hash = MD5.Create())
                {
                    byte[] hashBytes = md5Hash.ComputeHash(data);
                    imageName = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
                imageName += Path.GetExtension(uri.LocalPath);

                Console.WriteLine($"Saving image '{Path.Combine(GetDataFolder(), imageName)}'");
                using var file = File.Create(Path.Combine(GetDataFolder(), imageName));
                file.Write(data);
            }
            return imageName;
        }

        public static Stream GetFileFromCache(string fileName)
        {
            string filePath = Path.Combine(GetDataFolder(), fileName);

            if (File.Exists(filePath))
                return File.OpenRead(filePath);
            return null;
        }

        private static string[] gradients = new string[]
        {
            "gradient:#C56CD6FF-#3425AFFF",
            "gradient:#fad961FF-#f76b1cFF",
            "gradient:#f02fc2FF-#6094eaFF",
            "gradient:#42e695FF-#3bb2b8FF",
            "gradient:#F5515FFF-#A1051DFF",
            "gradient:#13f1fcFF-#0470dcFF"
        };

        private static Dictionary<Rarity, string> rarity = new Dictionary<Rarity, string>
        {
            { Rarity.Common, "gradient:#E3E3E3FF-#5D6874FF"},
            { Rarity.Rare, "gradient:#13f1fcFF-#0470dcFF"},
            { Rarity.Epic, "gradient:#fad961FF-#f76b1cFF"},
            { Rarity.Mythic, "gradient:#7117eaFF-#ea6060FF"},
        };

        public static Stream GetItemPreview(string imageFile, string name, Rarity rare)
        {
            MemoryStream stream = new MemoryStream();

            uint width = 512, height = 512;
            using (MagickImage image = new MagickImage(MagickColors.Transparent, width, height))
            {
                image.Draw(
                    new DrawableStrokeColor(MagickColors.Transparent),
                    new DrawableFillColor(MagickColors.Black),
                    new DrawableRoundRectangle(0, 0, width - 1, height - 1, 20, 20)
                );

                using MagickImage img = new MagickImage(GetFileFromCache(imageFile));

                uint newHeight, newWidth;
                double imageAspect = (double)img.Width / img.Height;
                double boxAspect = (double)width / height;

                if (imageAspect < boxAspect)
                {
                    newWidth = width;
                    newHeight = (uint)(width / imageAspect);
                }
                else
                {
                    newHeight = height;
                    newWidth = (uint)(height * imageAspect);
                }

                img.Resize(new MagickGeometry(newWidth, newHeight)
                {
                    IgnoreAspectRatio = false
                });
                image.Composite(img, Gravity.South, 0, 0, CompositeOperator.Atop);

                using MagickImage gradient = new MagickImage(rarity[rare], width, height);
                gradient.Evaluate(Channels.Alpha, EvaluateOperator.Multiply, 0.2);
                image.Composite(gradient, CompositeOperator.Atop);

                image.Draw(
                    new DrawableStrokeColor(MagickColors.White),
                    new DrawableFillColor(MagickColors.Transparent),
                    new DrawableRoundRectangle(0, 0, width - 1, height - 1, 20, 20)
                );

                var settings = new MagickReadSettings()
                {
                    Font = "Arial",
                    Width = width - 30,
                    Height = 60,
                    BackgroundColor = MagickColors.Transparent,
                    FillColor = MagickColors.White,
                    TextGravity = Gravity.Center,
                };
                using MagickImage text01 = new MagickImage($"caption:{name}", settings);
                image.Composite(text01, Gravity.North, CompositeOperator.Atop);

                image.Write(stream, MagickFormat.Png);
            }
            stream.Position = 0;
            return stream;
        }

        public static Stream GetShopkeeperPreview(string imageFile, string topText, string miniText, string bottomText, Item item = null)
        {
            MemoryStream stream = new MemoryStream();
            uint width = 650, height = 1050, cards = 3;
            using (MagickImage canvas = new MagickImage(MagickColors.Transparent, width + ((cards - 1) * 15), height + ((cards - 1) * 15)))
            {
                using MagickImage image = new MagickImage(MagickColors.Transparent, width, height);
                
                image.Draw(
                    new DrawableStrokeColor(MagickColors.Transparent),
                    new DrawableFillColor(MagickColors.Black),
                    new DrawableRoundRectangle(0, 0, width - 1, height - 1, 30, 30)
                );

                using MagickImage img = new MagickImage(GetFileFromCache(imageFile));
                double scaleFactor = (double)(height - 130) / img.Height;

                uint newHeight, newWidth;
                double imageAspect = (double)img.Width / img.Height;
                double boxAspect = (double)width / height;

                if (imageAspect < boxAspect)
                {
                    newWidth = width;
                    newHeight = (uint)(width / imageAspect);
                }
                else
                {
                    newHeight = height - 130;
                    newWidth = (uint)((height - 130) * imageAspect);
                }

                img.Resize(new MagickGeometry(newWidth, newHeight)
                {
                    IgnoreAspectRatio = false
                });
                image.Composite(img, Gravity.South, 0, 50, CompositeOperator.Atop);

                using MagickImage gradient  = new MagickImage(gradients[r.Next(gradients.Length)], width, height);
                gradient.Evaluate(Channels.Alpha, EvaluateOperator.Multiply, 0.2);
                image.Composite(gradient, CompositeOperator.Atop);

                image.Draw(
                    new DrawableFillColor(MagickColors.Black),
                    new DrawableRoundRectangle(0, 0, width - 1, 170, 30, 30)
                );

                var settings = new MagickReadSettings()
                {
                    Font = "Arial",
                    Width = width - 60,
                    Height = 120,
                    BackgroundColor = MagickColors.Transparent,
                    FillColor = MagickColors.White,
                };
                using MagickImage text01 = new MagickImage($"caption:{topText}", settings);
                image.Composite(text01, Gravity.North, CompositeOperator.Atop);

                settings.Height = 50;
                using MagickImage text02 = new MagickImage($"caption:{miniText}", settings);
                image.Composite(text02, Gravity.North, 0, 120, CompositeOperator.Atop);


                if (item != null)
                {
                    image.Draw(
                        new DrawableFillColor(MagickColors.Black),
                        new DrawableGravity(Gravity.South),
                        new DrawableRoundRectangle(0, height - 140, width - 1, height, 30, 30)
                    );

                    using MagickImage itemImage = new MagickImage(GetItemPreview(item.ImageFile, "", (Rarity)item.Rarity));
                    itemImage.Resize(200, 200);
                    image.Composite(itemImage, Gravity.Southeast, 20, 20, CompositeOperator.Atop);

                    if (bottomText != null)
                    {
                        settings.Width -= 200;
                        settings.Height = 100;
                        using MagickImage text03 = new MagickImage($"caption:{bottomText}", settings);
                        image.Composite(text03, Gravity.Southwest, 30, 20, CompositeOperator.Atop);
                    }
                } 
                else if (bottomText != null)
                {
                    image.Draw(
                        new DrawableFillColor(MagickColors.Black),
                        new DrawableGravity(Gravity.South),
                        new DrawableRoundRectangle(0, height - 100, width - 1, height, 30, 30)
                    );

                    new Drawables()
                        .FontPointSize(64)
                        .Font("Arial")
                        .FillColor(MagickColors.White)
                        .Gravity(Gravity.South)
                        .Text(0, 20, bottomText)
                        .Draw(image);
                }

                double opacity = (1.0 / cards);
                image.Evaluate(Channels.Alpha, EvaluateOperator.Multiply, opacity);
                for (int i = 0; i < cards; i++)
                {
                    canvas.Composite(image, Gravity.Southeast, i * 15, i * 15, CompositeOperator.SrcOver);
                    image.Evaluate(Channels.Alpha, EvaluateOperator.Multiply, 1.5 + (opacity * (i + 1)));
                }
                canvas.Write(stream, MagickFormat.Png);
            }
            stream.Position = 0;
            return stream;
        }


    }
}
