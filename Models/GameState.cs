using System.Collections.Generic;

namespace MemoryGame.Models
{
    public class GameState
    {
        public string Category { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int TimeLeft { get; set; }
        public int TimeElapsed { get; set; } 

        public List<TileState> Tiles { get; set; } = new();
    }

    public class TileState
    {
        public string ImagePath { get; set; }
        public int PairId { get; set; }
        public bool IsFlipped { get; set; }
        public bool IsMatched { get; set; }
    }
}
