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

namespace Optimization
{
    public class ACO
    {
        public Dictionary<Map, Dictionary<Map, float>> mapNeigs = new(); // from -> L(to, pheromone)

        public Dictionary<(Map, Graph.Node), List<(Vector2Int, Vector2Int)>> movmenets = new(); // (from,currentNode) -> unused L(pivot,size) 

        public Data data = new();

        public bool enforceGraph = true;


        public (List<Map>, Data) Execute(Graph graph, int antCount, float pherIntnesity, float disipacion, IEvaluator evaluator, ITerminator terminator, IRestriction restriction)
        {
            // Reset data
            ResetData();

            // Init Best;
            float bestEval = float.MinValue;
            List<Map> best = new();

            // Sort by neigs amount
            var predeterminePath = GetNodeSortedByNeigs(graph);

            Timer.Start("ACO-TotalTime");

            var prevExplored = 0;

            // cata iteracion del while es una nueva generacion de hormigas
            while (!terminator.Execute())
            {
                // Data of the generation
                var gen = new Data.generacion();
                Timer.Start("ACO-GenerationTime");

                prevExplored = mapNeigs.Count;

                // Init Best of the generation
                float bestGenEval = float.MinValue;
                List<Map> bestGen = new();
                float averageCumulated = 0;

                // Store ant paths
                var paths = new List<List<Map>>();

                Timer.Start("ACO-ExplorationTime");

                // Each ant explore and return a path
                for (int i = 0; i < antCount; i++)
                {
                    var path = AntPathBT(graph, predeterminePath, restriction);
                    paths.Add(path);
                }
                
                gen.explorationTime = Timer.End("ACO-ExplorationTime");


                //var swEval = new System.Diagnostics.Stopwatch(); // <- For TESTING
                //swEval.Start();// <- For TESTING
                // por cada camino regreso las hormigas y aplico la pheromona
                foreach (var path in paths)
                {
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
                //swEval.Stop();
                //gen.evaluatorTime = swEval.ElapsedMilliseconds; // <- For TESTING

                // disminuyo la feromona
                foreach (var (from, edge) in mapNeigs)
                {
                    var ms = edge.Select(e => e.Key).ToList();
                    foreach (var m in ms)
                    {
                        edge[m] *= disipacion;
                    }
                }

                //swGen.Stop();
                //gen.totalTime = swGen.ElapsedMilliseconds; // <- For TESTING
                if (bestEval < bestGenEval)
                {
                    bestEval = bestGenEval;
                    best = bestGen;
                }
                //gen.genBest = bestGenEval; // <- For TESTING
                //gen.average = (paths.Count != 0) ? averageCumulated / (paths.Count) : -1; // <- For TESTING
                //gen.niegExplored = mapNeigs.Count - prevExplored; // <- For TESTING
                //data.generations.Add(gen); // <- For TESTING

            }

            data.totalTime = Timer.End("ACO-TotalTime");
            data.totalNeigsExplored = mapNeigs.Count; // <- For TESTING

            if (best.Count != 0)
            {
                data.best = (best, bestEval); // <- For TESTING
                return (best, data);
            }
            else
            {
                data.best = (bestNoCompleted, bestNCEval); // <- For TESTING
                return (bestNoCompleted, data);
            }

        }

        public void ResetData()
        {
            mapNeigs = new();
            movmenets = new();
            data = new();
        }

        /// <summary>
        /// Regresa una lista de mapas que representan el camino de una hormiga,
        /// si encuentra un camino que no cumple con las restricciones, regresa
        /// e intenta explorar otro camino.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<Map> AntPathBT(Graph graph, List<(Graph.Node, Graph.Node)> path, IRestriction restriction)
        {
            int FailAmount_NoChilds = 15;
            int FailAmount_InvalidStep = 15;

            Map currentMap = GenerateFirst(path[0].Item2);
            var pathList = new List<Map>();
            pathList.Add(currentMap);

            Map lastMap = null;
            for (int i = 1; i < path.Count; i++)
            {
                /*
                if (FailAmount_NoChilds <= 0) // parche para que no se quede pegao el codigo (PARCHE)
                {
                    Debug.Log("la hormiga fallo en encontrar caminos con hijos");
                    return pathList;
                }

                if(FailAmount_InvalidStep <= 0) // parche para que no se quede pegao el codigo (PARCHE)
                {
                    Debug.Log("la hormiga fallo en encontrar pasitos valido");
                    return pathList;
                }
                */

                var (nPrev, nCurrent) = path[i];

                if (!mapNeigs.ContainsKey(currentMap))
                {
                    mapNeigs.Add(currentMap, new Dictionary<Map, float>());
                }

                mapNeigs.TryGetValue(currentMap, out var neigs);

                // si ya explore alguno continua algun de esos 
                var validAmount = ValidsPathAmount(neigs);
                var rValue = UnityEngine.Random.Range(0f, validAmount + 1f); // el 1 podria ser una variable de ajuste (!!!)
                if (rValue < validAmount)
                {
                    // si existe elijo por pheromona
                    var select = neigs.ToList().RandomRullete(n => n.Value);
                    pathList.Add(select.Key);
                    currentMap = select.Key;
                }
                else // o decido si seguir explorando
                {
                    // pregunto si me quedan caminos por explorar SI NO, retorno
                    if (!CanExplore(currentMap, graph, nPrev, nCurrent))
                    {
                        // si no puedo explorar marco el mapa actual como pheromonas 0
                        mapNeigs[lastMap][currentMap] = 0;                  // ojo
                                                                            // retrocedo
                        i -= 2;                                               // cheacar si esto es correcto haciendo lo paso a poaso (!!!)
                        FailAmount_NoChilds--;
                        continue;
                        //return pathList;
                    }

                    // creo un nuevo diccionario
                    var (neig, value) = AntExplore(currentMap, graph, nCurrent, restriction);
                    //var (neig, value) = AntExplore(currentMap, graph, nPrev, nCurrent, restriction);

                    if (value > 0)
                    {
                        pathList.Add(neig);
                        currentMap = neig;
                    }
                    else
                    {
                        i--;
                        FailAmount_InvalidStep--;
                        //return pathList;// <- por aqui deberia ir un "Si falla explora denuevo"
                    }
                }

                lastMap = currentMap;


                if (i == 0)
                {
                    Debug.Log("Mapa explorado al completo, imposible de terminar");
                    return pathList;
                }
            }

            return pathList;
        }

        public bool CanExplore(Map current, Graph graph, Graph.Node nodePrev, Graph.Node nodeCurrent)
        {
            movmenets.TryGetValue((current, nodeCurrent), out var validPivots);
            if (validPivots == null)
            {
                // genero la lista de pivotes validos
                validPivots = GeneratePivotsNeigs(current, graph, nodePrev, nodeCurrent);
                movmenets.Add((current, nodeCurrent), validPivots);
            }

            return validPivots.Count != 0;
        }

        public int ValidsPathAmount(Dictionary<Map, float> neigs)
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

        public (Map, float) AntExplore(Map currentMap, Graph graph, Graph.Node nodeCurrent, IRestriction restriction)
        {
            movmenets.TryGetValue((currentMap, nodeCurrent), out var validPivots);
            /*
            if(validPivots == null)
            {
                // genero la lista de pivotes validos
                validPivots = GeneratePivotsNeigs(currentMap, graph, nodeCurrent, nodeCurrent);
                movmenets.Add((currentMap, nodeCurrent), validPivots);
            }
            */

            var (pivot, size) = validPivots.GetRandom(); // selecciona un pivote y tamaño para colocar la nueva habitacion
            validPivots.Remove((pivot, size));

            var nextMap = currentMap.Clone() as Map;
            nextMap.SetRoomTiles(pivot, pivot + size, nodeCurrent.id);

            // no son añadidos si no cumplen con la restriccion
            if (restriction.Execute(new Tuple<Map, Graph>(nextMap, graph)))                          // la restricciones podria estar afuera (!!!)
            {
                if (!mapNeigs[currentMap].ContainsKey(nextMap))
                {
                    mapNeigs[currentMap].Add(nextMap, 1); // este valor podria ser una heuristica tambien
                }
                return (nextMap, 1);

            }
            else
            {
                if (!mapNeigs[currentMap].ContainsKey(nextMap))
                {
                    mapNeigs[currentMap].Add(nextMap, 0);
                }
                return (nextMap, 0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="current"></param>
        /// <param name="graph"></param>
        /// <param name="nodePrev"></param>
        /// <param name="nodeCurrent"></param>
        /// <returns></returns>
        public List<(Vector2Int, Vector2Int)> GeneratePivotsNeigs(Map current, Graph graph, Graph.Node nodePrev, Graph.Node nodeCurrent)
        {
            // genero todas las combinaciones de tamaño de la nueva habitacion
            var allSizes = new List<Vector2Int>();
            for (int i = nodeCurrent.minArea.x; i <= nodeCurrent.maxArea.x; i++)
            {
                for (int j = nodeCurrent.minArea.y; j <= nodeCurrent.maxArea.y; j++)
                {
                    allSizes.Add(new Vector2Int(i, j));
                }
            }

            // para cada tamaño
            var allPivots = new List<(Vector2Int, Vector2Int)>();
            foreach (var size in allSizes)
            {
                // genero todos los pivotes validos en base a la direccion 
                var (pivots, emptyPos) = GetPivotsNeigs(nodeCurrent.pos - nodePrev.pos, size, nodePrev.id, current);
                foreach (var (p, s) in pivots)
                {
                    allPivots.Add((p, s));
                }
            }

            return allPivots;
        }

        /// <summary>
        /// regresa una lista de nodos ordenados desde el nodo con mas vecinos,
        /// para luego obtener los vecinos de este y regresarlos ordenados por
        /// la cantidad de vecinos, asi susesibamente.
        /// </summary>
        /// <param name="graph"></param>
        /// <returns> a tuple [from -> to]</returns>
        private List<(Graph.Node, Graph.Node)> GetNodeSortedByNeigs(Graph graph) // mover esto como una funcion SORT
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
                if (added.Contains(current))
                    continue;

                toR.Add((prev, current));
                added.Add(current);

                var neigs = graph.GetNeighbors(current);
                neigs = neigs.OrderByDescending((n) =>
                {
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

        /*
        private List<(Vector2Int,Vector2Int)> GetPivotAllowedCurrentNeigs(Vector2Int sizeArea,int roomID,Map map,Graph graph,Graph.Node current)
        {
            // obtengo vecinos del nodo actual
            var neigs = graph.GetNeighbors(current);

            // me quedo con los vecinos que ya estan en el mapa
            neigs = neigs.Where(n => map.rooms[n.id] != null).ToList();

            // inicializo una lista de pivotes permitidos (pivot,size)
            var allPivots = new List<List<(Vector2Int, Vector2Int)>>();
            // inicializo una lista de posiciones vacias de cada vecino
            var allAdjacents = new List<List<Vector2Int>>();

            foreach (var nei in neigs)
            {
                // obtengo la habitacion correspondiente al nodo
                var neigRoom = map.rooms[nei.id];

                // obtengo la direcciones a la que se encuentra la nueva habitacion
                var vec = nei.pos + current.pos;

                // obtengo los pivoten entre los nodos y las posiciones vacias adjacentes
                var (pivots, adjeacents) = GetPivotsNeigs(vec, sizeArea, roomID, map);
                allPivots.Add(pivots);
                allAdjacents.Add(adjeacents);
            }

            var toR = new List<(Vector2Int, Vector2Int)>();
            for (int i = 0; i < allPivots.Count; i++)
            {
                var pivots = allPivots[i];

                foreach (var (pivot,size) in pivots)
                {
                    var allowed = true;
                    for (int j = 0; j < allAdjacents.Count; j++)
                    {
                        if (i == j)
                            continue;

                        var adjs = allAdjacents[j];

                        if (!adjs.Any( ad => Contained(pivot,size, ad)))
                        {
                            allowed = false;
                            break;
                        }
                    }

                    if(allowed)
                        toR.Add((pivot, size));
                }
            }

            return toR;
        }
        */



        /// <summary>
        /// con la direccion entre la habitacion que quiero añadir y la habitacion que
        /// ya esta en el mapa, obtengo los posibles pivotes para añadir la nueva habitacion
        /// </summary>
        /// <param name="nodeVector"></param>
        /// <param name="sizeArea"></param>
        /// <param name="roomID"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        private (List<(Vector2Int, Vector2Int)>, List<Vector2Int>) GetPivotsNeigs(Vector2 nodeVector, Vector2Int sizeArea, int roomID, Map map)
        {
            var toR = new List<(Vector2Int, Vector2Int)>();

            // obtengo la direcciones a la que se encuentra la nueva habitacion
            var dirs = !enforceGraph ?
                Directions.AllDirs() :
                Directions.AngulatedDirs(nodeVector);

            // obtengo la habitacion anterior
            var room = map.rooms[roomID];

            var toR2 = new List<Vector2Int>();
            foreach (var dir in dirs)
            {
                if (dir == Directions.Dirs_4.None)
                    continue;

                // obtengo las posiciones vacias
                var emptyPos = GetAdjacent(dir, room);
                toR2.AddRange(emptyPos);

                // obtengo el desplazamiento principal
                var starDisp = (StartDisp(dir) * sizeArea - StartDisp(dir));                        // <---------- pareceira que el "- StartDisp(dir)" es correcto

                // obtengo el desplazamiento secundario
                var endDisp = (EndDisp(dir) * sizeArea - EndDisp(dir));                             // <---------- pareceira que el "- EndDisp(dir)" es correcto

                foreach (var pos in emptyPos)
                {
                    // obtengo la cantidad de desplazamientos secundarios
                    var dispacedPoints = Utils.GetPointsBetween(pos + starDisp, pos + endDisp);

                    foreach (var point in dispacedPoints)
                    {
                        // si no esta en la lista lo añado
                        if (!toR.Contains((point, sizeArea)))
                            toR.Add((point, sizeArea));
                    }
                }
            }

            return (toR, toR2);
        }

        /// <summary>
        /// Entrega todos los tieles adjacentes a una habitacion en (CORRECTA)
        /// una direccion en concreto
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="room"></param>
        /// <returns></returns>
        private List<Vector2Int> GetAdjacent(Directions.Dirs_4 dir, Dictionary<Vector2Int, Tile> room)
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

        /// <summary>
        /// Regresa el pivote final para poner la nueva habitacion. (CORRECTA)
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
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


        /// <summary>
        /// regresa el pivote de inicio para poner la nueva habitacion (CORRECTA)
        /// en base a la direccion que se esta poniendo.
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
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


        // NO SE SI ESTA FUNCION SE VA A OCUPAR AUN
        public bool Contained(Vector2Int pivot, Vector2Int size, Vector2Int pos) // esto podria ir en un "Utils"
        {
            var end = pivot + size;
            return pos.x >= pivot.x && pos.x <= end.x && pos.y >= pivot.y && pos.y <= end.y;
        }
    }
}