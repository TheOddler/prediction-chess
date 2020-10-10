public enum ChessColor
{
    White,
    Black,
}

public static class ChessColorExtensions
{
    public static ChessColor Invert(this ChessColor color)
    {
        return color == ChessColor.White ? ChessColor.Black : ChessColor.White;
    }

    public static UnityEngine.Color AsUnityColor(this ChessColor color)
    {
        return color == ChessColor.White ? UnityEngine.Color.white : UnityEngine.Color.black;
    }
}
