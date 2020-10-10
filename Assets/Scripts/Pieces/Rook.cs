using System.Collections.Generic;

public class Rook : Piece
{
    public override HashSet<BoardPosition> CalculateLegalDestinations()
    {
        var top = CalculateLegalDestinationsInDirection(0, 1);
        var bottom = CalculateLegalDestinationsInDirection(0, -1);
        var left = CalculateLegalDestinationsInDirection(-1, 0);
        var right = CalculateLegalDestinationsInDirection(1, 0);

        // Combine all
        var all = top; // just so the naming makes more sense
        all.UnionWith(bottom);
        all.UnionWith(left);
        all.UnionWith(right);
        return all;
    }
}
