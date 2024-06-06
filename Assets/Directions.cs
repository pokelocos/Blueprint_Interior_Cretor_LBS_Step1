using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class Directions
{
    public static List<Vector2Int> directions_8 = new List<Vector2Int>()
    {
        new Vector2Int(1, 0),  // Right
        new Vector2Int(1, 1),  // Right-Up
        new Vector2Int(0, 1),  // Up
        new Vector2Int(-1, 1), // Left-Up
        new Vector2Int(-1, 0), // Left
        new Vector2Int(-1, -1),// Left-Down
        new Vector2Int(0, -1), // Down
        new Vector2Int(1, -1)  // Right-Down
    };

    public static List<Vector2Int> directions_4 = new List<Vector2Int>()
    {
        new Vector2Int(1, 0),  // Right
        new Vector2Int(0, 1),  // Up
        new Vector2Int(-1, 0), // Left
        new Vector2Int(0, -1)  // Down
    };

    public enum Dirs_8
    {
        Right = 0,
        RightUp = 1,
        Up = 2,
        LeftUp = 3,
        Left = 4,
        LeftDown = 5,
        Down = 6,
        RightDown = 7,
        None = -1
    }

    public enum Dirs_4
    {
        Right = 0,
        Up = 1,
        Left = 2,
        Down = 3,
        None = -1
    }

    /// <summary>
    /// Get the directions of a vector.
    /// </summary>
    /// <param name="dir">The vector to analyze.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><b>First</b> goes the horizontal directions.</description></item>
    /// <item><description><b>Second</b> goes the vertical directions.</description></item>
    /// <item><description><b>If</b> the vector is <b>zero</b>, returns none.</description></item>
    /// </list>
    /// </returns>
    public static List<Dirs_4> GetDirs(Vector2 dir)
    {
        var toR = new List<Dirs_4>();
        if (dir.x > 0)
        {
            toR.Add(Dirs_4.Right);
        }
        else if (dir.x < 0)
        {
            toR.Add(Dirs_4.Left);
        }
        else
        {
            toR.Add(Dirs_4.None);
        }

        if (dir.y > 0)
        {
            toR.Add(Dirs_4.Up);
        }
        else if (dir.y < 0)
        {
            toR.Add(Dirs_4.Down);
        }
        else
        {
            toR.Add(Dirs_4.None);
        }

        return toR;
    }

    public static List<Vector2Int> EnumToVector(List<Dirs_4> dir)
    {
        var toR = new List<Vector2Int>();
        foreach (var d in dir)
        {
            if (d == Dirs_4.None)
            {
                toR.Add(Vector2Int.zero);
                continue;
            }

            var v = directions_4[(int)d];
            toR.Add(v);
        }

        return toR;
    }

}