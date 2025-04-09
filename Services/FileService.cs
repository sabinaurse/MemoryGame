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

            // Evită duplicate
            if (!users.Exists(u => u.Username == newUser.Username))
            {
                users.Add(newUser);
                string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
        }

        public static void DeleteUser(string username)
        {
            // Ștergem utilizatorul din lista de utilizatori.
            var users = LoadUsers();
            users.RemoveAll(u => u.Username == username);
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);

            // Șterge fișierul de joc salvat, dacă există.
            DeleteSavedGame(username);

            // Opțional: șterge fișierul de imagine asociat utilizatorului.
            // Înainte de a șterge imaginea, te asiguri că imaginea se află într-un folder unde este
            // permisă ștergerea și că imaginea nu este folosită de alți utilizatori.
            DeleteUserImage(username);
        }

        private static void DeleteSavedGame(string username)
        {
            // Construiește calea către fișierul de joc salvat.
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
                    // Poți loga eroarea sau afișa un mesaj pentru depanare.
                    Console.WriteLine($"Eroare la ștergerea fișierului de joc salvat: {ex.Message}");
                }
            }
        }

        private static void DeleteUserImage(string username)
        {
            // Încarcă utilizatorul pentru a obține calea imaginii.
            var users = LoadUsers();
            var user = users.FirstOrDefault(u => u.Username == username);

            // Dacă nu găsim utilizatorul, nu avem ce șterge.  
            // Dacă imaginea a fost ștearsă la înregistrare sau este o imagine standard, nu o ștergem.
            // Dacă ai stocat calea imaginii personalizate în user.ImagePath, o poți șterge:
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
                        // Logarea sau tratarea erorii.
                        Console.WriteLine($"Eroare la ștergerea imaginii: {ex.Message}");
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
