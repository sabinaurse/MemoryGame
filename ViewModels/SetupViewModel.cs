using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using MemoryGame.Commands;
using MemoryGame.Services;
using MemoryGame.Views;

namespace MemoryGame.ViewModels
{
    public class SetupViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> Categories { get; set; } = new()
        {
            "Animals", "Flags", "Fruits"
        };

        public ObservableCollection<int> AvailableSizes { get; set; } = new() { 2, 3, 4, 5, 6 };

        private string selectedCategory;
        public string SelectedCategory
        {
            get => selectedCategory;
            set { selectedCategory = value; OnPropertyChanged(); }
        }
        private string currentUsername;
        public string CurrentUsername
        {
            get => currentUsername;
            set { currentUsername = value; OnPropertyChanged(); }
        }

        private bool isStandardMode = true;
        public bool IsStandardMode
        {
            get => isStandardMode;
            set
            {
                isStandardMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCustomMode));
            }
        }

        public bool IsCustomMode
        {
            get => !IsStandardMode;
            set { IsStandardMode = !value; }
        }

        private int selectedRows = 2;
        public int SelectedRows
        {
            get => selectedRows;
            set { selectedRows = value; OnPropertyChanged(); }
        }

        private int selectedColumns = 2;
        public int SelectedColumns
        {
            get => selectedColumns;
            set { selectedColumns = value; OnPropertyChanged(); }
        }

        private string timeLimit;
        public string TimeLimit
        {
            get => timeLimit;
            set { timeLimit = value; OnPropertyChanged(); }
        }

        public SetupViewModel()
        {
            StartGameCommand = new RelayCommand(_ => StartGame());
            AboutCommand = new RelayCommand(_ => OpenAboutWindow()); 
            SaveGameCommand = new RelayCommand(_ => SaveGame());
            OpenGameCommand = new RelayCommand(_ => OpenGame());
            OpenStatisticsCommand = new RelayCommand(_ => OpenStatistics());
            BackCommand = new RelayCommand(_ => Back());
        }

        public ICommand AboutCommand { get; }
        private void OpenAboutWindow()
        {
            var aboutView = new AboutView(); 
            aboutView.ShowDialog();
        }

        public ICommand StartGameCommand { get; }
        private void StartGame()
        {
            int rows = 4;
            int cols = 4;

            if (IsCustomMode)
            {
                rows = SelectedRows;
                cols = SelectedColumns;

                if (rows < 2 || rows > 6 || cols < 2 || cols > 6 || (rows * cols) % 2 != 0)
                {
                    MessageBox.Show("Rows and Columns must be between 2 and 6, and the total number of tiles must be even!");
                    return;
                }
            }

            if (!int.TryParse(TimeLimit, out int time) || time <= 0)
            {
                MessageBox.Show("Please enter a valid positive number for Time Limit!");
                return;
            }

            var gameViewModel = new GameViewModel(SelectedCategory, rows, cols, time, CurrentUsername);
            var gameView = new GameView();
            gameView.DataContext = gameViewModel;
            gameView.Show();

            foreach (var window in Application.Current.Windows)
            {
                if (window is Views.SetupView setupView)
                {
                    setupView.Close();
                    break;
                }
            }
        }
        public ICommand SaveGameCommand { get; }

        private void SaveGame()
        {
            var gameView = Application.Current.Windows.OfType<Views.GameView>().FirstOrDefault();
            if (gameView == null)
            {
                MessageBox.Show("No game is currently open!");
                return;
            }

            if (gameView.DataContext is GameViewModel gameViewModel)
            {
                var gameState = gameViewModel.CreateGameState();
                GameSaveService.SaveGame(gameState, CurrentUsername); 
                MessageBox.Show("Game saved successfully!");
            }
        }

        public ICommand BackCommand { get; }
        private void Back()
        {
            var loginView = new LoginView { DataContext = new LoginViewModel() };
            loginView.Show();

            var setupView = Application.Current.Windows.OfType<SetupView>().FirstOrDefault();
            setupView?.Close();
        }

        public ICommand OpenGameCommand { get; }
        private void OpenGame()
        {
            var gameState = GameSaveService.LoadGame(CurrentUsername);
            if (gameState == null)
            {
                MessageBox.Show("No saved game found for this user!");
                return;
            }

            var gameViewModel = new GameViewModel(gameState.Category, gameState.Rows, gameState.Columns, gameState.TimeLeft, CurrentUsername);
            gameViewModel.LoadFromGameState(gameState);

            var gameView = new Views.GameView { DataContext = gameViewModel };
            gameView.Show();

            var setupView = Application.Current.Windows.OfType<Views.SetupView>().FirstOrDefault();
            setupView?.Close();
        }


        public ICommand OpenStatisticsCommand { get; }

        private void OpenStatistics()
        {
            var statisticsView = new StatisticsView();
            statisticsView.DataContext = new StatisticsViewModel();
            statisticsView.ShowDialog();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
