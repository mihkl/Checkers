namespace CheckersLib;

public class GameState(List<CheckersSquare>? board, List<CheckersPiece>? pieces, bool isRedTurn,
    CheckersPiece? selectedPiece, bool isGameOver, string? gameOverMessage)
{
    public List<CheckersSquare>? Board { get; set; } = board;
    public List<CheckersPiece>? Pieces { get; set; } = pieces;
    public bool IsRedTurn { get; set; } = isRedTurn;
    public CheckersPiece? SelectedPiece { get; set; } = selectedPiece;
    public bool IsGameOver { get; set; } = isGameOver;
    public string? GameOverMessage { get; set; } = gameOverMessage;
}

