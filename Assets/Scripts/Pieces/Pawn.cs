﻿using System.Collections.Generic;

public class Pawn : Piece
{
    public override HashSet<BoardPosition> CalculateLegalDestinations()
    {
        var legalMoves = new HashSet<BoardPosition>();
        int dir = Color == ChessColor.White ? 1 : -1;

        // Move
        BoardPosition normalMove = Position.Add(0, dir);
        if (Others.AtPosition(normalMove) == null)
        {
            legalMoves.Add(normalMove);
        }

        // Double move
        BoardPosition doubleMove = Position.Add(0, dir * 2);
        bool doubleMoveAllowed = (Color == ChessColor.White && Position.y == 1) || (Color == ChessColor.Black && Position.y == 6);
        if (doubleMoveAllowed && Others.AtPosition(doubleMove) == null)
        {
            legalMoves.Add(doubleMove);
        }

        // Attack
        BoardPosition attack1 = Position.Add(1, dir);
        BoardPosition attack2 = Position.Add(-1, dir);
        if (Enemies.AtPosition(attack1) != null)
        {
            legalMoves.Add(attack1);
        }
        if (Enemies.AtPosition(attack2) != null)
        {
            legalMoves.Add(attack2);
        }

        // Cleanup (it's possible when we are at the edge we may have added our own position as a valid move, remove that)
        legalMoves.Remove(Position);

        return legalMoves;
    }
}
