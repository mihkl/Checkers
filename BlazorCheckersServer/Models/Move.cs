namespace BlazorCheckersServer.Models
{
    public sealed class Move(int row, int column, bool IsCaptureMove)
    {
        public int Row { get; set; } = row;
        public int Col { get; set; } = column;
        public bool IsCaptureMove { get; set; } = IsCaptureMove;
    }
}
