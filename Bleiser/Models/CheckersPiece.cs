using System.Collections.ObjectModel;

namespace Bleiser.Models
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
        public int Row => Index / 8;
        public int Column => Index % 8;
        public (int, int) Position => (Row, Column);
        public string PieceInfo => $"{Color} {(IsKing ? "King" : "Piece")} at {Row}, {Column}";

        private List<Move> GetPossibleMoves(ObservableCollection<CheckersPiece> pieces, bool isAfterCapture = false)
        {
            var directions = new List<(int, int)>();
            var possibleMoves = new List<Move>();

            switch (Color)
            {
                case PieceColor.Black: directions = [(1, 1), (1, -1), (-1, 1), (-1, -1)]; break;
                case PieceColor.Red: directions = [(-1, 1), (-1, -1), (1, 1), (1, -1)]; break;
            }
            if (IsKing)
            {
                directions.AddRange([(1, 1), (1, -1), (-1, 1), (-1, -1)]);
            }

            foreach (var (row, column) in directions)
            {
                var newRow = Row + row;
                var newColumn = Column + column;
                if (newRow < 0 || newRow > 7 || newColumn < 0 || newColumn > 7) continue;
                var newIndex = newRow * 8 + newColumn;
                if (pieces[newIndex] is EmptyPiece)
                {
                    if (isAfterCapture) continue;
                    if ((Color == PieceColor.Black && newRow < Row) || (Color == PieceColor.Red && newRow > Row)) continue;
                    possibleMoves.Add(new Move(newRow, newColumn, false));
                }
                else if (pieces[newIndex]?.Color != Color)
                {
                    var jumpRow = newRow + row;
                    var jumpColumn = newColumn + column;
                    if (jumpRow < 0 || jumpRow > 7 || jumpColumn < 0 || jumpColumn > 7) continue;
                    var jumpIndex = jumpRow * 8 + jumpColumn;
                    if (pieces[jumpIndex] is EmptyPiece)
                    {
                        possibleMoves.Add(new Move(jumpRow, jumpColumn, true));
                    }
                }
            }

            var outOfBounds = possibleMoves.Where(IsOutOfBounds).ToList();

            foreach (var move in outOfBounds)
            {
                possibleMoves.Remove(move);
            }

            return possibleMoves;
        }

        private List<Move> GetAllFriendlyMoves(ObservableCollection<CheckersPiece> pieces)
        {
            var possibleMoves = new List<Move>();
            foreach (var piece in pieces)
            {
                if (piece.Color == Color)
                {
                    possibleMoves.AddRange(piece.GetPossibleMoves(pieces));
                }
            }
            return possibleMoves;
        }

        public List<Move> GetValidMoves(ObservableCollection<CheckersPiece> pieces, bool isAfterCapture)
        {
            var piecePossibleMoves = GetPossibleMoves(pieces, isAfterCapture);
            var friendlyPossibleMoves = GetAllFriendlyMoves(pieces);
            var validMoves = new List<Move>();

            if (friendlyPossibleMoves.Any(move => move.IsCaptureMove))
            {
                validMoves = piecePossibleMoves.Where(move => move.IsCaptureMove).ToList();
            }
            else
            {
                validMoves = piecePossibleMoves;
            }

            return validMoves;
        }

        public bool? MovePiece(CheckersPiece selectedPiece, CheckersPiece piece, ObservableCollection<CheckersPiece> pieces)
        {
            if (selectedPiece is null) return null;
            var originalSelectedPiecePosition = selectedPiece.Position;
            var originalPiecePosition = piece.Position;
            var index = piece.Index;
            var selectedPieceIndex = selectedPiece.Index;
            pieces[index] = selectedPiece;
            selectedPiece.Index = index;
            var movedPiece = pieces[index];
            pieces[selectedPieceIndex] = new EmptyPiece(selectedPieceIndex);
            if (DidCapture(pieces, originalPiecePosition, originalSelectedPiecePosition, movedPiece)) return true;
            return false;
        }

        private static bool DidCapture(ObservableCollection<CheckersPiece> pieces, (int, int) originalPiecePosition, (int, int) originalSelectedPiecePosition, CheckersPiece piece)
        {
            var rowDifference = originalPiecePosition.Item1 - originalSelectedPiecePosition.Item1;
            var columnDifference = originalPiecePosition.Item2 - originalSelectedPiecePosition.Item2;
            if (Math.Abs(rowDifference) == 2 && Math.Abs(columnDifference) == 2)
            {
                CapturePiece(pieces, originalPiecePosition, originalSelectedPiecePosition);
                return true;
            }
            if (piece.IsKing && (rowDifference == 2 || columnDifference == 2))
            {
                CapturePiece(pieces, originalPiecePosition, originalSelectedPiecePosition);
                return true;
            }
            return false;
        }

        private static void CapturePiece(ObservableCollection<CheckersPiece> pieces, (int, int) originalPiecePosition, (int, int) originalSelectedPiecePosition)
        {
            var row = (originalPiecePosition.Item1 + originalSelectedPiecePosition.Item1) / 2;
            var column = (originalPiecePosition.Item2 + originalSelectedPiecePosition.Item2) / 2;
            var index = row * 8 + column;
            pieces[index] = new EmptyPiece(index);
        }

        private static bool IsOutOfBounds(Move move)
        {
            if (move.Row < 0 || move.Row > 7 || move.Column < 0 || move.Column > 7)
            {
                return true;
            }
            return false;
        }
    }

    public sealed class EmptyPiece(int index): CheckersPiece(index) { }

    public sealed class Move(int row, int column, bool IsCaptureMove)
    {
        public int Row { get; set; } = row;
        public int Column { get; set; } = column;
        public bool IsCaptureMove { get; set; } = IsCaptureMove;
    }
}
