using Optimization.Evaluators;
using Optimization.Terminators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ACO 
{
    public Dictionary<Map, Dictionary<Map, int>> neigEdge = new();

    public Map Execute(Map map,float antCount,IEvaluator evaluator, ITerminator terminator, Func<Map, List<Map>> GetNeighbors)
    {
        while(!terminator.Execute())
        {
            for(int i = 0;i < antCount;i++)
            {
                neigEdge.TryGetValue(map,out var neigs);
                if(neigs.Count <= 0)
                {
                    // Generate Neighbors
                    // evaluate neighbors
                    // Add to neigEdge
                }

                foreach(var (neig, value) in neigs)
                {
                    // Update Pheromone
                }

               
            }
        }

        return map;
    }

    public List<Map> AAA(Graph graph)
    {
        var map = new Map();

        // Init
        var added = new List<Graph.Node>();
        var nodes = new List<Graph.Node>();
        nodes.AddRange(graph.nodes);
        nodes = nodes.OrderByDescending(n => graph.GetNeighbors(n).Count).ToList();
        var roomID = 0;

        // First Node
        var current = nodes.First();
        var w = (int)current.maxArea.x;
        var h = (int)current.maxArea.y;
        map.SetRoomTiles(new Vector2Int(0, 0), new Vector2Int(w - 1, h - 1), 0);
        var offset = map.Center - current.pos;
        nodes.Remove(current);
        added.Add(current);

        var neigs = graph.GetNeighbors(current).OrderByDescending(n => graph.GetNeighbors(n).Count).ToList();
        foreach (var n in neigs)
        {
            var nv = current.pos + n.pos;
            var pos = GetPivotsNeigs(nv, n.maxArea, roomID, map);
        }

        // TDOO: TERMINAR DE IMPLEMENTAR

        

        return null;
    }

    /*
    private List<Map> GetNeig(Map current, Vector2Int pivot,Vector2int , int RoomID)
    {
        Map map = current.Clone() as Map;

        map.SetRoomTiles(,pivot,roomID)
    }
    */

    private List<Vector2Int> GetPivotsNeigs(Vector2 nodeVector, Vector2 sizeArea,int roomID, Map map)
    {
        var dirs = Directions.GetDirs(nodeVector);
        var room = map.rooms[roomID];

        var toR = new List<Vector2Int>();
        foreach (var dir in dirs)
        {
            if (dir == Directions.Dirs_4.None)
                continue;

            var emptyPos = new List<Vector2Int>();
            foreach (var (pos, tile) in room) 
            {
                var adjacentTile = pos + Directions.directions_4[(int)dir];
                Tile t = null;
                if (!room.TryGetValue(adjacentTile, out t))
                {
                    emptyPos.Add(adjacentTile);
                }
            }

            var dis = (DisplacementDir(dir) * sizeArea).ToInt();

            var otherMove = OtherMove(dir) * sizeArea;
            var otherCount = otherMove.MaxAbs();

            foreach (var pos in emptyPos)
            {
                for (var i = 0; i < otherCount; i++)
                {
                    var pivot = pos + dis + (otherMove * i).ToInt();
                    if (!toR.Contains(pivot))
                        toR.Add(pivot);
                }
            }
        }

        return toR;
    }

    private Vector2 OtherMove(Directions.Dirs_4 dir)
    {
        switch (dir)
        {
            case Directions.Dirs_4.Up:
                return new Vector2(1, 0);
            case Directions.Dirs_4.Down:
                return new Vector2(1, 0);
            case Directions.Dirs_4.Left:
                return new Vector2(0, -1);
            case Directions.Dirs_4.Right:
                return new Vector2(0, -1);
            default:
                return Vector2.zero;
        }
    }

    private Vector2 DisplacementDir(Directions.Dirs_4 dir)
    {
        switch (dir)
        {
            case Directions.Dirs_4.Up:
                return new Vector2(-1, 1);
            case Directions.Dirs_4.Down:
                return new Vector2(0, 1);
            case Directions.Dirs_4.Left:
                return new Vector2(-1, 1);
            case Directions.Dirs_4.Right:
                return new Vector2(1, 0);
            default:
                return Vector2.zero;
        }
    }


}
