using Optimization.Evaluators;
using Optimization.Neigbors;
using Optimization.Restrictions;
using Optimization.Terminators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class Data // TODO: implementar despues de probar que el esxperimento funciona
{
    public struct generacion
    {
        public float totalTime;
        public float explorationTime;
        public float evaluatorTime;
        public float genBest;
        public (List<Map>, float) best;
        public float average;
        public int successfulPathCount;
        public int deadEndCount;
    }

    public List<generacion> generations = new ();

    // tiempo total de ejecucion
    public float totalTime;
}

public class ACO
{
    public Dictionary<Map, Dictionary<Map, float>> mapNeigs = new(); // from -> L(to, pheromone)

    public bool enforceGraph = true;

    public Data data;

    public (List<Map>,Data) Execute(Graph graph, int antCount,float pherIntnesity, float disipacion, IEvaluator evaluator, ITerminator terminator,IRestriction restriction)
    {
        var data = new Data();// <- For TESTING

        float bestEval = float.MinValue;
        List<Map> best = new();
        

        // obtengo los nodos ordenados por la cantidad de vecinos
        var predeterminePath = GetNodeSortedByNeigs(graph);

        var swTotal = new System.Diagnostics.Stopwatch(); // <- For TESTING
        swTotal.Start();// <- For TESTING
        // cata iteracion del while es una nueva generacion de hormigas
        while (!terminator.Execute())
        {
            float bestGenEval = float.MinValue;
            List<Map> bestGen = new();
            float averageCumulated = 0;

            var gen = new Data.generacion();
            var swGen = new System.Diagnostics.Stopwatch(); // <- For TESTING
            swGen.Start();// <- For TESTING

            var paths = new List<List<Map>>(); // last node added, currentmap

            var swExplore = new System.Diagnostics.Stopwatch(); // <- For TESTING
            swExplore.Start();// <- For TESTING
            // cada hormiga explora un camino hasta llegar a completar un mapa con todas las habitaciones
            for (int i = 0; i < antCount; i++)
            {
                var path = AntPath(graph, predeterminePath, restriction);
                paths.Add(path);
            }
            swExplore.Stop();// <- For TESTING
            gen.explorationTime = swExplore.ElapsedMilliseconds;// <- For TESTING

            var swEval = new System.Diagnostics.Stopwatch(); // <- For TESTING
            swEval.Start();// <- For TESTING
            // por cada camino regreso las hormigas y aplico la pheromona
            foreach (var path in paths)
            {
                // si la hormiga no termino el camino
                if (path.Count < graph.nodes.Count)
                {
                    gen.deadEndCount++; // <- For TESTING
                    continue; // no suma feromonas
                }

                var last = path.Last();
                
                // evaluo el camino
                var pathEval = evaluator.Execute(last);
                averageCumulated += pathEval;

                if (bestGenEval < pathEval)
                {
                    bestGenEval = pathEval;
                    bestGen = path;
                }

                // actualizo feromonas
                for (int i = 0; i < path.Count - 1; i++)
                {
                    var cur = path[i];
                    var next = path[i + 1];

                    // VER Si ESTO es la mejor forma de aumentar el valor de la feromona
                    mapNeigs[cur][next] += (pathEval * pherIntnesity);
                }
            }
            swEval.Stop();
            gen.evaluatorTime = swEval.ElapsedMilliseconds; // <- For TESTING

            // disminuyo la feromona
            foreach (var (from, edge) in mapNeigs)
            {
                var ms = edge.Select(e => e.Key).ToList();
                foreach (var m in ms)
                {
                    edge[m] *= disipacion;
                }
            }

            swGen.Stop();
            gen.totalTime = swGen.ElapsedMilliseconds; // <- For TESTING
            gen.successfulPathCount = paths.Count; // <- For TESTING
            gen.deadEndCount = antCount - paths.Count; // <- For TESTING
            if (bestEval < bestGenEval)
            {
                bestEval = bestGenEval;
                best = bestGen;
            }
            gen.genBest = bestGenEval; // <- For TESTING
            gen.best = (best, bestEval); // <- For TESTING
            gen.average = (paths.Count != 0)? averageCumulated/(paths.Count) : -1; // <- For TESTING
            data.generations.Add(gen); // <- For TESTING

        }
        swTotal.Stop(); // <- For TESTING
        data.totalTime = swTotal.ElapsedMilliseconds; // <- For TESTING

        return (best, data);
    }

    /// <summary>
    /// XXX
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private List<Map> AntPath(Graph graph, List<(Graph.Node, Graph.Node)> path,IRestriction restriction)
    {
        Map current = GenerateFirst(path[0].Item2);
        var pathList = new List<Map>();
        pathList.Add(current);

        for (int i = 1; i < path.Count; i++)
        {
            var (nPrev, nCurrent) = path[i];

            if (!mapNeigs.ContainsKey(current))
            {
                mapNeigs.Add(current, new Dictionary<Map, float>());
            }

            //var neigs = mapNeigs[current];
            mapNeigs.TryGetValue(current, out var neigs);

            // si ya explore alguno continua algun de esos 
            var validAmount = ValidsPathAmount(neigs);
            var rValue = UnityEngine.Random.Range(0f, validAmount + 1f);
            if (rValue < validAmount)
            {
                // si existe elijo por pheromona
                var select = neigs.ToList().RandomRullete(n => n.Value);
                pathList.Add(select.Key);
                current = select.Key;
            }
            else // o decido si seguir explorando
            {
                // creo un nuevo diccionario
                var (neig, value) = AntExplore(current, graph, nPrev, nCurrent, restriction);

                if(value > 0)
                {
                    pathList.Add(neig);
                    current = neig;
                }
                else
                {
                    return pathList;
                }
            }
        }

        return pathList;
    }

    public int ValidsPathAmount(Dictionary<Map,float> neigs)
    {
        var sum = 0;
        for (int j = 0; j < neigs.Count; j++)
        {
            sum += (neigs.ElementAt(j).Value > 0) ? 1 : 0;
        }
        return sum;
    }

    private List<Map> GetRoots(Map map)
    {
        var toR = new List<Map>();
        foreach (var (from, to) in mapNeigs)
        {
            if (to.ContainsKey(map))
            {
                toR.Add(from);
            }
        }
        return toR;
    }

    private Map GenerateFirst(Graph.Node node)
    {
        var init = new Map();
        var current = node;

        // selecciono un tamaño para la nueva habitacion
        var w = UnityEngine.Random.Range((int)node.minArea.x, (int)node.maxArea.x + 1);
        var h = UnityEngine.Random.Range((int)node.minArea.y, (int)node.maxArea.y + 1);

        init.SetRoomTiles(new Vector2Int(0, 0), new Vector2Int(w, h), node.id);

        return init;
    }

    /*
     *      if (!transitable)
                    {
                        var roots = GetRoots(current);
                        foreach (var root in roots)
                            mapNeigs[root][current] = 0;

                        return pathList;
                    }*/



    public (Map, float) AntExplore(Map current, Graph graph, Graph.Node nodePrev, Graph.Node nodeCurrent, IRestriction restriction)
    {
        // selecciono un tamaño para la nueva habitacion
        var w = UnityEngine.Random.Range((int)nodeCurrent.minArea.x, (int)nodeCurrent.maxArea.x + 1);
        var h = UnityEngine.Random.Range((int)nodeCurrent.minArea.y, (int)nodeCurrent.maxArea.y + 1);

        // obtengo los posibles pivotes para ese tamaño
        var pivots = GetPivotsNeigs(nodePrev.pos + nodeCurrent.pos,new Vector2Int(w,h), nodePrev.id, current);

        // selecciono uno aleatorio
        var pivot = pivots.GetRandom();
        var other = current.Clone() as Map;
        other.SetRoomTiles(pivot, pivot + nodeCurrent.maxArea, nodeCurrent.id);
        
        // no son añadidos si no cumplen con la restriccion
        if (restriction.Execute(new Tuple<Map, Graph>(other, graph)))
        {
            if (!mapNeigs[current].ContainsKey(other))
            {
                mapNeigs[current].Add(other, 1);
            }
            return (other, 1);

        }
        else
        {
            if (!mapNeigs[current].ContainsKey(other))
            {
                mapNeigs[current].Add(other, 0);
            }
            return (other, 0);
        }
    }

    private List<Map> GenerateNeigs(Map prev, Graph graph, Graph.Node nodePrev, Graph.Node nodeCurrent)
    {
        var toR = new List<Map>();

        // obtengo los posibles pivotes
        var pivots = GetPivotsNeigs(nodePrev.pos + nodeCurrent.pos, nodeCurrent.maxArea,nodePrev.id, prev);

        // por cada pivote
        for (int i = 0; i < pivots.Count; i++)
        {
            var other = prev.Clone() as Map;
            other.SetRoomTiles(pivots[i], pivots[i] + nodeCurrent.maxArea, nodeCurrent.id);
            toR.Add(other);
        }

        return toR;
    }

    /// <summary>
    /// regresa una lista de nodos ordenados desde el nodo con mas vecinos,
    /// para luego obtener los vecinos de este y regresarlos ordenados por
    /// la cantidad de vecinos, asi susesibamente.
    /// </summary>
    /// <param name="graph"></param>
    /// <returns> a tuple [from -> to]</returns>
    private List<(Graph.Node, Graph.Node)> GetNodeSortedByNeigs(Graph graph)
    {
        var added = new List<Graph.Node>();
        var toR = new List<(Graph.Node, Graph.Node)>();
        var toAdd = new Queue<(Graph.Node, Graph.Node)>();

        var first = graph.nodes.OrderByDescending(n => graph.GetNeighbors(n).Count).First();
        toAdd.Enqueue((first, first));

        while (toAdd.Count > 0)
        {
            var (prev, current) = toAdd.Dequeue();

            // si ya esta en la lista lo salto
            if(added.Contains(current))
                continue;

            toR.Add((prev, current));
            added.Add(current);

            var neigs = graph.GetNeighbors(current);
            neigs = neigs.OrderByDescending((n) => {
                var amount = added.Intersect(graph.GetNeighbors(n)).Count();
                return amount;
                }).ToList();
            foreach (var n in neigs)
            {
                if (!added.Contains(n))
                    toAdd.Enqueue((current, n));
            }
        }
        return toR;
    }

    /// <summary>
    /// con la direccion entre la habitacion que quiero añadir y la habitacion que
    /// ya esta en el mapa, obtengo los posibles pivotes para añadir la nueva habitacion
    /// </summary>
    /// <param name="nodeVector"></param>
    /// <param name="sizeArea"></param>
    /// <param name="roomID"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    private List<Vector2Int> GetPivotsNeigs(Vector2 nodeVector, Vector2Int sizeArea, int roomID, Map map)
    {
        var toR = new List<Vector2Int>();

        // obtengo la direcciones a la que se encuentra la nueva habitacion
        var dirs = !enforceGraph ?
            Directions.AllDirs():
            Directions.AngulatedDirs(nodeVector);


        // obtengo la habitacion anterior
        var room = map.rooms[roomID];

        foreach (var dir in dirs)
        {
            if (dir == Directions.Dirs_4.None)
                continue;

            // obtengo las posiciones vacias
            var emptyPos = GetAdjacent(dir, room);

            // obtengo el desplazamiento principal
            var starDisp = (StartDisp(dir) * sizeArea);// - StartDisp(dir);
            //var v1 = Vector2Int.zero;

            // obtengo el desplazamiento secundario
            var endDisp = (EndDisp(dir) * sizeArea);// - EndDisp(dir);

            foreach (var pos in emptyPos)
            {
                // obtengo la cantidad de desplazamientos secundarios
                var dispacedPoints = Utils.GetPointsBetween(pos + starDisp, pos + endDisp);

                foreach (var point in dispacedPoints)
                {
                    // obtengo el pivote
                    //var pivot = pos + v2;//v1 + v2;

                    // si no esta en la lista lo añado
                    if (!toR.Contains(point))
                        toR.Add(point);

                    // si no esta en la lista lo añado
                    //if (!toR.Contains(pivot))
                    //    toR.Add(pivot);
                }

            }
        }

        return toR;  // FIX: los numeros podrian estar mas distantes de lo que deberian
    }

    private List<Vector2Int> GetAdjacent(Directions.Dirs_4 dir,Dictionary<Vector2Int,Tile> room)
    {
        var toR = new List<Vector2Int>();
        foreach (var (pos, tile) in room)
        {
            var adjacentTile = pos + Directions.directions_4[(int)dir];
            Tile t = null;
            if (!room.TryGetValue(adjacentTile, out t))
            {
                toR.Add(adjacentTile);
            }
        }
        return toR;
    }

    private Vector2Int EndDisp(Directions.Dirs_4 dir)
    {
        switch (dir)
        {
            case Directions.Dirs_4.Up:
                return new Vector2Int(0, 0);
            case Directions.Dirs_4.Down:
                return new Vector2Int(0, -1);
            case Directions.Dirs_4.Left:
                return new Vector2Int(-1, 0);
            case Directions.Dirs_4.Right:
                return new Vector2Int(0, 0);
            default:
                return Vector2Int.zero;
        }
    }

    private Vector2Int StartDisp(Directions.Dirs_4 dir)
    {
        switch (dir)
        {
            case Directions.Dirs_4.Up:
                return new Vector2Int(-1, 0);
            case Directions.Dirs_4.Down:
                return new Vector2Int(-1, -1);
            case Directions.Dirs_4.Left:
                return new Vector2Int(-1, -1);
            case Directions.Dirs_4.Right:
                return new Vector2Int(0, -1);
            default:
                return Vector2Int.zero;
        }
    }
}

/*
public class ACO2
{
    public class EdgeACO
    {
        public Map prev;
        public (Vector2Int, int) move;
        // move es el ultimo movimeinto pero creo que necesito la ruta completa

        public Map next = null;
        public float pheromone = 1;

        public Map Walk(Graph graph)
        {
            if (next == null)
            {
                var otherRoom = graph.nodes[move.Item2];

                var other = prev.Clone() as Map;
                var wNeig = (int)otherRoom.maxArea.x;
                var hNeig = (int)otherRoom.maxArea.y;
                other.SetRoomTiles(
                    move.Item1,
                    new Vector2Int(wNeig, hNeig),
                    move.Item2);
            }

            return next;
        }
    }

    public Dictionary<List<Graph.Node>, EdgeACO> edges = new(new ListComparer<Graph.Node>());

    public List<Map> Execute(Graph graph, int antCount,float disipacion, IEvaluator evaluator, ITerminator terminator)
    {
        (Map,float) best = (null,-1);

        // obtengo los nodos ordenados por la cantidad de vecinos
        var predeterminePath = GetNodeSortedByNeigs(graph);

        // cata iteracion del while es una nueva generacion de hormigas
        while (!terminator.Execute())
        {
            var paths = new List<List<EdgeACO>>();

            // cada hormiga explora un camino hasta llegar a completar un mapa con todas las habitaciones
            for (int i = 0; i < antCount; i++)
            {
                var path = AntPath_FullRandom(graph);
                paths.Add(path);
            }

            // por cada camino regreso las hormigas y aplico la pheromona
            foreach (var path in paths)
            {
                // evaluo el camino
                var pathEval = evaluator.Execute(path);

                // actualizo feromonas
                foreach (var edge in path)
                {
                    edge.pheromone = pathEval; // VER Si ESTO es la mejor forma de aumentar el valor de la feromona
                }
            }

            // disminuyo la feromona
            foreach (var (map,edge) in edges)
            {
                edge.pheromone *= disipacion;
            }
        }

        return null;
    }

    private List<EdgeACO> AntPath(Graph graph,List<(Graph.Node,Graph.Node)> path)
    {
        var queue = new Queue<(Graph.Node,Graph.Node)>(path);

        var added = new List<Graph.Node>();

        while (queue.Count > 0)
        {
            var (prev,current) = queue.Dequeue();
            added.Add(current);

            // ver si se ha creado un mapa a partir de este nodo
            if (edges.ContainsKey(added))
            {
                var edge = edges[added];
                edge.Walk(graph);

            }
            else
            {
                var w = (int)added[0].maxArea.x;
                var h = (int)added[0].maxArea.y;
                var map = new Map();
                map.SetRoomTiles(new Vector2Int(0, 0), new Vector2Int(w - 1, h - 1), 0);
                edges.Add(added, new EdgeACO()
                {
                    prev = map,
                    move = (Vector2Int.zero, id)
                });
            }
        }

        return null;
    }


    private List<EdgeACO> AntPath_FullRandom(Graph graph) // TODO: TERMINAR DE IMPLEMENTAR
    {
        // elegir un nodo aleatorio
        var id = UnityEngine.Random.Range(0, graph.nodes.Count);
        var first = graph.nodes[id];
        var added = new List<Graph.Node>() { first };

        // ver si se ha creado un mapa a partir de este nodo
        if(!edges.ContainsKey(added))
        {
            var w = (int)added[0].maxArea.x;
            var h = (int)added[0].maxArea.y;
            var map = new Map();
            map.SetRoomTiles(new Vector2Int(0, 0), new Vector2Int(w - 1, h - 1), 0);
            edges.Add(added, new EdgeACO() {
                prev = map,
                move = (Vector2Int.zero, id)
            });
        }

        var toAdd = new List<Graph.Node>();

        // obtengo los vecinos del current
        var neig = graph.GetNeighbors(first);
        
        // quito lo que ya esta en "added"
        neig = neig.Where(n => !added.Contains(n)).ToList();
        toAdd.AddRange(neig);

        do
        {
            // elijo un toAdd aleatorio
            var n = toAdd.GetRandom();

            // creo una nueva llave para el diccionario
            var key = new List<Graph.Node>();
            key.AddRange(added);
            key.Add(n);

            // si exite en el diccionario uso esa
            if (edges.ContainsKey(key))
            {
                var edge = edges[key];
                added.Add(n);
                toAdd.Remove(n);
            }
            else
            {
                // si no existe la creo
                var edge = new EdgeACO();
                edge.prev = edges[added].Walk(graph);
                edge.move = (Vector2Int.zero, id);
                edges.Add(key, edge);
            }


            neig = graph.GetNeighbors(toAdd[0]);


        }while (toAdd.Count > 0);


            return null;
    }

    /// <summary>
    /// regresa una lista de nodos ordenados desde el nodo con mas vecinos,
    /// para luego obtener los vecinos de este y regresarlos ordenados por
    /// la cantidad de vecinos, asi susesibamente.
    /// </summary>
    /// <param name="graph"></param>
    /// <returns> a tuple [from -> to]</returns>
    private List<(Graph.Node,Graph.Node)> GetNodeSortedByNeigs(Graph graph)
    {
        var added = new List<Graph.Node>();
        var toR = new List<(Graph.Node,Graph.Node)>();
        var toAdd = new Queue<(Graph.Node,Graph.Node)>();

        var first = graph.nodes.OrderByDescending(n => graph.GetNeighbors(n).Count).First();
        toAdd.Enqueue((first,first));

        while (toAdd.Count > 0)
        {
            var (prev,current) = toAdd.Dequeue();
            toR.Add((prev, current));
            added.Add(current);

            var neigs = graph.GetNeighbors(current);
            neigs = neigs.OrderByDescending(n => added.Intersect(graph.GetNeighbors(n))).ToList();
            foreach (var n in neigs)
            {
                if (!added.Contains(n))
                    toAdd.Enqueue((current, n));
            }
        }
        return toR;
    }

    /// <summary>
    /// Determines if all nodes in the graph are connected.
    /// </summary>
    /// <param name="graph">The graph to check for connectivity.</param>
    /// <returns>True if all nodes are connected; otherwise, false.</returns>
    private bool IsAllConected(Graph graph)
    {
        var visited = new List<Graph.Node>();
        var toVisit = new Queue<Graph.Node>();
        toVisit.Enqueue(graph.nodes[0]);

        while (toVisit.Count > 0)
        {
            var current = toVisit.Dequeue();
            visited.Add(current);

            var neigs = graph.GetNeighbors(current);
            foreach (var n in neigs)
            {
                if(!visited.Contains(n))
                    toVisit.Enqueue(n);
            }
        }

        return visited.Count == graph.nodes.Count;
    }

    private List<Graph> SeparateSubGraph(Graph graph)
    {
        throw new NotImplementedException(); // TODO: Implement
    }


}

public class ListComparer<T> : IEqualityComparer<List<T>>
{
    public bool Equals(List<T> x, List<T> y)
    {
        if (x == null || y == null)
            return x == y;

        return x.SequenceEqual(y);
    }

    public int GetHashCode(List<T> obj)
    {
        return base.GetHashCode();
    }
}

/*
public class ACO2
{

    public List<Map> Execute(Graph graph)
    {
        var roomID = 0;
        var acoEdges = new List<(Map, Map, float)>(); // Map, Map, Pheromone

        var nodes = new List<Graph.Node>();
        nodes.AddRange(graph.nodes);
        nodes = nodes.OrderByDescending(n => graph.GetNeighbors(n).Count).ToList();

        var init = new Map();
        var current = nodes.First();
        var w = (int)current.maxArea.x;
        var h = (int)current.maxArea.y;
        init.SetRoomTiles(new Vector2Int(0, 0), new Vector2Int(w, h), 0);

        var neigs = graph.GetNeighbors(current).OrderByDescending(n => graph.GetNeighbors(n).Count).ToList();
        for (int i = 0; i < neigs.Count; i++)
        {
            var neig = neigs[i];

            // calculo los posibles pivotes
            var nv = current.pos + neig.pos;
            var pivots = GetPivotsNeigs(current.pos + neig.pos, neigs[i].maxArea, roomID, init); //??

            // por cada pivote
            for (int j = 0; j < pivots.Count; j++)
            {
                var other = init.Clone() as Map;
                var wNeig = (int)neigs[i].maxArea.x;
                var hNeig = (int)neigs[i].maxArea.y;
                other.SetRoomTiles(pivots[j],neigs[i].maxArea.Clone() as Vector2, roomID);
                acoEdges.Add((init,other, 0));
            }
        }


        acoEdges.Add((init, , 0));
    }

    public List<Map> Execute(Graph graph)
    {
        var map = new Map();
        var toR = new List<Map>();

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

        

        return toR;
    }


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
*/