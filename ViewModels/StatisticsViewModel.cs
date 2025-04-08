using System.Collections.ObjectModel;
using MemoryGame.Models;
using MemoryGame.Services;

namespace MemoryGame.ViewModels
{
    public class StatisticsViewModel
    {
        public ObservableCollection<User> Users { get; set; }

        public StatisticsViewModel()
        {
            Users = new ObservableCollection<User>(FileService.LoadUsers());
        }
    }
}
