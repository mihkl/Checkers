using static BlazorCheckersServer.Models.CheckersGameServerModel;

namespace BlazorCheckersServer.Models
{
    public class CheckersPiece(int index, PieceColor color = PieceColor.Transparent)
    {
        public bool IsKing { get; set; } = false;
        public PieceColor? Color { get; set; } = color;
        public string? ColorString => Color switch
        {
            PieceColor.Black => "Black",
            PieceColor.Red => "Red",
            _ => "Transparent"
        };
        public int Index { get; set; } = index;
        public int Row => Index / BoardParameters;
        public int Col => Index % BoardParameters;
        public (int, int) Position => (Row, Col);

        public List<Move> GetValidMoves(List<CheckersPiece> pieces, bool isAfterCapture)
        {
            var piecePossibleMoves = GetMoves(pieces, isAfterCapture);
            var friendlyPossibleMoves = GetAllFriendlyMoves(pieces);

            if ((!IsKing || isAfterCapture) && friendlyPossibleMoves.Any(move => move.IsCaptureMove))
            {
                return piecePossibleMoves.Where(move => move.IsCaptureMove).ToList();
            }
            return piecePossibleMoves;
        }

        private List<Move> GetMoves(List<CheckersPiece> pieces, bool isAfterCapture = false)
        {
            if (IsKing) return RemoveOutOfBounds(GetKingMoves(pieces, isAfterCapture));
            return RemoveOutOfBounds(GetRegularMoves(pieces, isAfterCapture));
        }

        private List<Move> GetAllFriendlyMoves(List<CheckersPiece> pieces)
        {
            var possibleMoves = new List<Move>();
            foreach (var piece in pieces.Where(piece => piece.Color == Color))
            {
                possibleMoves.AddRange(piece.GetMoves(pieces));
            }
            return possibleMoves;
        }

        private static List<Move> RemoveOutOfBounds(List<Move> possibleMoves) => possibleMoves.Where(IsInBounds).ToList();

        private static bool IsInBounds(Move move) => 
            !(move.Row < 0 || move.Row > BoardParameters - 1 || move.Col < 0 || move.Col > BoardParameters - 1);

        private List<Move> GetRegularMoves(List<CheckersPiece> pieces, bool isAfterCapture)
        {
            var possibleMoves = new List<Move>();
            List<(int, int)> directions = [(1, 1), (1, -1), (-1, 1), (-1, -1)];

            foreach (var (row, column) in directions)
            {
                var newRow = Row + row;
                var newColumn = Col + column;
                if (newRow < 0 || newRow > BoardParameters - 1 || newColumn < 0 || newColumn > BoardParameters - 1) continue;
                var newIndex = newRow * BoardParameters + newColumn;

                if (pieces[newIndex] is EmptyPiece)
                {
                    if (isAfterCapture) continue;
                    if (Color == PieceColor.Black && newRow < Row || Color == PieceColor.Red && newRow > Row) continue;

                    possibleMoves.Add(new Move(newRow, newColumn, false));
                }
                else if (pieces[newIndex]?.Color != Color)
                {
                    var jumpRow = newRow + row;
                    var jumpColumn = newColumn + column;
                    if (jumpRow < 0 || jumpRow > BoardParameters - 1 || jumpColumn < 0 || jumpColumn > BoardParameters - 1) continue;

                    var jumpIndex = jumpRow * BoardParameters + jumpColumn;

                    if (pieces[jumpIndex] is EmptyPiece)
                    {
                        possibleMoves.Add(new Move(jumpRow, jumpColumn, true));
                    }
                }
            }
            return possibleMoves;
        }

        private List<Move> GetKingMoves(List<CheckersPiece> pieces, bool isAfterCapture)
        {
            var possibleMoves = new List<Move>();
            List<(int, int)> directions = [(1, 1), (1, -1), (-1, 1), (-1, -1)];

            foreach (var (row, column) in directions)
            {
                for (var i = 1; i < BoardParameters; i++)
                {
                    var newRow = Row + row * i;
                    var newColumn = Col + column * i;
                    if (newRow < 0 || newRow > BoardParameters - 1 || newColumn < 0 || newColumn > BoardParameters - 1) break;
                    var newIndex = newRow * BoardParameters + newColumn;

                    if (pieces[newIndex] is EmptyPiece)
                    {
                        if (isAfterCapture) break;

                        if (!(possibleMoves.Any(possibleMoves => possibleMoves.Row == newRow && possibleMoves.Col == newColumn)))
                        {
                            possibleMoves.Add(new Move(newRow, newColumn, false)); continue;
                        }
                    }
                    else if (pieces[newIndex]?.Color == Color) break;

                    var jumpRow = newRow + row;
                    var jumpColumn = newColumn + column;
                    if (jumpRow < 0 || jumpRow > BoardParameters - 1 || jumpColumn < 0 || jumpColumn > BoardParameters - 1) break;

                    var jumpIndex = jumpRow * BoardParameters + jumpColumn;

                    if (pieces[jumpIndex] is not EmptyPiece) break;

                    possibleMoves.Add(new Move(jumpRow, jumpColumn, true));

                    while (pieces[jumpIndex] is EmptyPiece)
                    {
                        jumpRow += row;
                        jumpColumn += column;
                        if (jumpRow < 0 || jumpRow > BoardParameters - 1 || jumpColumn < 0 || jumpColumn > BoardParameters - 1) break;

                        jumpIndex = jumpRow * BoardParameters + jumpColumn;
                        if (pieces[jumpIndex] is EmptyPiece) possibleMoves.Add(new Move(jumpRow, jumpColumn, true));
                    }
                }
            }
            return possibleMoves;
        }

        public void MovePiece(CheckersPiece selectedPiece, Move move, List<CheckersPiece> pieces)
        {
            if (selectedPiece is null) return;
            var originalSelectedPiecePosition = selectedPiece.Position;
            var originalPiecePosition = (move.Row, move.Col);
            var index = move.Row * BoardParameters + move.Col;
            var selectedPieceIndex = selectedPiece.Index;
            pieces[index] = selectedPiece;
            selectedPiece.Index = index;
            var movedPiece = pieces[index];
            pieces[selectedPieceIndex] = new EmptyPiece(selectedPieceIndex);

            IsKing = IsKing || (Color == PieceColor.Black && Row == BoardParameters - 1) || (Color == PieceColor.Red && Row == 0);

            if (IsKing)
            {
                CapturePieceKing(pieces, originalPiecePosition, originalSelectedPiecePosition, movedPiece);
                return;
            }
            CapturePieceRegular(pieces, originalPiecePosition, originalSelectedPiecePosition);
        }

        private static void CapturePieceKing(List<CheckersPiece> pieces, (int, int) originalPiecePosition, (int, int) originalSelectedPiecePosition, CheckersPiece movedPiece)
        {
            var rowDifference = originalPiecePosition.Item1 - originalSelectedPiecePosition.Item1;
            var columnDifference = originalPiecePosition.Item2 - originalSelectedPiecePosition.Item2;

            var rowDirection = rowDifference > 0 ? 1 : -1;
            var columnDirection = columnDifference > 0 ? 1 : -1;
            var row = originalSelectedPiecePosition.Item1;
            var column = originalSelectedPiecePosition.Item2;

            while (row != originalPiecePosition.Item1 && column != originalPiecePosition.Item2)
            {
                var index = row * BoardParameters + column;
                if (pieces[index]?.Color != movedPiece.Color && pieces[index] is not EmptyPiece)
                {
                    CapturePieceKing(pieces, row, column);
                }
                row += rowDirection;
                column += columnDirection;
            }
        }

        private static void CapturePieceKing(List<CheckersPiece> pieces, int row, int col) =>
            pieces[row * BoardParameters + col] = new EmptyPiece(row * BoardParameters + col);

        private static void CapturePieceRegular(List<CheckersPiece> pieces, (int, int) originalPiecePosition, (int, int) originalSelectedPiecePosition)
        {
            var row = (originalPiecePosition.Item1 + originalSelectedPiecePosition.Item1) / 2;
            var column = (originalPiecePosition.Item2 + originalSelectedPiecePosition.Item2) / 2;
            var index = row * BoardParameters + column;
            pieces[index] = new EmptyPiece(index);
        }
    }
}
