using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using MemoryGame.Commands;
using MemoryGame.Models;
using MemoryGame.Services;
using MemoryGame.Views;

namespace MemoryGame.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<User> Users { get; set; } = new();
        private User selectedUser;
        public User SelectedUser
        {
            get => selectedUser;
            set
            {
                selectedUser = value;
                SelectedUsername = selectedUser?.Username;
                SelectedImagePath = selectedUser?.ImagePath;
                OnPropertyChanged();
                UpdateCommandStates();
            }
        }

        private string selectedUsername;
        public string SelectedUsername
        {
            get => selectedUsername;
            set
            {
                selectedUsername = value;
                OnPropertyChanged();
                UpdateCommandStates();
            }
        }

        private string selectedImagePath;
        public string SelectedImagePath
        {
            get => selectedImagePath;
            set
            {
                selectedImagePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedFullImagePath)); // pentru preview imagine
                UpdateCommandStates();
            }
        }

        // Pentru previzualizare imagine în partea dreaptă
        public string SelectedFullImagePath =>
            string.IsNullOrEmpty(SelectedImagePath) ? null :
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SelectedImagePath).Replace("\\", "/");

        public ICommand LoadUsersCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand BrowseImageCommand { get; }

        public LoginViewModel()
        {
            LoadUsersCommand = new RelayCommand(_ => LoadUsers());
            RegisterCommand = new RelayCommand(_ => Register(), _ => CanRegister());
            DeleteCommand = new RelayCommand(_ => DeleteUser(), _ => SelectedUsername != null);
            PlayCommand = new RelayCommand(_ => Play(), _ => SelectedUsername != null);
            BrowseImageCommand = new RelayCommand(_ => BrowseImage());

            LoadUsers();
        }

        private void LoadUsers()
        {
            Users.Clear();
            foreach (var user in FileService.LoadUsers())
                Users.Add(user);
        }

        private void Register()
        {
            var newUser = new User { Username = SelectedUsername, ImagePath = SelectedImagePath };
            FileService.SaveUser(newUser);
            LoadUsers();
            SelectedUsername = "";
            SelectedImagePath = "";
        }

        private void DeleteUser()
        {
            FileService.DeleteUser(SelectedUsername);
            LoadUsers();
            SelectedUsername = "";
        }
        private void Play()
        {
            var setupView = new SetupView();
            var setupViewModel = new SetupViewModel();
            setupViewModel.CurrentUsername = SelectedUsername;
            setupView.DataContext = setupViewModel;
            setupView.Show();

            foreach (var window in System.Windows.Application.Current.Windows)
            {
                if (window is Views.LoginView loginView)
                {
                    loginView.Close();
                    break;
                }
            }
        }

        private void BrowseImage()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif",
                InitialDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images")
            };

            if (dialog.ShowDialog() == true)
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var selectedFullPath = dialog.FileName;

                if (selectedFullPath.StartsWith(baseDir))
                {
                    var relativePath = Path.GetRelativePath(baseDir, selectedFullPath);
                    SelectedImagePath = relativePath.Replace("\\", "/");
                }
                else
                {
                    System.Windows.MessageBox.Show("Te rog selectează o imagine din folderul Images al aplicației.");
                }
            }
        }


        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(SelectedUsername) && !string.IsNullOrWhiteSpace(SelectedImagePath);
        }


        private void UpdateCommandStates()
        {
            (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PlayCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
