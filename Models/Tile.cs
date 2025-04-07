using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MemoryGame.Models
{
    public class Tile : INotifyPropertyChanged
    {
        private bool isFlipped;
        private bool isMatched;

        public string ImagePath { get; set; }
        public int PairId { get; set; }

        public bool IsFlipped
        {
            get => isFlipped;
            set
            {
                if (isFlipped != value)
                {
                    isFlipped = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsMatched
        {
            get => isMatched;
            set
            {
                if (isMatched != value)
                {
                    isMatched = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
