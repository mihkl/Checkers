

namespace BlazorCheckersServer.Models
{
    public class CheckersGameServerModel
    {
        public static int BoardParameters => 8;
        public List<CheckersSquare> Board { get; set; } = [];
        public List<CheckersPiece> Pieces { get; set; } = [];
        public List<Move> CurrentValidMoves { get; set; } = [];
        public List<CheckersPiece?> BlackPieces { get; set; } = [];
        public List<CheckersPiece?> RedPieces { get; set; } = [];
        public bool IsRedTurn { get; set; } = true;
        public bool IsBlackTurn => !IsRedTurn;
        public CheckersPiece? SelectedPiece { get; set; }
        public uint PlayerCount { get; set; } = 0;
        public string PlayerOneID { get; set; } = string.Empty;
        public string PlayerTwoID { get; set; } = string.Empty;
        public bool IsGameOver { get; set; }
        public string? GameOverMessage { get; set; }

        public CheckersGameServerModel()
        {
            InitializeBoard(BoardParameters);
            InitializePieces(BoardParameters);
        }

        public GameState GetGameState() => new(this);

        private void ChangeTurn()
        {
            SelectedPiece = null;
            IsRedTurn = !IsRedTurn;
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

        private void UpdateValidMoves(List<Move> currentValidMoves) => 
            Board.ForEach(square => square.UpdateValidMoves(GetMovesAsInt(currentValidMoves)));

        private static List<(int, int)> GetMovesAsInt(List<Move> moves) => moves.Select(move => (move.Row, move.Col)).ToList();

        public void TrySelectPiece(CheckersPiece piece, CheckersPiece? selectedPiece)
        {
            if ((piece.Color == PieceColor.Black && IsRedTurn)
                || (piece.Color == PieceColor.Red && IsBlackTurn)
                || (selectedPiece is null && piece is EmptyPiece)) return;

            if (selectedPiece is null || selectedPiece?.Color == piece.Color)
            {
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
            if (GetMovesAsInt(CurrentValidMoves).Contains((piece.Row, piece.Col)))
            {
                var move = CurrentValidMoves.Find(move => move.Row == piece.Row && move.Col == piece.Col)!;
                MakeMove(move, SelectedPiece);
                UpdatePieces();
            }
        }

        public void MakeMove(Move move, CheckersPiece? selectedPiece)
        {
            if (selectedPiece is null) return;
            selectedPiece.MovePiece(selectedPiece, move, Pieces);

            if (move.IsCaptureMove is false)
            {
                CurrentValidMoves = [];
                ChangeTurn();
            }
            else
            {
                CurrentValidMoves = AddPossibleMoves(selectedPiece, true);
                if (CurrentValidMoves.Count == 0) ChangeTurn(); 
            }
            UpdateValidMoves(CurrentValidMoves);
        }

        private List<Move> AddPossibleMoves(CheckersPiece? selectedPiece, bool isAfterCapture = false)
        {
            if (selectedPiece is null) return [];
            var moves = selectedPiece.GetValidMoves(Pieces, isAfterCapture);

            UpdateValidMoves(moves);
            return moves;
        }

        #region Initialization
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
        #endregion
    }
}
