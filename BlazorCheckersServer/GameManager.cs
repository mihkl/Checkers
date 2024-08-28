using BlazorCheckersServer.Models;
using System.Collections.Concurrent;
using System.Linq;

namespace BlazorCheckersServer
{
    public class GameManager
    {
        private readonly ConcurrentDictionary<string, CheckersGameServerModel> games = new();

        public string CreateGame()
        {
            var gameId = Guid.NewGuid().ToString();
            var game = new CheckersGameServerModel();
            games[gameId] = game;
            return gameId;
        }

        public void RemoveGameByValue(CheckersGameServerModel game)
        {
            var gameId = games.FirstOrDefault(x => x.Value == game).Key;
            if (gameId is not null) games.TryRemove(gameId, out _);
        }

        public CheckersGameServerModel? GetGame(string gameId) => games.TryGetValue(gameId, out var game) ? game : null;

        public CheckersGameServerModel? GetGameByConnectionId(string connectionId) => 
            games.Values.FirstOrDefault(x => x.PlayerOneID == connectionId || x.PlayerTwoID == connectionId);

        public bool GameExists(string gameId) => games.ContainsKey(gameId);

        public List<string> GetAvailableGames() => [.. games.Keys];
    }
}
