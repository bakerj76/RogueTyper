using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Game.Resources
{
    public class ResourceLoader
    {
        private const int NumTiles = 10;

        public static Material[] Tiles;
        public static Material Player;

        public static string[] Words;

        private const byte DifficultyLevels = 10;

        static ResourceLoader()
        {
            LoadMaterials();
            LoadWords();
        }

        private static void LoadMaterials()
        {
            Player = GetMaterial("player.png");
            Tiles = new Material[NumTiles];
            LoadTiles();
        }

        private static void LoadWords()
        {
            Words = File.ReadAllLines(@"../../Resources/words.txt");
        }

        public static string GetRandomWord(byte minDifficulty, byte maxDifficulty)
        {
            minDifficulty = minDifficulty > DifficultyLevels ? DifficultyLevels : minDifficulty;

            var rangeStart = (Words.Length / DifficultyLevels) * minDifficulty;
            var rangeEnd = (Words.Length / DifficultyLevels) * (maxDifficulty + 1);

            return Words[StaticRandom.Random.Next(rangeStart, rangeEnd)];
        }

        private static void LoadTiles()
        {
            for (var i = 0; i < NumTiles; i++)
            {
                Tiles[i] = GetMaterial("tile" + i + ".png");
            }
        }

        private static ImageBrush GetImageBrush(ImageSource image)
        {
            return new ImageBrush{ImageSource = image};
        }

        private static DiffuseMaterial GetMaterial(ImageSource image)
        {
            return new DiffuseMaterial(GetImageBrush(image));
        }

        private static DiffuseMaterial GetMaterial(string path)
        {
            return GetMaterial(new BitmapImage(GetUri(path)));
        }

        private static Uri GetUri(string path)
        {
            return new Uri(Path.Combine(@"../../Resources/Images/", path), UriKind.Relative);
        }
    }
}
