using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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

}

public static class Utils
{
    public static int BitArrayToInt(BitArray bits)
    {
        if (bits.Length != 8)
            throw new ArgumentException("El BitArray debe tener exactamente 8 bits.");

        byte[] bytes = new byte[1];
        bits.CopyTo(bytes, 0);
        return bytes[0];
    }

    public static BitArray IntToBitArray(int number)
    {
        return new BitArray(new[] { number });
    }

    public static void GenerateImage(Map map, string fileName, string path)
    {
        var colors = new Dictionary<int, Color>();

        var t = new Texture2D(map.Width, map.Height);
        for (int i = 0; i < map.Width; i++)
        {
            for (int j = 0; j < map.Height; j++)
            {
                var c = map.Data[i,j].roomID;

                if (!colors.ContainsKey(c))
                {
                    colors.Add(c, new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f)));
                    t.SetPixel(i, j, colors[c]);
                }
                t.SetPixel(i, j, colors[c]);
            }
        }

        byte[] pngData = t.EncodeToPNG();
        string filePath = Path.Combine(path, fileName);
        File.WriteAllBytes(filePath, pngData);

        Debug.Log("Textura guardada como " + fileName + " en la ruta: " + filePath);
    }

    // FIX?: Esto se podria hacer de otra forma pa que sea menos ciclos.
    public static List<Vector2Int> GetNeigborPositions(List<Vector2Int> pos, List<Vector2Int> dirs)
    {
        var toR = new List<Vector2Int>();
        toR.AddRange(pos);

        for (int i = 0; i < pos.Count; i++)
        {
            for (int j = 0; j < dirs.Count; j++)
            {
                var n = pos[i] + dirs[j];
                if (!toR.Contains(n))
                    toR.Add(n);
            }
        }
        return toR;
    }
}

public class TEST_1 : MonoBehaviour
{
    public List<int> InternalCorners = new List<int>();
    public List<int> ExternalCorners = new List<int>();

    void Start()
    {
        // init Seed
        UnityEngine.Random.InitState(117);

        //Create Graph
        var graph = SimpleGraph();

        // Point Constructive
        var pointMap = PointConstructive(graph);
        pointMap.Print();
        Utils.GenerateImage(pointMap,"Constructive_Point_Map.png", Application.dataPath);

        // Smart Constructive
        var smartMap = SmartConstructive(graph);
        smartMap.Print();
        Utils.GenerateImage(pointMap, "Constructive_Smart_Map.png", Application.dataPath);

        // Hill Climbing
        // var hillClimbing = new HillClimbing();

        // Simulated Annealing
        var simulatedAnnealing = new SimulatedAnnealing();
        simulatedAnnealing.Ejecute(pointMap, 1000f, (x) => 0, (x) => new List<Map>());

        // Tabu Search
        var tabuSearch = new TabuSearch();
        
    }

    public Graph SimpleGraph()
    {
        var graph = new Graph();
        graph.nodes.Add(new Graph.Node() {
            pos = new Vector2(0, 0),
            minArea = new Vector2(3, 3),
            maxArea = new Vector2(6, 6),
            c = Color.red 
        });
        graph.nodes.Add(new Graph.Node()
        {
            pos = new Vector2(-10, 0),
            minArea = new Vector2(3, 3),
            maxArea = new Vector2(6, 6),
            c = Color.blue
        });
        graph.nodes.Add(new Graph.Node()
        {
            pos = new Vector2(5, 10),
            minArea = new Vector2(3, 3),
            maxArea = new Vector2(6, 6),
            c = Color.green
        });
        graph.nodes.Add(new Graph.Node()
        {
            pos = new Vector2(5, -10),
            minArea = new Vector2(3, 3),
            maxArea = new Vector2(6, 6),
            c = Color.cyan
        });

        return graph;
    }

    public float VoidEvaluator(Map map)
    {
        return 0; // TODO: Implement
    }

    public float ExteriorWallEvaluator(Map map)
    {
        return 0; // TODO: Implement
    }

    public float CornerEvaluator(Map map)
    {
        return 0; // TODO: Implement
    }

    public List<Map> GetNeighbors(Map map)
    {
        return new List<Map>(); // TODO: Implement
    }

    /// <summary>
    /// Get a map with the rooms painted as 1 point each.
    /// </summary>
    /// <param name="graph"></param>
    /// <returns></returns>
    public Map PointConstructive(Graph graph)
    {
        // Init size map
        int maxX = int.MaxValue, maxY = int.MaxValue;
        int minX = int.MinValue, minY = int.MinValue;
        foreach (var node in graph.nodes)
        {
            if (node.pos.x + (node.maxArea.x/2f) > maxX)
                maxX = (int)node.pos.x;
            if (node.pos.y + (node.maxArea.y/2f) > maxY)
                maxY = (int)node.pos.y;
            if (node.pos.x - (node.maxArea.x/2f) < minX)
                minX = (int)node.pos.x;
            if (node.pos.y - (node.maxArea.y/2f) < minY)
                minY = (int)node.pos.y;
        }
        var w = maxX - minX;
        var h = maxY - minY;
        var offset = new Vector2(minX, minY);
        var map = new Map(w, h, 2);

        // paint Tiles
        for (int i = 0; i < graph.nodes.Count; i++)
        {
            var node = graph.nodes[i];
            var pos = node.pos - offset;

            map.SetRoomTiles(new List<Vector2Int>() { new Vector2Int((int)pos.x, (int)pos.y) }, i+1);
        }

        return map;
    }

    // AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
    /// <summary>
    /// Get a map with the rooms painted.
    /// This start with the room with more connections and try to connect with the others.
    /// This method ignore unconnected rooms.
    /// </summary>
    /// <param name="graph"></param>
    /// <returns></returns>
    public Map SmartConstructive(Graph graph) // FIX?: esto podria recivir una func para sacar max, min, rand, avrg de (w,h) inicial.
    {
        var added = new List<Graph.Node>();

        // Init
        var nodes = new List<Graph.Node>();
        nodes.AddRange(graph.nodes);
        nodes = nodes.OrderByDescending(n => graph.GetNeighbors(n).Count).ToList();

        // first Room
        var current = nodes.First();
        var w = (int)current.maxArea.x;
        var h = (int)current.maxArea.y;
        var map = new Map(w, h, 2);
        map.SetRoomTiles(new Vector2Int(0, 0), new Vector2Int(w - 1, h - 1), 0);
        var offset = map.Center - current.pos;
        nodes.Remove(current);
        added.Add(current);

        // others Rooms
        int i = 1;
        while (nodes.Count > 0)
        {
            var neig = graph.GetNeighbors(current);
            neig = neig.Except(added).OrderByDescending(n => graph.GetNeighbors(n).Count).ToList();

            if (neig.Count == 0)
            {
                current = nodes.First(); //Except(added).First();
                map.SetRoomTiles(new Vector2Int(0, 0), new Vector2Int(w - 1, h - 1), 0); // Fix
                nodes.Remove(current);
                added.Add(current);
                continue;
            }

            neig = neig.OrderByDescending(n => graph.GetNeighbors(n).Count).ToList();

            for (int j = 0; j < neig.Count; j++)
            {
                var distances = (current.maxArea/2f + neig[j].maxArea/2f); // <<
                var cath1 = distances.MinAbs();
                var dir = neig[j].pos - current.pos;
                var angle = Vector2.Angle(dir, Vector2.right);
                var cath2 = cath1 * Mathf.Cos(angle * Mathf.Deg2Rad);
                var midpoint = current.pos + new Vector2(cath1, cath2);

                var maxCord = midpoint + (neig[j].maxArea/2f); // <<
                var minCord = midpoint - (neig[j].maxArea/2f); // <<
                for (int k = 0; k < neig.Count; k++)
                {

                }

                
                
            }
            i++;
        }

        return map;
    }




}

public class Graph
{
    public struct Node
    {
        public Vector2 pos;
        public Vector2 minArea;
        public Vector2 maxArea;
        public Color c;
    }

    public struct Edge
    {
        public Node n1, n2;
    }

    public List<Node> nodes = new List<Node>();
    public List<Edge> edges = new List<Edge>();

    public Node GetMaxConection()
    {
        var dict = new Dictionary<Node, int>();

        foreach (var edge in edges)
        {
            if(dict.ContainsKey(edge.n1))
            {
                dict[edge.n1]++;
            }
            else
            {
                dict.Add(edge.n1, 1);
            }

            if (dict.ContainsKey(edge.n2))
            {
                dict[edge.n2]++;
            }
            else
            {
                dict.Add(edge.n2, 1);
            }
        }

        return dict.OrderByDescending(x => x.Value).First().Key;
    }

    public List<Node> GetNeighbors(Node node)
    {
        var neighbors = new List<Node>();
        foreach (var edge in edges)
        {
            if (edge.n1.Equals(node))
                neighbors.Add(edge.n2);
            if (edge.n2.Equals(node))
                neighbors.Add(edge.n1);
        }
        return neighbors;
    }
}