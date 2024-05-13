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
    public RectInt Bounds
    {
        get
        {
            var min = new Vector2Int(int.MaxValue, int.MaxValue);
            var max = new Vector2Int(int.MinValue, int.MinValue);

            foreach (var r in rooms)
            {
                foreach (var t in r.Value)
                {
                    if (t.Key.x < min.x)
                        min.x = t.Key.x;
                    if (t.Key.x > max.x)
                        max.x = t.Key.x;

                    if (t.Key.y < min.y)
                        min.y = t.Key.y;
                    if (t.Key.y > max.y)
                        max.y = t.Key.y;
                }

            }
            return new RectInt(min.x, min.y, max.x - min.x, max.y - min.y);
        }
    }

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

    public int Area
    {
        get
        {
            var area = 0;
            foreach (var r in rooms)
            {
                foreach (var t in r.Value)
                {
                    area++;
                }
            }
            return area;
        }
    }
    #endregion

    #region Variables
    public Dictionary<int, Dictionary<Vector2Int, Tile>> rooms = new();
    #endregion

    #region Constructors
    public Map()
    {
        
    }

    public object Clone()
    {
        var map = new Map();

        foreach (var r in this.rooms)
        {
            var room = new Dictionary<Vector2Int, Tile>();
            foreach (var t in r.Value)
            {
                room.Add(t.Key, t.Value.Clone() as Tile);
            }
            map.rooms.Add(r.Key, room);
        }
        return map;
    }
    #endregion

    #region Methods

    /// <summary>
    /// Set the room id to the tiles in the positions list,
    /// and recalculate the neigthbors and walls.
    /// </summary>
    /// <param name="positions"></param>
    /// <param name="roomID"></param>
    public void SetRoomTiles(List<Vector2Int> positions, int roomID)
    {
        // set room owner id
        for (int i = 0; i < positions.Count; i++)
        {
            var x = positions[i].x;
            var y = positions[i].y;

            Tile tile = null;
            foreach (var r in rooms)
            {
                var pos = new Vector2Int(x, y);
                if (r.Value.ContainsKey(pos))
                {
                    tile = r.Value[pos];
                    r.Value.Remove(new Vector2Int(x, y));
                    break;
                }
            }

            if (tile == null)
            {
                tile = new Tile();
                tile.roomID = roomID;

                if (!rooms.ContainsKey(roomID))
                {
                    rooms.Add(roomID, new Dictionary<Vector2Int, Tile>());
                }
                rooms[roomID].Add(new Vector2Int(x, y), tile);
            }
            else
            {
                tile.roomID = roomID;
                if (!rooms.ContainsKey(roomID))
                {
                    rooms.Add(roomID, new Dictionary<Vector2Int, Tile>());
                }
                rooms[roomID].Add(new Vector2Int(x, y), tile);
            }
        }

        // recalcualte neig value and wall value
        var dirs = Utils.GetNeigborPositions(positions, Directions.directions_8);
        for (int i = 0; i < dirs.Count; i++)
        {
            var x = dirs[i].x;
            var y = dirs[i].y;

            foreach (var r in rooms)
            {
                var pos = new Vector2Int(x, y);
                if (r.Value.ContainsKey(pos))
                {
                    r.Value[pos].neigCount = CalcNeigthbors(x, y);
                    r.Value[pos].numWall = CalcWalls(x, y);
                }
            }
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

            foreach (var r in rooms)
            {
                var pos = new Vector2Int(nx, ny);
                if (r.Value.ContainsKey(pos))
                {
                    bitArray[i] = true;
                }
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

            foreach (var r in rooms)
            {
                var pos = new Vector2Int(nx, ny);
                if (r.Value.ContainsKey(pos))
                {
                    toR++;
                }
            }
        }
        return toR;
    }

    /// <summary>
    /// Converts the rooms of the map into a tile matrix and returns the matrix along with its dimensions.
    /// </summary>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><description><b>Matrix of Tile:</b> A matrix of Tile objects representing the map, where each position corresponds to a tile on the map.</description></item>
    /// <item><description><b>Width:</b> The width of the matrix, which is the number of columns in the tile matrix.</description></item>
    /// <item><description><b>Height:</b> The height of the matrix, which is the number of rows in the tile matrix.</description></item>
    /// </list>
    /// </returns>
    public (Tile[,],int,int) ToTileMatrix()
    {
        var rect = Bounds;  
        var tiles = new Tile[rect.width, rect.height];

        foreach (var r in rooms)
        {
            foreach (var t in r.Value)
            {
                tiles[t.Key.x,t.Key.y] = t.Value;
            }
        }
        return (tiles,rect.width,rect.height);
    }


    /// <summary>
    /// Print the map in the console.
    /// </summary>
    public void Print()
    {
        var (tiles, w, h) = ToTileMatrix();

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                var msg = "[" + i + "," + j + "] = " +
                    "ID: " + tiles[i, j].roomID + ", " +
                    "Neig: " + tiles[i, j].neigCount + ", " +
                    "Walls: " + tiles[i, j].numWall;

                Debug.Log(msg);
            }
        }
    }
    #endregion
}
