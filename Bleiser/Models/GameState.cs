using System.IO.Pipelines;

namespace Bleiser.Models
{
    public class GameState
    {
        public List<CheckersSquare>? Board { get; set; }
        public List<CheckersPiece>? Pieces { get; set; }
        public List<CheckersPiece?>? BlackPieces { get; set; }
        public List<CheckersPiece?>? RedPieces { get; set; }
        public bool IsRedTurn { get; set; }
        public CheckersPiece? SelectedPiece { get; set; }
        public bool IsGameOver { get; set; }
        public string? GameOverMessage { get; set; }
    }
}
