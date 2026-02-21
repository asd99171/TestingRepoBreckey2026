using UnityEngine;

public enum GridDirection
{
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

public static class GridDirectionUtil
{
    public static Vector3 ToWorldVector(GridDirection dir)
    {
        if (dir == GridDirection.North)
        {
            return Vector3.forward;
        }
        else if (dir == GridDirection.East)
        {
            return Vector3.right;
        }
        else if (dir == GridDirection.South)
        {
            return Vector3.back;
        }
        else
        {
            return Vector3.left;
        }
    }

    public static GridDirection TurnLeft(GridDirection dir)
    {
        int v = (int)dir;
        v = (v + 3) % 4;
        return (GridDirection)v;
    }

    public static GridDirection TurnRight(GridDirection dir)
    {
        int v = (int)dir;
        v = (v + 1) % 4;
        return (GridDirection)v;
    }

    public static float ToYaw(GridDirection dir)
    {
        if (dir == GridDirection.North)
        {
            return 0f;
        }
        else if (dir == GridDirection.East)
        {
            return 90f;
        }
        else if (dir == GridDirection.South)
        {
            return 180f;
        }
        else
        {
            return 270f;
        }
    }
}
