using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TEST_1 : MonoBehaviour
{
    public List<int> InternalCorners = new List<int>();
    public List<int> ExternalCorners = new List<int>();

    public NumbersSet numbersSet = new NumbersSet();

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
        //var smartMap = SmartConstructive(graph);
        //smartMap.Print();
        //Utils.GenerateImage(pointMap, "Constructive_Smart_Map.png", Application.dataPath);

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
        var boundArea = map.Width + map.Height;
        var voidArea = boundArea - map.Area;

        return 1 - (voidArea / boundArea);
    }

    public float ExteriorWallEvaluator(Map map)
    {
        var n = 0;
        var min = 2f * (map.Width + map.Height);
        var max = 2f * min;

        foreach (var r in map.rooms)
        {
            foreach (var t in r.Value)
            {
                n += t.Value.numWall;
            }
        }

        return 1 - ((n - min) / (max - min));
    }

    public float CornerEvaluator(Map map)
    {
        foreach (var r in map.rooms)
        {
            foreach (var t in r.Value)
            {
                var tile = t.Value;
                tile.neigCount;
            }
        }
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
        var map = new Map();

        var i = 0;
        foreach (var node in graph.nodes)
        {
            map.SetRoomTiles(new List<Vector2Int>() { new Vector2Int((int)node.pos.x, (int)node.pos.y) }, i);
            i++;
        }

        return map;
    }

    /// <summary>
    /// Get a map with the rooms painted.
    /// This start with the room with more connections and try to connect with the others.
    /// This method ignore unconnected rooms.
    /// </summary>
    /// <param name="graph"></param>
    /// <returns></returns>
    public Map SmartConstructive(Graph graph) // FIX?: esto podria recivir una func para sacar max, min, rand, avrg de (w,h) inicial.
    {
        throw new NotImplementedException();

        /*
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
        */
    }
}
