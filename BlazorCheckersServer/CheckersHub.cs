using Microsoft.AspNetCore.SignalR;

namespace BlazorCheckersServer
{
    public class CheckersHub(GameManager gameManager): Hub
    {
        private readonly GameManager gameManager = gameManager;

        public async Task<string> CreateGame()
        {
            var gameId = gameManager.CreateGame();
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
            await Clients.Caller.SendAsync("GameCreated", gameId);
            return gameId;
        }

        public async Task JoinGame(string gameId)
        {
            if (!gameManager.GameExists(gameId))
            {
                await Clients.Caller.SendAsync("Error", "Game does not exist.");
                return;
            }

            var game = gameManager.GetGame(gameId)!;
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);

            game.PlayerCount++;
            if (game.PlayerCount == 1 && string.IsNullOrEmpty(game.PlayerOneID))
            {
                game.PlayerOneID = Context.ConnectionId;
                await Clients.Caller.SendAsync("ReceivePlayer", true);
            }
            else if (game.PlayerCount == 2 && string.IsNullOrEmpty(game.PlayerTwoID))
            {
                game.PlayerTwoID = Context.ConnectionId;
                await Clients.Caller.SendAsync("ReceivePlayer", false);
            }

            var gameState = game.GetGameState();
            await Clients.Group(gameId).SendAsync("ReceiveGameState", gameState);
        }

        public async Task TrySelectPiece(string gameId, int pieceIndex)
        {
            if (!gameManager.GameExists(gameId))
            {
                await Clients.Caller.SendAsync("Error", "Game does not exist.");
                return;
            }

            var game = gameManager.GetGame(gameId)!;

            if ((Context.ConnectionId != game.PlayerOneID && game.IsRedTurn) ||
                (Context.ConnectionId != game.PlayerTwoID && game.IsBlackTurn))
            {
                return;
            }

            var piece = game.Pieces[pieceIndex];
            game.TrySelectPiece(piece, game.SelectedPiece);
            var gameState = game.GetGameState();

            await Clients.Group(gameId).SendAsync("ReceiveGameState", gameState);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var game = gameManager.GetGameByConnectionId(Context.ConnectionId);

            if (Context.ConnectionId == game?.PlayerOneID) game.PlayerOneID = "";

            else if (Context.ConnectionId == game?.PlayerTwoID) game.PlayerTwoID = "";

            if (game?.PlayerCount > 0) game.PlayerCount--;

            if (game?.PlayerCount == 0) gameManager.RemoveGameByValue(game);

            return base.OnDisconnectedAsync(exception);
        }
    }

}
