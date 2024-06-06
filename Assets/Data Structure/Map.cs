using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

public class Tile : ICloneable
{
    public Map owner;
    public int roomID = -1;

    #region Constructors
    public Tile(Map owner)
    {
        this.roomID = -1;
        this.owner = owner;
    }

    public object Clone() // TODO: Impelentar el sistema de diccionario que permite clonar referencias.
    {
        return new Tile(null)
        {
            roomID = this.roomID,
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

    public int Width {
        get {
            var min = int.MaxValue;
            var max = int.MinValue;

            foreach (var r in rooms)
            {
                foreach (var t in r.Value)
                {
                    if (t.Key.x < min)
                        min = t.Key.x;
                    if (t.Key.x > max)
                        max = t.Key.x;
                }

            }
            return (max - min) + 1;
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
                    if (t.Key.y < min)
                        min = t.Key.y;
                    if (t.Key.y > max)
                        max = t.Key.y;
                }

            }
            return (max - min) + 1;
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
                var nTile = t.Value.Clone() as Tile;
                nTile.owner = map;
                room.Add(t.Key, nTile);
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
                tile = new Tile(this);
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
    }

    /// <summary>
    /// Retrieves a list of walls comprising a room's perimeter, 
    /// each represented by points and their facing direction.
    /// </summary>
    /// <param name="roomID">The room identifier.</param>
    /// <returns>A list of wall points and their facing direction.</returns>
    public List<(Vector2Int[],Vector2Int)> GetWalls(int roomID)
    {
        var r = rooms[roomID];
        var walls = new List<(Vector2Int[],Vector2Int)>();
        var horizontal = GetHorizontalWalls(roomID);
        var vertical = GetVerticalWalls(roomID);
        walls.AddRange(horizontal);
        walls.AddRange(vertical);
        return walls;
    }

    /// <summary>
    /// Get a list of points that form the wall and the direction of the wall.
    /// </summary>
    /// <param name="roomID"></param>
    /// <returns></returns>
    internal List<Vector2Int> GetCorners(int roomID)
    {
        var corners = GetConcaveCorners(roomID);
        corners.AddRange(GetConvexCorners(roomID));
        return corners;
    }

    /// <summary>
    /// Retrieves a list of convex corners within a specified room.
    /// </summary>
    /// <param name="roomID">The identifier of the room.</param>
    /// <returns>A list of Vector2Int representing convex corners positions.</returns>
    internal List<Vector2Int> GetConvexCorners(int roomID)
    {
        var pairs = rooms[roomID];
        var corners = new List<Vector2Int>();
        foreach (var (pos, tile) in pairs)
        {
            var value = NeigthborValue(pos.x,pos.y);

            if (NumbersSet.IsConvexCorner(value))
            {
                corners.Add(pos);
            }
        }
        return corners;
    }

    /// <summary>
    /// Retrieves a list of concave corners within a specified room.
    /// </summary>
    /// <param name="roomID">The identifier of the room.</param>
    /// <returns>A list of Vector2Int representing convex corners positions.</returns>
    internal List<Vector2Int> GetConcaveCorners(int roomID)
    {
        var pairs = rooms[roomID];
        var corners = new List<Vector2Int>();

        foreach (var p in pairs)
        {
            var (pos, tile) = p;
            var value = NeigthborValue(pos.x, pos.y);

            if (!NumbersSet.IsConcaveCorner(value))
                continue;

            var dirs = Directions.directions_4;
            for (int i = 0; i < dirs.Count; i++)
            {
                var oPos = pos + dirs[i];

                pairs.TryGetValue(oPos, out var other);

                if (other == null)
                    continue;

                var oValue = NeigthborValue(oPos.x, oPos.y);

                if (NumbersSet.IsWall(oValue))
                {
                    corners.Add(oPos);
                }
            }
        }
        return corners;
    }

    /// <summary>
    /// Get a list of points that form the wall and the direction of the wall.
    /// </summary>
    /// <param name="roomID"></param>
    /// <returns></returns>
    private List<(Vector2Int[],Vector2Int)> GetVerticalWalls(int roomID)
    {
        var room = rooms[roomID];
        var walls = new List<(Vector2Int[],Vector2Int)>();

        var convexCorners = GetConvexCorners(roomID);
        var allCorners = GetConcaveCorners(roomID);
        allCorners.AddRange(convexCorners);

        foreach (var current in convexCorners)
        {
            Vector2Int? other = null;
            int lessDist = int.MaxValue;
            foreach (var candidate in allCorners)
            {
                if (current == candidate)
                    continue;

                if (current.x - candidate.x != 0) // Comprobación para pared vertical
                    continue;

                var dist = Mathf.Abs(current.y - candidate.y);
                if (dist < lessDist)
                {
                    lessDist = dist;
                    other = candidate;
                }
            }

            if (other == null)
                other = current;

            if (walls.Any(w => (w.Item1.First() == other) && (w.Item1.Last() == current))) // Unnecessary?
                continue;

            var wallTiles = new List<Vector2Int>();
            var end = Mathf.Max(current.y, other?.y ?? 0);
            var start = Mathf.Min(current.y, other?.y ?? 0);

            for (int i = 0; i <= end - start; i++)
            {
                wallTiles.Add(new Vector2Int(current.x, start + i)); 
            }

            bool toRight = true;
            for (int i = 0; i < wallTiles.Count; i++)
            {
                var n = wallTiles[i] + Vector2Int.right;
                if (room.ContainsKey(n))
                {
                    toRight = false;
                    break;
                }
            }

            var dir = (toRight) ? Vector2Int.right : Vector2Int.left; // Cambiado para muro vertical
            walls.Add((wallTiles.ToArray(), dir));
        }
        return walls;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="roomID"></param>
    /// <returns></returns>
    private List<(Vector2Int[],Vector2Int)> GetHorizontalWalls(int roomID)
    {
        var room = rooms[roomID];
        var walls = new List<(Vector2Int[],Vector2Int)>();

        var convexCorners = GetConvexCorners(roomID);
        var allCorners = GetConcaveCorners(roomID);
        allCorners.AddRange(convexCorners);

        foreach (var current in convexCorners)
        {
            Vector2Int? other = null;
            int lessDist = int.MaxValue;
            foreach (var candidate in allCorners)
            {
                if (current == candidate)
                    continue;

                if (current.y - current.y != 0)
                    continue;

                var dist = Mathf.Abs(current.x - candidate.x);
                if (dist < lessDist)
                {
                    lessDist = dist;
                    other = candidate;
                }
            }

            if (other == null)
                other = current;

            if (walls.Any(w => (w.Item1.First() == other) && (w.Item1.Last() == current))) // UNESESARY?
                continue;

            var wallTiles = new List<Vector2Int>();
            var end = Mathf.Max(current.x,  other?.x ?? 00);
            var start = Mathf.Min(current.x, other?.x ?? 00);
            for (int i = 0; i <= end - start; i++)
            {
                wallTiles.Add(new Vector2Int(start + i, current.y));
            }

            bool toUp = true;
            for (int i = 0; i < wallTiles.Count; i++)
            {
                var n = wallTiles[i] + Vector2Int.up;
                if(room.ContainsKey(n))
                {
                    toUp = false;
                    break;
                }
            }

            var dir = (toUp) ? Vector2Int.up : Vector2Int.down;
           walls.Add((wallTiles.ToArray(),dir));
        }
        return walls;
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
    public int NeigthborValue(int x, int y)
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
    public int WallsValue(int x, int y)
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
    public ((Vector2Int[,], int[,], Tile[,]), int, int) ToTileMatrix()
    {
        var rect = Bounds;
        var tiles = new Tile[rect.width + 1, rect.height + 1];
        var cords = new Vector2Int[rect.width + 1, rect.height + 1];
        var roomID = new int[rect.width + 1, rect.height + 1];

        foreach (var r in rooms)
        {
            foreach (var t in r.Value)
            {
                var pivot = t.Key - rect.min;
                tiles[pivot.x, pivot.y] = t.Value;
                cords[pivot.x, pivot.y] = t.Key;
                roomID[pivot.x, pivot.y] = r.Key;
            }
        }
        return ((cords, roomID, tiles), rect.width + 1, rect.height + 1);
    }

    public (List<Vector2Int>, List<Tile>) GetNeigTiles(Vector2Int pos, List<Vector2Int> dirs)
    {
        var posR = new List<Vector2Int>();
        var tilesR = new List<Tile>();

        foreach (var d in dirs)
        {
            var dd = pos + d;

            foreach (var r in rooms)
            {
                posR.Add(dd);
                tilesR.Add(r.Value.ContainsKey(dd) ? r.Value[dd]: null);
            }
        }

        return (posR, tilesR);
    }


    /// <summary>
    /// Print the map in the console.
    /// </summary>
    public void Print()
    {
        var ((cords,rooms,tiles), w, h) = ToTileMatrix();

        var msg = "\n";
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                msg += rooms[i, j] + ", ";
            }
            msg += "\n";
        }
        Debug.Log(msg);
    }
    #endregion
}
