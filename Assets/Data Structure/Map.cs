using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class Tile : ICloneable
{
    public int roomID = -1;
    public int neigCount = -1;
    public int numWall = -1;

    #region Constructors
    public Tile()
    {
        this.roomID = -1;
        this.neigCount = -1;
        this.numWall = -1;
    }

    public object Clone()
    {
        return new Tile()
        {
            roomID = this.roomID,
            neigCount = this.neigCount,
            numWall = this.numWall
        };
    }
    #endregion
}

public class Map : ICloneable
{
    #region Properties
    public int Width{
        get {
            var min = int.MaxValue;
            var max = int.MinValue;

            foreach (var r in rooms)
            {
                foreach (var t in r.Value)
                {
                    if(t.Key.x < min)
                        min = t.Key.x;
                    if(t.Key.x > max)
                        max = t.Key.x;
                }

            }
            return max - min;
        }
    }
    public int Height {
        get
        {
            var min = int.MaxValue;
            var max = int.MinValue;

            foreach (var r in rooms)
            {
                foreach (var t in r.Value)
                {
                    if(t.Key.y < min)
                        min = t.Key.y;
                    if(t.Key.y > max)
                        max = t.Key.y;
                }

            }
            return max - min;
        }
    }



    public Vector2Int Center
    {
        get
        {
            var sum = Vector2Int.zero;
            var i = 0;
            foreach (var r in rooms)
            {
                foreach (var t in r.Value)
                {
                    sum.x += t.Key.x;
                    sum.y += t.Key.y;
                    i++;
                }
            }
            return new Vector2Int(sum.x / i, sum.y / i); // BUG?: si no hay rooms, se divide por 0
        }
    }

    public Tile[,] Data { get; set; }
    #endregion

    public Dictionary<int, Dictionary<Vector2Int, Tile>> rooms = new();


    #region Constructors
    public Map(int w, int h, int d)
    {
        this.Width = w;
        this.Height = h;
        Data = NewData(w, h);
    }

    public object Clone()
    {
        var map = new Map(this.Width, this.Height, 0);

        for (int i = 0; i < this.Width; i++)
        {
            for (int j = 0; j < this.Height; j++)
            {
                map.Data[i, j] = this.Data[i, j].Clone() as Tile;
            }
        }
        return map;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Return a new data with the size x,y
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Tile[,] NewData(int x, int y)
    {
        var data = new Tile[x, y];
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                data[i, j] = new Tile();
            }
        }

        return data;
    }

    /// <summary>
    /// 
    /// </summary>
    private void RecalculateTileRooms()
    {
        rooms.Clear();
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                var tile = Data[i, j];
                if(rooms.ContainsKey(tile.roomID))

            }
        }
    }

    /// <summary>
    /// Set the room id to the tiles in the positions list,
    /// and recalculate the neigthbors and walls.
    /// </summary>
    /// <param name="positions"></param>
    /// <param name="value"></param>
    public void SetRoomTiles(List<Vector2Int> positions, int value)
    {
        // set room owner id
        for (int i = 0; i < positions.Count; i++)
        {
            var x = positions[i].x;
            var y = positions[i].y;

            this.Data[x, y].roomID = value;
        }

        // recalcualte neig value and wall value
        var neigs = Utils.GetNeigborPositions(positions, Directions.directions_4);
        for (int i = 0; i < neigs.Count; i++)
        {
            var x = neigs[i].x;
            var y = neigs[i].y;

            this.Data[x, y].neigCount = CalcNeigthbors(x, y);
            this.Data[x, y].numWall = CalcWalls(x, y);
        }
    }

    /// <summary>
    /// Set the room id to the tiles in the rectangle,
    /// and recalculate the neigthbors and walls.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="value"></param>
    public void SetRoomTiles(Vector2Int min, Vector2Int max, int value)
    {
        var pos = new List<Vector2Int>();
        for (int i = min.x; i < max.x; i++)
        {
            for (int j = min.y; j < max.y; j++)
            {
                pos.Add(new Vector2Int(i, j));
            }
        }
        SetRoomTiles(pos, value);
    }

    /// <summary>
    /// Clean the tiles in the positions list.
    /// </summary>
    /// <param name="positions"></param>
    public void CleanTiles(List<Vector2Int> positions)
    {
        // set room owner to 0
        for (int i = 0; i < positions.Count; i++)
        {
            var x = positions[i].x;
            var y = positions[i].y;

            this.Data[x, y] = new Tile();
        }
    }

    /// <summary>
    /// Return the number of neigthbors that the tile has in the 8 directions.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private int CalcNeigthbors(int x, int y)
    {
        var bitArray = new BitArray(8);
        for (int i = 0; i < Directions.directions_8.Count; i++)
        {
            var dir = Directions.directions_8[i];
            var nx = x + dir.x;
            var ny = y + dir.y;

            if (ContainPosition(nx, ny))
                continue;

            if (this.Data[nx, ny].roomID != 0)
            {
                bitArray[i] = true;
            }
        }

        return Utils.BitArrayToInt(bitArray);
    }

    /// <summary>
    /// Return the number of walls that the tile has in the 4 directions.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private int CalcWalls(int x, int y)
    {
        var toR = 0;
        for (int i = 0; i < Directions.directions_4.Count; i++)
        {
            var dir = Directions.directions_4[i];
            var nx = x + dir.x;
            var ny = y + dir.y;

            if (ContainPosition(nx, ny))
                continue;

            if (this.Data[nx, ny].roomID != this.Data[x, y].roomID)
            {
                toR++;
            }
        }
        return toR;
    }

    /// <summary>
    /// Return if the position is in or out of the map.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool ContainPosition(int x, int y)
    {
        return x < 0 || x >= this.Width || y < 0 || y >= this.Height;
    }

    /// <summary>
    /// Print the map in the console.
    /// </summary>
    public void Print()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                var msg = "[" + i + "," + j + "] = " +
                    "ID: " + Data[i, j].roomID + ", " +
                    "Neig: " + Data[i, j].neigCount + ", " +
                    "Walls: " + Data[i, j].numWall;

                Debug.Log(msg);
            }
        }
    }

    // FIX: Esto es optimizable, Se pude ahorrar la intancia de nuevo data y
    // solo añadir los necesarios. Ademas se puede poner que horizontal y vertical
    // se puedan extender a la vez.
    public void ExtendBorder(Directions.Dirs_4 dir, int value) // TODO: Add CLOCK
    {
        value = Mathf.Abs(value);
        Tile[,] data = null;

        switch (dir)
        {
            case Directions.Dirs_4.Right:
                Width += value;
                data = NewData(Width, Height);
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        data[i, j] = Data[i, j];
                    }
                }
                break;
            case Directions.Dirs_4.Up:
                Height += value;
                data = NewData(Width, Height);
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        Data[i, j] = Data[i, j];
                    }
                }
                break;
            case Directions.Dirs_4.Left:
                Width += value;
                data = NewData(Width, Height);
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        data[i + value, j] = Data[i, j];
                    }
                }
                break;
            case Directions.Dirs_4.Down:
                Height += value;
                data = NewData(Width, Height);
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        data[i, j + value] = Data[i, j];
                    }
                }
                break;
            case Directions.Dirs_4.None: // Do nothing
                    break;
            default:
                Debug.LogWarning("Invalid direction");
                break;
        }
    }

    // FIX: Esto es optimizable, Se pude ahorrar la intancia de nuevo data y
    // solo añadir los necesarios. Ademas se puede poner que horizontal y vertical
    // se puedan extender a la vez.
    public void RetractBorder(Directions.Dirs_4 dir, int value)
    {
        value = Mathf.Abs(value);

        Tile[,] data = null;

        switch (dir)
        {
            case Directions.Dirs_4.Right:
                data = NewData(Width - value, Height);
                for (int i = 0; i < Width - value; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        data[i, j] = Data[i, j];
                    }
                }
                break;
            case Directions.Dirs_4.Up:
                data = NewData(Width, Height - value);
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height - value; j++)
                    {
                        Data[i, j] = Data[i, j];
                    }
                }
                break;
            case Directions.Dirs_4.Left:
                data = NewData(Width - value, Height);
                for (int i = 0; i < Width - value; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        data[i, j] = Data[i + value, j];
                    }
                }
                break;
            case Directions.Dirs_4.Down:
                data = NewData(Width, Height - value);
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height - value; j++)
                    {
                        data[i, j] = Data[i, j + value];
                    }
                }
                break;
            default:
                Debug.LogError("Invalid direction");
                break;
        }
    }
    #endregion
}
