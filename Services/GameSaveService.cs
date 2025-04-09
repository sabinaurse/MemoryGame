using System;
using System.IO;
using System.Text.Json;
using MemoryGame.Models;

namespace MemoryGame.Services
{
    public static class GameSaveService
    {
        private static readonly string SaveFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saved_games");

        static GameSaveService()
        {
            if (!Directory.Exists(SaveFolder))
                Directory.CreateDirectory(SaveFolder);
        }

        public static void SaveGame(GameState gameState, string username)
        {
            var filePath = Path.Combine(SaveFolder, $"{username}.json");
            var json = JsonSerializer.Serialize(gameState, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public static GameState LoadGame(string username)
        {
            var filePath = Path.Combine(SaveFolder, $"{username}.json");
            if (!File.Exists(filePath))
                return null;

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<GameState>(json);
        }
    }

}
