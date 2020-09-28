using System;
using UnityEngine;

[Serializable]
public struct BoardPosition
{
    public int x;
    public int y;

    public Vector3 worldPosition => new Vector3(x - 3.5f, 0, y - 3.5f);

    public BoardPosition(int x, int y)
    {
        this.x = Mathf.Clamp(x, 0, 7);
        this.y = Mathf.Clamp(y, 0, 7);
    }

    public BoardPosition(Vector3 worldPosition) : this(Mathf.RoundToInt(worldPosition.x + 3.5f), Mathf.RoundToInt(worldPosition.z + 3.5f))
    {
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
}