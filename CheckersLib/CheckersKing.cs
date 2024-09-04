using static CheckersLib.Constants;
using static CheckersLib.Move;

namespace CheckersLib;

public sealed class CheckersKing(int index, PieceColors? color, PieceColors? borderColor = PieceColors.Yellow): CheckersPiece(index, color, borderColor)
{
    private static void Capture(List<CheckersPiece> pieces, int row, int col) =>
        pieces[row * BoardParameters + col] = new EmptyPiece(row * BoardParameters + col);

    protected override List<Move> GetMoves(List<CheckersPiece> pieces, bool isAfterCapture)
    {
        var possibleMoves = new List<Move>();

        foreach (var (rowDirection, colDirection) in Directions)
        {
            for (var i = 1; i < BoardParameters; i++)
            {
                AddDirectionToMove(Row, Col, rowDirection * i, colDirection * i, out var newRow, out var newCol);

                if (CanAddMove(newRow, newCol, pieces))
                {
                    if (isAfterCapture) break;
                    if (!possibleMoves.Any(possibleMoves => possibleMoves.Row == newRow && possibleMoves.Col == newCol))
                    {
                        possibleMoves.Add(new Move(newRow, newCol, false)); continue;
                    }
                }
                else if (TryAccessCollection(pieces, GetMoveIndex(newRow, newCol), out CheckersPiece? piece))
                {
                    if (piece?.Color == Color) break;
                }
                AddDirectionToMove(newRow, newCol, rowDirection, colDirection, out var jumpRow, out var jumpCol);

                if (!CanAddMove(jumpRow, jumpCol, pieces)) break;
                possibleMoves.Add(new Move(jumpRow, jumpCol, true));

                while (pieces[GetMoveIndex(jumpRow, jumpCol)] is EmptyPiece)
                {
                    jumpRow += rowDirection;
                    jumpCol += colDirection;

                    if (!IsValidIndex(jumpRow, jumpCol, out _)) break;
                    if (CanAddMove(jumpRow, jumpCol, pieces)) possibleMoves.Add(new Move(jumpRow, jumpCol, true));
                }
            }
        }
        return possibleMoves;
    }

    protected override void CapturePiece(List<CheckersPiece> pieces, (int, int) originalPiecePosition, (int, int) originalSelectedPiecePosition)
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
            if (pieces[index]?.Color != Color && pieces[index] is not EmptyPiece)
            {
                Capture(pieces, row, column);
            }
            row += rowDirection;
            column += columnDirection;
        }
    }

    protected override bool TryGetOnlyCaptureMoves(bool isAfterCapture, List<Move> friendlyPossibleMoves, List<Move> piecePossibleMoves,
        out List<Move> captureMoves)
    {
        captureMoves = [];
        if (isAfterCapture && friendlyPossibleMoves.Any(move => move.IsCaptureMove))
        {
            captureMoves = piecePossibleMoves.Where(move => move.IsCaptureMove).ToList();
            return true;
        }
        return false;
    }
}
