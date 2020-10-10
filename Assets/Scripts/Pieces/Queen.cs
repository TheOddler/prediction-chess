using System.Collections.Generic;

public class Queen : Piece
{
    public override HashSet<BoardPosition> CalculateLegalDestinations()
    {
        var top = CalculateLegalDestinationsInDirection(0, 1);
        var bottom = CalculateLegalDestinationsInDirection(0, -1);
        var left = CalculateLegalDestinationsInDirection(-1, 0);
        var right = CalculateLegalDestinationsInDirection(1, 0);

        var topRight = CalculateLegalDestinationsInDirection(1, 1);
        var topLeft = CalculateLegalDestinationsInDirection(-1, 1);
        var bottomRight = CalculateLegalDestinationsInDirection(1, -1);
        var bottomLeft = CalculateLegalDestinationsInDirection(-1, -1);

        // Combine all
        var all = top; // just so the naming makes more sense
        all.UnionWith(bottom);
        all.UnionWith(left);
        all.UnionWith(right);
        all.UnionWith(topRight);
        all.UnionWith(topLeft);
        all.UnionWith(bottomRight);
        all.UnionWith(bottomLeft);
        return all;
    }
}
