
namespace CheckersLib;

public sealed class CheckersSquare
{
    public int Row { get; set; }
    public int Column { get; set; }
    public bool IsValidMove { get; set; }
    public string BackgroundColor => SquareColorsExtensions.ToString((Row + Column) % 2 == 0 ? SquareColors.Light : SquareColors.Dark);

    public void UpdateValidMoves(List<(int, int)> validMoves)
    {
        IsValidMove = validMoves.Contains((Row, Column));
    }
}
