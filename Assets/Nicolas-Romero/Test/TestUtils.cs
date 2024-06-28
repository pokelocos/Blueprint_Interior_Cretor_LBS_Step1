using Optimization.Evaluators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestUtils
{
    public static Graph ExampleGraph()
    {
        var graph = new Graph();
        graph.nodes.Add(new Graph.Node()
        {
            name = "A",
            id = 1,
            pos = new Vector2(0, 0),
            minArea = new Vector2Int(3, 3),
            maxArea = new Vector2Int(6, 6),
            color = Color.red
        });
        graph.nodes.Add(new Graph.Node()
        {
            name = "B",
            id = 2,
            pos = new Vector2(-10, 0),
            minArea = new Vector2Int(3, 3),
            maxArea = new Vector2Int(6, 6),
            color = Color.blue
        });
        graph.nodes.Add(new Graph.Node()
        {
            name = "C",
            id = 3,
            pos = new Vector2(5, 10),
            minArea = new Vector2Int(3, 3),
            maxArea = new Vector2Int(6, 6),
            color = Color.green
        });
        graph.nodes.Add(new Graph.Node()
        {
            name = "D",
            id = 4,
            pos = new Vector2(5, -10),
            minArea = new Vector2Int(3, 3),
            maxArea = new Vector2Int(6, 6),
            color = Color.cyan
        });

        graph.edges.Add(new Graph.Edge() { n1 = graph.nodes[0], n2 = graph.nodes[1] });
        graph.edges.Add(new Graph.Edge() { n1 = graph.nodes[0], n2 = graph.nodes[2] });
        graph.edges.Add(new Graph.Edge() { n1 = graph.nodes[0], n2 = graph.nodes[3] });
        graph.edges.Add(new Graph.Edge() { n1 = graph.nodes[1], n2 = graph.nodes[2] });
        graph.edges.Add(new Graph.Edge() { n1 = graph.nodes[1], n2 = graph.nodes[3] });
        graph.edges.Add(new Graph.Edge() { n1 = graph.nodes[2], n2 = graph.nodes[3] });

        return graph;
    }

    public static Graph ExampleGraph_Trianlge()
    {
        var graph = new Graph();
        graph.nodes.Add(new Graph.Node()
        {
            name = "A",
            id = 1,
            pos = new Vector2(0, 0),
            minArea = new Vector2Int(2, 2),
            maxArea = new Vector2Int(3, 3),
            color = Color.red
        });
        graph.nodes.Add(new Graph.Node()
        {
            name = "B",
            id = 2,
            pos = new Vector2(10, 10),
            minArea = new Vector2Int(2, 2),
            maxArea = new Vector2Int(3, 3),
            color = Color.blue
        });
        graph.nodes.Add(new Graph.Node()
        {
            name = "C",
            id = 3,
            pos = new Vector2(-10, -10),
            minArea = new Vector2Int(2, 2),
            maxArea = new Vector2Int(3, 3),
            color = Color.green
        });

        graph.edges.Add(new Graph.Edge() { n1 = graph.nodes[0], n2 = graph.nodes[1] });
        graph.edges.Add(new Graph.Edge() { n1 = graph.nodes[0], n2 = graph.nodes[2] });
        graph.edges.Add(new Graph.Edge() { n1 = graph.nodes[1], n2 = graph.nodes[2] });

        return graph;
    }

    /// <summary>
    /// Get a map with the rooms painted as 1 point each.
    /// </summary>
    /// <param name="graph"></param>
    /// <returns></returns>
    public static Map PointConstructive(Graph graph)
    {
        var map = new Map();

        var i = 1; // 0 is for empty tiles
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
