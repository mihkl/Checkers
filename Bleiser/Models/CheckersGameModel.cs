using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace Bleiser.Models
{
    public class CheckersGameModel
    {
        public static int BoardParameters => 8;
        public static string BoardParametersString => $"{800 / BoardParameters}px";
        public static string PieceParametersString => $"{640 / BoardParameters}px";
        public ObservableCollection<CheckersSquare> Board { get; set; }
        public ObservableCollection<CheckersPiece> Pieces { get; set; }
        public ObservableCollection<(int, int)> CurrentValidMoves { get; set; }
        public ObservableCollection<CheckersPiece?> BlackPieces { get; set; }
        public ObservableCollection<CheckersPiece?> RedPieces { get; set; }
        public bool IsRedTurn { get; set; } = true;
        public bool IsBlackTurn => !IsRedTurn;
        public CheckersPiece? SelectedPiece { get; set; }

        public bool IsGameOver { get; set; }

        public string? GameOverMessage { get; set; }

        public CheckersGameModel(string[] args)
        {
            Board = [];
            Pieces = [];
            CurrentValidMoves = [];
            BlackPieces = [];
            RedPieces = [];

            InitializeBoard(BoardParameters);
            InitializePieces(BoardParameters);
        }
        

        private void MakeMove(CheckersPiece piece, CheckersPiece? selectedPiece)
        {
            if (selectedPiece is null) return;
            var didCapture = selectedPiece.MovePiece(selectedPiece, piece, Pieces);

            if (didCapture is null or false)
            {
                CurrentValidMoves = [];
                ChangeTurn();
            }
            else
            {
                CurrentValidMoves = AddPossibleMoves(selectedPiece, true);
                if (CurrentValidMoves.Count == 0)
                {
                    ChangeTurn();
                }
            }

            UpdateValidMoves(CurrentValidMoves);
        }

        private void ChangeTurn()
        {
            SelectedPiece = null;
            IsRedTurn = !IsRedTurn;
        }

        public void TrySelectPiece(CheckersPiece? piece, CheckersPiece? selectedPiece)
        {
            if (piece is null) return;
            if (selectedPiece is null && piece is EmptyPiece) return;
            if (selectedPiece is null || selectedPiece?.Color == piece.Color)
            {
                if (piece?.Color == PieceColor.Black && IsRedTurn) return;
                if (piece?.Color == PieceColor.Red && IsBlackTurn) return;
                SelectedPiece = piece;
                CurrentValidMoves = AddPossibleMoves(SelectedPiece);
                return;
            }
            if (selectedPiece == piece)
            {
                SelectedPiece = null;
                CurrentValidMoves = [];
                return;
            }
            if (CurrentValidMoves.Contains((piece.Row, piece.Column)))
            {
                MakeMove(piece, SelectedPiece);
                UpdatePieces();
            }
        }

        private void UpdatePieces()
        {
            BlackPieces.Clear();
            RedPieces.Clear();
            foreach (var piece in Pieces)
            {
                if (piece is not EmptyPiece)
                {
                    switch (piece.Color)
                    {
                        case PieceColor.Black: BlackPieces.Add(piece); break;
                        case PieceColor.Red: RedPieces.Add(piece); break;
                    }
                }
            }
            CheckGameOver();
        }

        private void CheckGameOver()
        {
            if (BlackPieces.Count == 0)
            {
                IsGameOver = true;
                GameOverMessage = "Red Wins!";
            }
            else if (RedPieces.Count == 0)
            {
                IsGameOver = true;
                GameOverMessage = "Black Wins!";
            }
        }

        private void UpdateValidMoves(ObservableCollection<(int, int)> currentValidMoves)
        {
            foreach (var square in Board)
            {
                square.UpdateValidMoves(currentValidMoves);
            }
        }

        private ObservableCollection<(int, int)> AddPossibleMoves(CheckersPiece? selectedPiece, bool isAfterCapture = false)
        {
            if (selectedPiece is null) return [];
            var moves = selectedPiece.GetValidMoves(Pieces, isAfterCapture);
            var validMoves = new ObservableCollection<(int, int)>();
            foreach (var move in moves)
            {
                if (move.Row >= 0 && move.Row < BoardParameters && move.Column >= 0 && move.Column < BoardParameters)
                {
                    validMoves.Add((move.Row, move.Column));
                }
            }
            UpdateValidMoves(validMoves);
            return validMoves;
        }

        private void InitializeBoard(int boardParameters)
        {
            for (var i = 0; i < boardParameters; i++)
            {
                for (var j = 0; j < boardParameters; j++)
                {
                    var square = new CheckersSquare
                    {
                        Row = i,
                        Column = j
                    };
                    Board.Add(square);
                }
            }
        }

        private void InitializePieces(int boardParameters)
        {
            for (int i = 0; i < boardParameters; i++)
            {
                for (int j = 0; j < boardParameters; j++)
                {
                    var index = i * boardParameters + j;
                    if ((i + j) % 2 == 0)
                    {
                        Pieces.Add(new EmptyPiece(index));
                    }
                    else
                    {
                        if (i < boardParameters / 2 - 1)
                        {
                            var piece = new CheckersPiece(index, PieceColor.Black);
                            BlackPieces.Add(piece);
                            Pieces.Add(piece);
                        }
                        else if (i > boardParameters / 2)
                        {
                            var piece = new CheckersPiece(index, PieceColor.Red);
                            RedPieces.Add(piece);
                            Pieces.Add(piece);
                        }
                        else
                        {
                            Pieces.Add(new EmptyPiece(index));
                        }
                    }
                }
            }
        }
    }
}
