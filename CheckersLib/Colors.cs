namespace CheckersLib;

public enum PieceColors
{
    Black,
    Red,
    Yellow,
    White,
    Transparent,
}

public static class PieceColorsExtensions
{
    public static string ToString(PieceColors? color) => color switch
    {
        PieceColors.Black => "Black",
        PieceColors.Red => "Red",
        PieceColors.Yellow => "Yellow",
        PieceColors.White => "White",
        _ => "Transparent",
    };
}

public enum SquareColors
{
    Light,
    Dark,
}

public static class SquareColorsExtensions
{
    public static string ToString(SquareColors color) => color switch
    {
        SquareColors.Light => "#F0D2B4",
        SquareColors.Dark => "#BA7A3A",
        _ => "Red",
    };
}
