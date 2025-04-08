using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MemoryGame.Models;
using MemoryGame.Commands;
using MemoryGame.Services;

namespace MemoryGame.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Tile> Tiles { get; set; } = new();
        public int Rows { get; }
        public int Columns { get; }
        public string Category { get; }
        public int TimeLeft { get; private set; }

        private DispatcherTimer timer;
        private int initialTimeLimit;
        private string currentUsername;

        public string CurrentUsername
        {
            get => currentUsername;
            set { currentUsername = value; OnPropertyChanged(); }
        }

        public ICommand FlipTileCommand { get; }
        public ICommand SaveGameCommand { get; }

        private Tile firstFlippedTile;
        private Tile secondFlippedTile;
        private bool canFlip = true;

        public GameViewModel(string category, int rows, int columns, int timeLimit, string username)
        {
            Category = category;
            Rows = rows;
            Columns = columns;
            TimeLeft = timeLimit;
            initialTimeLimit = timeLimit;
            CurrentUsername = username;

            FlipTileCommand = new RelayCommand(tileObj => FlipTile(tileObj as Tile));
            SaveGameCommand = new RelayCommand(_ => SaveGame());
            ExitGameCommand = new RelayCommand(_ => ExitGame());

            GenerateTiles();
            StartTimer();
        }

        private void GenerateTiles()
        {
            var imagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tiles", Category);

            if (!Directory.Exists(imagesPath))
            {
                MessageBox.Show($"The selected category folder does not exist: {imagesPath}");
                throw new DirectoryNotFoundException($"Folder not found: {imagesPath}");
            }

            var allImages = Directory.GetFiles(imagesPath, "*.jpg")
                .Concat(Directory.GetFiles(imagesPath, "*.jpeg"))
                .Concat(Directory.GetFiles(imagesPath, "*.png"))
                .Concat(Directory.GetFiles(imagesPath, "*.gif"))
                .ToList();

            if (allImages.Count < (Rows * Columns) / 2)
            {
                MessageBox.Show($"Not enough images in {Category} category! Need at least {(Rows * Columns) / 2} images.");
                throw new Exception("Not enough images in selected category!");
            }

            var random = new Random();

            var selectedImages = allImages.OrderBy(_ => random.Next()).Take((Rows * Columns) / 2).ToList();
            var tileList = new List<Tile>();

            int pairId = 0;
            foreach (var imagePath in selectedImages)
            {
                tileList.Add(new Tile { ImagePath = imagePath.Replace("\\", "/"), IsFlipped = false, IsMatched = false, PairId = pairId });
                tileList.Add(new Tile { ImagePath = imagePath.Replace("\\", "/"), IsFlipped = false, IsMatched = false, PairId = pairId });
                pairId++;
            }

            var shuffledTiles = tileList.OrderBy(_ => random.Next()).ToList();
            foreach (var tile in shuffledTiles)
            {
                Tiles.Add(tile);
            }
        }

        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                TimeLeft--;
                OnPropertyChanged(nameof(TimeLeft));

                if (TimeLeft <= 0)
                {
                    timer.Stop();
                    MessageBox.Show("⏰ Timpul a expirat! Ai pierdut jocul.", "Game Over", MessageBoxButton.OK, MessageBoxImage.Warning);

                    // ➔ Update statistici: doar GamesPlayed++
                    FileService.UpdateStatistics(CurrentUsername, won: false);

                    ExitGame(); // ieșim înapoi la Login după pierdere
                }

            };
            timer.Start();
        }

        private void FlipTile(Tile tile)
        {
            if (!canFlip || tile == null || tile.IsFlipped || tile.IsMatched)
                return;

            tile.IsFlipped = true;

            if (firstFlippedTile == null)
            {
                firstFlippedTile = tile;
            }
            else
            {
                secondFlippedTile = tile;
                canFlip = false;

                var delayTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                delayTimer.Tick += (s, e) =>
                {
                    delayTimer.Stop();
                    CheckMatch();
                };
                delayTimer.Start();
            }
        }

        private void CheckMatch()
        {
            if (firstFlippedTile.PairId == secondFlippedTile.PairId)
            {
                firstFlippedTile.IsMatched = true;
                secondFlippedTile.IsMatched = true;
            }
            else
            {
                firstFlippedTile.IsFlipped = false;
                secondFlippedTile.IsFlipped = false;
            }

            firstFlippedTile = null;
            secondFlippedTile = null;
            canFlip = true;

            OnPropertyChanged(nameof(Tiles));

            CheckVictory();
        }

        private void CheckVictory()
        {
            if (Tiles.All(t => t.IsMatched))
            {
                timer.Stop();
                MessageBox.Show("🏆 Felicitări! Ai câștigat jocul!", "Victory", MessageBoxButton.OK, MessageBoxImage.Information);

                // ➔ Update statistici: GamesPlayed++, GamesWon++
                FileService.UpdateStatistics(CurrentUsername, won: true);

                ExitGame(); // ieșim înapoi la Login după victorie
            }
        }


        private void SaveGame()
        {
            if (string.IsNullOrEmpty(CurrentUsername))
            {
                MessageBox.Show("Username is not set. Cannot save game.");
                return;
            }

            var gameState = CreateGameState();
            GameSaveService.SaveGame(gameState, CurrentUsername);
            MessageBox.Show("Game saved successfully!");
        }


        public GameState CreateGameState()
        {
            var gameState = new GameState
            {
                Category = Category,
                Rows = Rows,
                Columns = Columns,
                TimeLeft = TimeLeft,
                TimeElapsed = initialTimeLimit - TimeLeft,
                Tiles = Tiles.Select(tile => new TileState
                {
                    ImagePath = tile.ImagePath,
                    PairId = tile.PairId,
                    IsFlipped = tile.IsFlipped,
                    IsMatched = tile.IsMatched
                }).ToList()
            };
            return gameState;
        }

        public void LoadFromGameState(GameState gameState)
        {
            Tiles.Clear();

            foreach (var tileState in gameState.Tiles)
            {
                Tiles.Add(new Tile
                {
                    ImagePath = tileState.ImagePath,
                    PairId = tileState.PairId,
                    IsFlipped = tileState.IsFlipped,
                    IsMatched = tileState.IsMatched
                });
            }

            TimeLeft = gameState.TimeLeft;
            OnPropertyChanged(nameof(TimeLeft));
        }
        public ICommand ExitGameCommand { get; }
        private void ExitGame()
        {
            // Caută fereastra GameView
            Views.GameView gameView = null;
            foreach (var window in Application.Current.Windows)
            {
                if (window is Views.GameView gv)
                {
                    gameView = gv;
                    break;
                }
            }

            if (gameView != null)
            {
                // 1. Deschide LoginView
                var loginView = new Views.LoginView();
                loginView.DataContext = new LoginViewModel();
                loginView.Show();

                // 2. Abia apoi închide GameView
                gameView.Close();
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
