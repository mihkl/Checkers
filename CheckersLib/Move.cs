using static CheckersLib.Constants;
namespace CheckersLib;

public sealed class Move(int row, int column, bool IsCaptureMove)
{
    public int Row { get; set; } = row;
    public int Col { get; set; } = column;
    public bool IsCaptureMove { get; set; } = IsCaptureMove;

    public static List<(int, int)> ToTuple(List<Move> moves) => moves.Select(move => (move.Row, move.Col)).ToList();

    public static int GetMoveIndex(int row, int col) => row * BoardParameters + col;

    public static List<Move> RemoveOutOfBounds(List<Move> possibleMoves) => possibleMoves.Where(IsInBounds).ToList();

    public static bool IsInBounds(Move move) =>
        !(move.Row < 0 || move.Row > BoardParameters - 1 || move.Col < 0 || move.Col > BoardParameters - 1);

    public static bool IsValidIndex(int row, int col, out int index)
    {
        index = GetMoveIndex(row, col);
        return index >= 0 && index < BoardParameters * BoardParameters;
    }

    public static bool CanAddMove(int row, int col, List<CheckersPiece> pieces)
    {
        if (IsValidIndex(row, col, out var index))
        {
            return pieces[index] is EmptyPiece;
        }
        return false;
    }

    public static void AddDirectionToMove(int row, int col, int rowDirection, int colDirection, out int newRow, out int newColumn)
    {
        newRow = row + rowDirection;
        newColumn = col + colDirection;
    }
}
