using System.Collections.Generic;

public class King : Piece
{
    const int MOVE_DISTANCE = 1;

    public override HashSet<BoardPosition> CalculateLegalDestinations()
    {
        var top = CalculateLegalDestinationsInDirection(0, 1, MOVE_DISTANCE);
        var bottom = CalculateLegalDestinationsInDirection(0, -1, MOVE_DISTANCE);
        var left = CalculateLegalDestinationsInDirection(-1, 0, MOVE_DISTANCE);
        var right = CalculateLegalDestinationsInDirection(1, 0, MOVE_DISTANCE);

        var topRight = CalculateLegalDestinationsInDirection(1, 1, MOVE_DISTANCE);
        var topLeft = CalculateLegalDestinationsInDirection(-1, 1, MOVE_DISTANCE);
        var bottomRight = CalculateLegalDestinationsInDirection(1, -1, MOVE_DISTANCE);
        var bottomLeft = CalculateLegalDestinationsInDirection(-1, -1, MOVE_DISTANCE);

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
