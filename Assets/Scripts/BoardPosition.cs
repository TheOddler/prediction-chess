using System;
using UnityEngine;

[Serializable]
public struct BoardPosition
{
    public readonly int x;
    public readonly int y;

    public Vector3 worldPosition => new Vector3(x - 3.5f, 0, y - 3.5f);

    public bool IsAtTopEdge => x == 7;
    public bool IsAtBottomEdge => x == 0;

    public BoardPosition(int x, int y)
    {
        this.x = Mathf.Clamp(x, 0, 7);
        this.y = Mathf.Clamp(y, 0, 7);
    }

    public BoardPosition(Vector3 worldPosition) : this(Mathf.RoundToInt(worldPosition.x + 3.5f), Mathf.RoundToInt(worldPosition.z + 3.5f))
    {
    }

    public static BoardPosition? IfInBoard(int x, int y)
    {
        if (x >= 0 && x <= 7 && y >= 0 && y <= 7) return new BoardPosition(x, y);
        else return null;
    }

    public static BoardPosition? IfInBoard(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x + 3.5f);
        int y = Mathf.RoundToInt(worldPosition.z + 3.5f);

        return IfInBoard(x, y);
    }

    public override bool Equals(object obj)
    {
        if (obj is BoardPosition)
        {
            return this == (BoardPosition)obj;
        }
        else
        {
            return false;
        }
    }

    public override int GetHashCode()
    {
        int hashCode = 144517355;
        hashCode = hashCode * -1521134295 + x.GetHashCode();
        hashCode = hashCode * -1521134295 + y.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(BoardPosition a, BoardPosition b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(BoardPosition a, BoardPosition b)
    {
        return !(a == b);
    }

    public override string ToString()
    {
        return $"({x},{y})";
    }

    public BoardPosition? Add(int x, int y)
    {
        return IfInBoard(this.x + x, this.y + y);
    }

    public int ManhattenDistanceTo(BoardPosition other)
    {
        return Mathf.Abs(x - other.x) + Mathf.Abs(y - other.y);
    }
}