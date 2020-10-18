using System.Collections.Generic;

public class Knight : Piece
{
    public override HashSet<BoardPosition> CalculateLegalDestinations()
    {
        var destinations = new HashSet<BoardPosition>();

        // Create possible destinations
        MaybeAddMove(1, 2, destinations);
        MaybeAddMove(-1, 2, destinations);
        MaybeAddMove(1, -2, destinations);
        MaybeAddMove(-1, -2, destinations);
        MaybeAddMove(2, 1, destinations);
        MaybeAddMove(-2, 1, destinations);
        MaybeAddMove(2, -1, destinations);
        MaybeAddMove(-2, -1, destinations);

        // Remove those that end up on friends
        destinations.RemoveWhere(p => Friends.AtPosition(p) != null);

        return destinations;
    }

    private void MaybeAddMove(int x, int y, HashSet<BoardPosition> set)
    {
        var destination = Position.Add(x, y);
        if (destination != null)
        {
            set.Add((BoardPosition)destination);
        }
    }
}
