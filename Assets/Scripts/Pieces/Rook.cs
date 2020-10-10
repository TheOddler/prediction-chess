using System.Collections.Generic;

public class Rook : Piece
{
    public override HashSet<BoardPosition> CalculateLegalDestinations()
    {
        return new HashSet<BoardPosition>();
    }
}
