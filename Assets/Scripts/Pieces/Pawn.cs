using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public override HashSet<BoardPosition> CalculateLegalDestinations()
    {
        var legalDestinations = new HashSet<BoardPosition>();
        int dir = Color == ChessColor.White ? 1 : -1;

        // Move
        BoardPosition? normalMove = Position.Add(0, dir);
        if (normalMove != null && Others.AtPosition(normalMove) == null)
        {
            legalDestinations.Add((BoardPosition)normalMove);

            // Double move
            BoardPosition? doubleMove = Position.Add(0, dir * 2);
            bool doubleMoveAllowed = (Color == ChessColor.White && Position.y == 1) || (Color == ChessColor.Black && Position.y == 6);
            if (doubleMoveAllowed && doubleMove != null && Others.AtPosition(doubleMove) == null)
            {
                legalDestinations.Add((BoardPosition)doubleMove);
            }
        }

        // Attack
        BoardPosition? attack1 = Position.Add(1, dir);
        BoardPosition? attack2 = Position.Add(-1, dir);
        if (Enemies.AtPosition(attack1) != null)
        {
            legalDestinations.Add((BoardPosition)attack1);
        }
        if (Enemies.AtPosition(attack2) != null)
        {
            legalDestinations.Add((BoardPosition)attack2);
        }

        return legalDestinations;
    }
}
