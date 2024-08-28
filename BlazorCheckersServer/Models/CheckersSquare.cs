using System.Collections.ObjectModel;


namespace BlazorCheckersServer.Models
{
    public class CheckersSquare
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public bool IsValidMove { get; set; }
        public string BackgroundColor => (Row + Column) % 2 == 0 ? "#F0D2B4" : "#BA7A3A";

        public void UpdateValidMoves(List<(int, int)> validMoves)
        {
            IsValidMove = validMoves.Contains((Row, Column));
        }
    }
}
