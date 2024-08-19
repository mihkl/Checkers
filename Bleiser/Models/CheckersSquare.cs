using System.Collections.ObjectModel;


namespace Bleiser.Models
{
    public class CheckersSquare
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public bool IsValidMove { get; set; }
        public string BackgroundColor => (Row + Column) % 2 == 0 ? "White" : "LightGray";

        public void UpdateValidMoves(ObservableCollection<(int, int)> validMoves)
        {
            IsValidMove = validMoves.Contains((Row, Column));
        }
    }
}
