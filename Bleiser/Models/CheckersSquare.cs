using System.Collections.ObjectModel;


namespace Bleiser.Models
{
    public class CheckersSquare
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public bool IsValidMove { get; set; }
        public string BackgroundColor => (Row + Column) % 2 == 0 ? "#F0D2B4" : "#BA7A3A";

        public void UpdateValidMoves(ObservableCollection<(int, int)> validMoves)
        {
            IsValidMove = validMoves.Contains((Row, Column));
        }
    }
}
