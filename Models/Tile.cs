using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MemoryGame.Models
{
    public class Tile : INotifyPropertyChanged
    {

        public string ImagePath { get; set; }
        public int PairId { get; set; }

        private bool isFlipped;
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

        private bool isMatched;
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
