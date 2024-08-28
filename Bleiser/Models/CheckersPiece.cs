using System.Collections.ObjectModel;
using static Bleiser.Models.CheckersGameClientModel;

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
        public int Row => Index / BoardParameters;
        public int Column => Index % BoardParameters;
        public (int, int) Position => (Row, Column);
        public string PieceInfo => $"{Color} {(IsKing ? "King" : "Piece")} at {Row}, {Column}";

    }
}
