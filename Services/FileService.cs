using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using MemoryGame.Models;

namespace MemoryGame.Services
{
    public static class FileService
    {
        private static readonly string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.json");

        public static List<User> LoadUsers()
        {
            if (!File.Exists(filePath))
                return new List<User>();

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        public static void SaveUser(User newUser)
        {
            var users = LoadUsers();

            if (!users.Exists(u => u.Username == newUser.Username))
            {
                users.Add(newUser);
                string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
        }

        public static void DeleteUser(string username)
        {
            var users = LoadUsers();
            users.RemoveAll(u => u.Username == username);
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);

            DeleteSavedGame(username);

            DeleteUserImage(username);
        }

        private static void DeleteSavedGame(string username)
        {
            string saveFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saved_games");
            string savedGamePath = Path.Combine(saveFolder, $"{username}.json");

            if (File.Exists(savedGamePath))
            {
                try
                {
                    File.Delete(savedGamePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error when trying to delet the saved game file: {ex.Message}");
                }
            }
        }

        private static void DeleteUserImage(string username)
        {
            var users = LoadUsers();
            var user = users.FirstOrDefault(u => u.Username == username);

            if (user != null && !string.IsNullOrEmpty(user.ImagePath))
            {
                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, user.ImagePath);
                if (File.Exists(imagePath))
                {
                    try
                    {
                        File.Delete(imagePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error when deleting picture: {ex.Message}");
                    }
                }
            }
        }

        public static void UpdateStatistics(string username, bool won)
        {
            var users = LoadUsers();
            var user = users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                user.GamesPlayed++;
                if (won)
                    user.GamesWon++;

                string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
        }
    }
}
