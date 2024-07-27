using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Models {
    public class CheckersPiece(int index, PieceColor color = PieceColor.Transparent) {
        public bool IsKing { get; set; } = false;
        public PieceColor? Color { get; set; } = color;
        public string? ColorString => Color switch {
            PieceColor.Black => "Black",
            PieceColor.Red => "Red",
            _ => "Transparent"
        };
        public int Index { get; set; } = index;
        public int Row => Index / 8;
        public int Column => Index % 8;
        public (int, int) Position => (Row, Column);
        public string PieceInfo => $"{Color} {(IsKing ? "King" : "Piece")} at {Row}, {Column}";

        public List<(int, int)> GetPossibleMoves(ObservableCollection<CheckersPiece?> pieces, bool isAfterCapture = false) {

            var possibleMoves = new List<(int, int)>();
            var directions = new List<(int, int)> { (1, 1), (1, -1), (-1, 1), (-1, -1) };
            if (IsKing) {
                directions.AddRange([(1, 1), (1, -1), (-1, 1), (-1, -1)]);
            }
            foreach (var (row, column) in directions) {
                var newRow = Row + row;
                var newColumn = Column + column;
                if (newRow < 0 || newRow > 7 || newColumn < 0 || newColumn > 7) continue;
                var newIndex = newRow * 8 + newColumn;
                if (pieces[newIndex] is EmptyPiece) {
                    if (isAfterCapture) continue;
                    possibleMoves.Add((newRow, newColumn));
                }
                else {
                    var jumpRow = newRow + row;
                    var jumpColumn = newColumn + column;
                    if (jumpRow < 0 || jumpRow > 7 || jumpColumn < 0 || jumpColumn > 7) continue;
                    var jumpIndex = jumpRow * 8 + jumpColumn;
                    if (pieces[jumpIndex] is EmptyPiece) {
                        possibleMoves.Add((jumpRow, jumpColumn));
                    }
                }
            }
            var outOfBounds = possibleMoves.Where(IsOutOfBounds).ToList();
            foreach (var move in outOfBounds) {
                possibleMoves.Remove(move);
            }
            return possibleMoves;
        }

        public bool? Move(CheckersPiece selectedPiece, CheckersPiece piece, ObservableCollection<CheckersPiece?> pieces) {
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

        private static bool DidCapture(ObservableCollection<CheckersPiece?> pieces, (int, int) originalPiecePosition, (int, int) originalSelectedPiecePosition, CheckersPiece piece) {
            
            var rowDifference = originalPiecePosition.Item1 - originalSelectedPiecePosition.Item1;
            var columnDifference = originalPiecePosition.Item2 - originalSelectedPiecePosition.Item2;
            if (Math.Abs(rowDifference) == 2 && Math.Abs(columnDifference) == 2) {
                CapturePiece(pieces, originalPiecePosition, originalSelectedPiecePosition);
                return true;
            }
            if (piece.IsKing && (rowDifference == 2 || columnDifference == 2)) {
                CapturePiece(pieces, originalPiecePosition, originalSelectedPiecePosition);
                return true;
            }
            return false;
        }

        private static void CapturePiece(ObservableCollection<CheckersPiece?> pieces, (int, int) originalPiecePosition, (int, int) originalSelectedPiecePosition) {
            
            var row = (originalPiecePosition.Item1 + originalSelectedPiecePosition.Item1) / 2;
            var column = (originalPiecePosition.Item2 + originalSelectedPiecePosition.Item2) / 2;
            var index = row * 8 + column;
            pieces[index] = new EmptyPiece(index);

        }

        private static bool IsOutOfBounds((int Row, int Column) move) {
            if (move.Row < 0 || move.Row > 7 || move.Column < 0 || move.Column > 7) {
                return true;
            }
            return false;
        }
    }

    public sealed class EmptyPiece(int index): CheckersPiece(index) {}
}
