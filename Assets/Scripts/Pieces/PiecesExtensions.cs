using System.Collections.Generic;
using System.Linq;

public static class PiecesExtensions
{
    public static Piece AtPosition(this IEnumerable<Piece> pieces, BoardPosition? pos)
    {
        return pieces.SingleOrDefault(p => p.Position == pos);
    }

    public static bool AnyMovingTo(this IEnumerable<Piece> pieces, BoardPosition move)
    {
        return pieces.MovingTo(move).Any();
    }

    public static IEnumerable<Piece> MovingTo(this IEnumerable<Piece> pieces, BoardPosition move)
    {
        return pieces.Where(p => p.Move == move);
    }

    public static IEnumerable<Piece> OfColor(this IEnumerable<Piece> pieces, ChessColor color)
    {
        return pieces.Where(p => p.Color == color);
    }
}