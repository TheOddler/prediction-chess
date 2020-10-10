using System.Collections.Generic;

public class Bishop : Piece
{
    public override HashSet<BoardPosition> CalculateLegalDestinations()
    {
        return new HashSet<BoardPosition>();
    }
}
