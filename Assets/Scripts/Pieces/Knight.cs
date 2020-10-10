using System.Collections.Generic;

public class Knight : Piece
{
    public override HashSet<BoardPosition> CalculateLegalDestinations()
    {
        var destinations = new HashSet<BoardPosition>();

        // Create possible destinations
        destinations.Add(Position.Add(1, 2));
        destinations.Add(Position.Add(-1, 2));
        destinations.Add(Position.Add(1, -2));
        destinations.Add(Position.Add(-1, -2));
        destinations.Add(Position.Add(2, 1));
        destinations.Add(Position.Add(-2, 1));
        destinations.Add(Position.Add(2, -1));
        destinations.Add(Position.Add(-2, -1));

        // Remove those that ended up outside the board, and thus got their distance reduced
        destinations.RemoveWhere(p => p.ManhattenDistanceTo(Position) != 3);

        // Remove those that end up on friends
        destinations.RemoveWhere(p => Friends.AtPosition(p) != null);

        return destinations;
    }
}
