namespace CheckersLib;

public static class Constants
{
    public const int BoardParameters = 8;
    public static string BoardParametersString => $"{800 / BoardParameters}px";
    public static string PieceParametersString => $"{640 / BoardParameters}px";
    public static List<(int rowDirection, int colDirection)> Directions => [(1, 1), (1, -1), (-1, 1), (-1, -1)];

    public static bool TryAccessCollection<T>(List<T> collection, int index, out T? value)
    {
        if (index >= 0 && index < collection.Count)
        {
            value = collection[index];
            return true;
        }
        value = default;
        return false;
    }
}
