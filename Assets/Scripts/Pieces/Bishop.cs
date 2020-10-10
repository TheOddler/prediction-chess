using System.Collections.Generic;

public class Bishop : Piece
{
    public override HashSet<BoardPosition> CalculateLegalDestinations()
    {
        var topRight = CalculateLegalDestinationsInDirection(1, 1);
        var topLeft = CalculateLegalDestinationsInDirection(-1, 1);
        var bottomRight = CalculateLegalDestinationsInDirection(1, -1);
        var bottomLeft = CalculateLegalDestinationsInDirection(-1, -1);

        // Combine all
        var all = topRight; // just so the naming makes more sense
        all.UnionWith(topLeft);
        all.UnionWith(bottomRight);
        all.UnionWith(bottomLeft);
        return all;
    }
}
