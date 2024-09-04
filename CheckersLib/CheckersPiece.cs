using static CheckersLib.Constants;
using static CheckersLib.Move;

namespace CheckersLib;

public class CheckersPiece(int index, PieceColors? color = PieceColors.Transparent,
    PieceColors? borderColor = PieceColors.Transparent)
{
    public PieceColors? BorderColor { get; set; } = borderColor;
    public string? BorderColorString => PieceColorsExtensions.ToString(BorderColor);
    public PieceColors? Color { get; set; } = color;
    public string? ColorString => PieceColorsExtensions.ToString(Color);
    public int Index { get; set; } = index;
    public int Row => Index / BoardParameters;
    public int Col => Index % BoardParameters;
    public (int, int) Position => (Row, Col);

    public List<Move> GetValidMoves(List<CheckersPiece> pieces, bool isAfterCapture)
    {
        var piecePossibleMoves = RemoveOutOfBounds(GetMoves(pieces, isAfterCapture));
        var friendlyPossibleMoves = RemoveOutOfBounds(GetAllFriendlyMoves(pieces, isAfterCapture));

        if (TryGetOnlyCaptureMoves(isAfterCapture, friendlyPossibleMoves, piecePossibleMoves, out var captureMoves))
        {
            return captureMoves;
        }

        return piecePossibleMoves;
    }

    public void MovePiece(Move move, List<CheckersPiece> pieces)
    {
        var originalSelectedPiecePosition = Position;
        var originalPiecePosition = (move.Row, move.Col);
        var index = move.Row * BoardParameters + move.Col;
        var selectedPieceIndex = Index;
        pieces[index] = this;
        Index = index;
        pieces[selectedPieceIndex] = new EmptyPiece(selectedPieceIndex);

        if (move.IsCaptureMove) CapturePiece(pieces, originalPiecePosition, originalSelectedPiecePosition);
    }

    protected virtual bool TryGetOnlyCaptureMoves(bool isAfterCapture, List<Move> friendlyPossibleMoves, List<Move> piecePossibleMoves,
        out List<Move> captureMoves)
    {
        captureMoves = [];
        if (isAfterCapture || friendlyPossibleMoves.Any(move => move.IsCaptureMove))
        {
            captureMoves = piecePossibleMoves.Where(move => move.IsCaptureMove).ToList();
            return true;
        }
        return false;
    }

    protected virtual List<Move> GetMoves(List<CheckersPiece> pieces, bool isAfterCapture)
    {
        var possibleMoves = new List<Move>();

        foreach (var (rowDirection, colDirection) in Directions)
        {
            AddDirectionToMove(Row, Col, rowDirection, colDirection, out var newRow, out var newCol);

            if (CanAddMove(newRow, newCol, pieces))
            {
                if (isAfterCapture) continue;
                if (Color == PieceColors.Black && newRow < Row || Color == PieceColors.Red && newRow > Row) continue;

                possibleMoves.Add(new Move(newRow, newCol, false));
            }
            else if (TryAccessCollection(pieces, GetMoveIndex(newRow, newCol), out CheckersPiece? piece))
            {
                if (piece?.Color == Color) continue;

                AddDirectionToMove(newRow, newCol, rowDirection, colDirection, out var jumpRow, out var jumpCol);

                if (CanAddMove(jumpRow, jumpCol, pieces)) possibleMoves.Add(new Move(jumpRow, jumpCol, true));
            }
        }
        return possibleMoves;
    }

    protected virtual void CapturePiece(List<CheckersPiece> pieces, (int, int) originalPiecePosition, (int, int) originalSelectedPiecePosition)
    {
        var row = (originalPiecePosition.Item1 + originalSelectedPiecePosition.Item1) / 2;
        var column = (originalPiecePosition.Item2 + originalSelectedPiecePosition.Item2) / 2;
        var index = row * BoardParameters + column;
        pieces[index] = new EmptyPiece(index);
    }

    protected List<Move> GetAllFriendlyMoves(List<CheckersPiece> pieces, bool isAfterCapture)
    {
        var possibleMoves = new List<Move>();
        foreach (var piece in pieces.Where(piece => piece.Color == Color))
        {
            possibleMoves.AddRange(piece.GetMoves(pieces, isAfterCapture));
        }
        return possibleMoves;
    }
}
