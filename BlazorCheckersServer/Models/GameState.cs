
namespace BlazorCheckersServer.Models
{
    public class GameState(CheckersGameServerModel game)
    {
        public List<CheckersSquare> Board { get; set; } = game.Board;
        public List<CheckersPiece> Pieces { get; set; } = game.Pieces;
        public List<CheckersPiece?> BlackPieces { get; set; } = game.BlackPieces;
        public List<CheckersPiece?> RedPieces { get; set; } = game.RedPieces;
        public bool IsRedTurn { get; set; } = game.IsRedTurn;
        public bool IsBlackTurn => !IsRedTurn;
        public CheckersPiece? SelectedPiece { get; set; } = game.SelectedPiece;
        public bool IsGameOver { get; set; } = game.IsGameOver;
        public string? GameOverMessage { get; set; } = game.GameOverMessage;
    }
}
