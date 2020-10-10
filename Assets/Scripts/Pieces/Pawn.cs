using System.Collections.Generic;

public class Pawn : Piece
{
    public override HashSet<BoardPosition> CalculateLegalDestinations()
    {
        var legalMoves = new HashSet<BoardPosition>();
        var others = OtherPieces;
        var enemies = Enemies;
        int dir = Color == ChessColor.White ? 1 : -1;

        // Move
        BoardPosition normalMove = Position.Add(0, dir);
        if (others.AtPosition(normalMove) == null)
        {
            legalMoves.Add(normalMove);
        }

        // Attack
        BoardPosition attack1 = Position.Add(1, dir);
        BoardPosition attack2 = Position.Add(-1, dir);
        if (enemies.AtPosition(attack1) != null)
        {
            legalMoves.Add(attack1);
        }
        if (enemies.AtPosition(attack2) != null)
        {
            legalMoves.Add(attack2);
        }

        // Cleanup (it's possible when we are at the edge we may have added our own position as a valid move, remove that)
        legalMoves.Remove(Position);

        return legalMoves;
    }
}
