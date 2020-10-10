using System.Collections.Generic;

public class Queen : Piece
{
    public override HashSet<BoardPosition> CalculateLegalDestinations()
    {
        return new HashSet<BoardPosition>();
    }
}
