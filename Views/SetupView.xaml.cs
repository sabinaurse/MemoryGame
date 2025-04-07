using MemoryGame.Commands;
using MemoryGame.ViewModels;
using System.Windows;

namespace MemoryGame.Views
{
    public partial class SetupView : Window
    {
        public SetupView()
        {
            InitializeComponent();
            DataContext = new SetupViewModel();
        }
    }
}
