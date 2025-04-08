using System;
using System.IO;

namespace MemoryGame.Models
{
    public class User
    {
        public string Username { get; set; }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public string ImagePath { get; set; }

        public string FullImagePath =>
            new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ImagePath)).AbsoluteUri;
    }
}
