﻿using System.Collections.Generic;

public class Knight : Piece
{
    public override HashSet<BoardPosition> CalculateLegalDestinations()
    {
        return new HashSet<BoardPosition>();
    }
}