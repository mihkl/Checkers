using Newtonsoft.Json;

namespace Bleiser.Models
{
    public class CheckersGameClientModel()
    {
        public static int BoardParameters => 8;
        public static string BoardParametersString => $"{800 / BoardParameters}px";
        public static string PieceParametersString => $"{640 / BoardParameters}px";
        public List<CheckersSquare> Board { get; set; } = [];
        public List<CheckersPiece> Pieces { get; set; } = [];
        public List<CheckersPiece?> BlackPieces { get; set; } = [];
        public List<CheckersPiece?> RedPieces { get; set; } = [];
        public bool IsRedTurn { get; set; } = true;
        public bool IsBlackTurn => !IsRedTurn;
        public CheckersPiece? SelectedPiece { get; set; }
        public bool IsGameOver { get; set; }
        public string? GameOverMessage { get; set; }
        public bool IsPlayerOne { get; set; }

        public void SyncGameState(object o)
        {
            if (o is null) return;
            var json = o.ToString();
            if (string.IsNullOrWhiteSpace(json)) return;
            var gameState = JsonConvert.DeserializeObject<GameState>(json);
            if (gameState is null) return;

            Board = gameState.Board ?? Board;
            Pieces = gameState.Pieces ?? Pieces;
            BlackPieces = gameState.BlackPieces ?? [];
            RedPieces = gameState.RedPieces ?? [];
            IsRedTurn = gameState.IsRedTurn;
            SelectedPiece = gameState.SelectedPiece;
            IsGameOver = gameState.IsGameOver;
            GameOverMessage = gameState.GameOverMessage;

            RemoveEnemyMoves(Board);
        }

        private void RemoveEnemyMoves(List<CheckersSquare> board)
        {
            if ((IsPlayerOne && IsRedTurn) || (!IsPlayerOne && !IsRedTurn)) return;
            board.ForEach(square => square.IsValidMove = false);
        }
    }
}
