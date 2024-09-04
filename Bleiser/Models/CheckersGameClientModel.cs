using Newtonsoft.Json;
using CheckersLib;

namespace Bleiser.Models;

public class CheckersGameClientModel()
{
    public List<CheckersSquare> Board { get; set; } = [];
    public List<CheckersPiece> Pieces { get; set; } = [];
    public List<CheckersPiece> BlackPieces => Pieces.Where(piece => piece.Color == PieceColors.Black).ToList();
    public List<CheckersPiece> RedPieces => Pieces.Where(piece => piece.Color == PieceColors.Red).ToList();
    public bool IsRedTurn { get; set; } = true;
    public bool IsBlackTurn => !IsRedTurn;
    public CheckersPiece? SelectedPiece { get; set; }
    public bool IsGameOver { get; set; }
    public string? GameOverMessage { get; set; }
    public bool IsPlayerOne { get; set; }

    public void SyncGameState(object state)
    {
        if (state is null) return;
        var json = state.ToString();
        if (string.IsNullOrWhiteSpace(json)) return;
        var gameState = JsonConvert.DeserializeObject<GameState>(json);
        if (gameState is null) return;

        Board = gameState.Board ?? Board;
        Pieces = gameState.Pieces ?? Pieces;
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
