using Optimization.Evaluators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Optimization.Selectors
{
    public interface ISelection
    {
        public List<(object, string)> Execute(List<(object, string)> population, IEvaluator evaluator);
    }
}

namespace Optimization.Terminators
{
    public interface ITerminator
    {
        public bool Execute();
    }

    public class AgregateTermination : ITerminator
    {
        public ITerminator[] terminators;

        public bool Execute()
        {
            foreach (var t in terminators)
            {
                if (t.Execute())
                    return true;
            }
            return false;
        }
    }

    public class IterationTerminator : ITerminator
    {
        public int maxIterations;
        private int currentIteration = 0;

        public bool Execute()
        {
            currentIteration++;
            return currentIteration >= maxIterations;
        }
    }

    public class ManualTerminator : ITerminator
    {
        public bool Execute()
        {
            return Event.current.keyCode == KeyCode.Escape;
            //return Input.GetKeyDown(KeyCode.Escape);
        }
    }

    public class TimeTerminator : ITerminator
    {
        public float maxTime;
        private float startTime;

        private bool inited = false;

        public bool Execute()
        {
            if(inited)
            {
                inited = true;
                startTime = Time.time;
            }

            return Time.time - startTime >= maxTime;
        }
    }
}

namespace Problem.Evaluators
{
    public class VoidEvaluator : IEvaluator
    {
        public float Execute(object obj) // CORRECT
        {
            var map = (Map)obj;

            float boundArea = map.Width + map.Height;
            float voidArea = boundArea - map.Area;

            return 1f - (voidArea / boundArea);
        }
    }

    public class ExteriorWallEvaluator : IEvaluator
    {
        public float Execute(object obj)
        {
            var map = (Map)obj;

            var n = 0;
            float min = 2f * (map.Width + map.Height); // circumference
            float max = 2f * min;

            foreach (var r in map.rooms)
            {
                foreach (var (pos, tile) in r.Value)
                {
                    n += map.WallsValue(pos.x, pos.y); // FIX: no esta sumando nada a n
                }
            }

            return 1f - (n / max);
            //return 1f - ((n - min) / (max - min)); // FIX: si n es menor que min, el resutlado mas que 1
        }
    }

    public class CornerEvaluator : IEvaluator
    {
        public float Execute(object obj)
        {
            var map = (Map)obj;

            var n = 0;
            float min = 4 * map.rooms.Count;
            float max = map.Height * map.Width;

            foreach (var (id, room) in map.rooms)
            {
                var c = map.GetConcaveCorners(id); // FIX?: no esta encontrando esquinas en el mapa de points
                c.AddRange(map.GetConvexCorners(id)); // FIX?: no esta encontrando esquinas en el mapa de points

                n += c.Count;
            }

            return 1 - (n / max);
            //return 1 - ((n - min) / (max - min)); // FIX: si n es menor que min, el resutlado mas que 1
        }
    }
}

namespace Optimization.Evaluators
{
    public interface IEvaluator
    {
        public float Execute(object obj);
    }

    public class WeightedAgregateEvaluator : IEvaluator
    {
        public (IEvaluator, float)[] evs;

        public float Execute(object obj)
        {
            if (evs == null || evs.Length == 0) { Debug.LogWarning("No Tiene Sub-Evalaudores."); return -1; }

            var map = (Map)obj;

            var total = 0f;
            foreach (var e in evs)
            {
                var (evaluator, weight) = e;
                var value = evaluator.Execute(map);
                total += value * weight;
            }
            return total;
        }
    }

}

namespace Optimization.Selector
{
    public interface ISelector
    {
        // neigbor, fitness, move
        public (object, float, string) Execute(List<(object, float, string)> population);
    }

    public class FirstBestSelector : ISelector
    {
        public (object, float, string) Execute(List<(object, float, string)> population)
        {
            return population.OrderByDescending(p => p.Item2).First();
        }
    }

    public class RandomBestSelector : ISelector
    {
        public (object, float, string) Execute(List<(object, float, string)> population)
        {
            var best = population.OrderByDescending(p => p.Item2).First();
            var bests = population.Where(p => p.Item2 == best.Item2).ToList();
            return bests[Random.Range(0, bests.Count)];
        }
    }

    public class RandomBestPercentil : ISelector
    {
        float percentil = 0.1f;

        public (object, float, string) Execute(List<(object, float, string)> population)
        {
            var sorted = population.OrderByDescending(p => p.Item2);
            var amount = (int)(population.Count * percentil);
            var bests = sorted.Take(amount).ToList();
            return bests[Random.Range(0, bests.Count)];
        }
    }

    public class RandomSelector : ISelector
    {
        public (object, float, string) Execute(List<(object, float, string)> population)
        {
            return population[Random.Range(0, population.Count)];
        }
    }
}

public class TestUtils 
{
    public static Graph ExampleGraph()
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

        graph.edges.Add(new Graph.Edge() { n1 = graph.nodes[0], n2 = graph.nodes[1] });
        graph.edges.Add(new Graph.Edge() { n1 = graph.nodes[0], n2 = graph.nodes[2] });
        graph.edges.Add(new Graph.Edge() { n1 = graph.nodes[0], n2 = graph.nodes[3] });
        graph.edges.Add(new Graph.Edge() { n1 = graph.nodes[1], n2 = graph.nodes[2] });
        graph.edges.Add(new Graph.Edge() { n1 = graph.nodes[1], n2 = graph.nodes[3] });
        graph.edges.Add(new Graph.Edge() { n1 = graph.nodes[2], n2 = graph.nodes[3] });

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
