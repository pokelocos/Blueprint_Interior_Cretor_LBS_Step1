using Optimization.Evaluators;
using Optimization.Terminators;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Optimization.Neigbors;
using Problem.Neigbors;
using Optimization;
using Problem.Evaluators;
using Optimization.Selector;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Experiment : MonoBehaviour
{

    void Start()
    {
        //Execute();
    }

    public void Execute()
    {
        // init Seed
        UnityEngine.Random.InitState(117);

        //Create Graph
        var graph = TestUtils.ExampleGraph();

        // Point Constructive
        var pointMap = TestUtils.PointConstructive(graph);
        Utils.GenerateImage(pointMap, "Constructive_Point_Map.png", Application.dataPath);

        // Evalutor
        var evaluator = new WeightedAgregateEvaluator()
        {
            evs = new (IEvaluator, float)[]
            {
                (new VoidEvaluator(), 0.3f),
                (new ExteriorWallEvaluator(), 0.3f),
                (new CornerEvaluator(), 0.4f)
            }
        };

        // Get Neighbors
        var neigbor = new AgregateNeigbors()
        {
            neigbors = new IGetNeighbors[]
            {
                new GetNeighborByMoveRooms(),
                new GetNeighborByMoveWalls()
            }
        };

        // Terminator
        var terminator = new ManualTerminator();

        // Selector
        var selector = new FirstBestSelector();

        ///*
        var hillClimbing = new HillClimbing();
        var hcMap = hillClimbing.Execute(
            pointMap,
            evaluator,
            terminator,
            neigbor,
            selector);

        // Show SA Map
        Utils.GenerateImage(hcMap, "HC_Map.png", Application.dataPath);

        //*/
        /*
        // Simulated Annealing
        var simulatedAnnealing = new SimulatedAnnealing();
        var saMap = simulatedAnnealing.Execute(
            pointMap,
            1000f,
            evaluator,
            terminator,
            neigbor);

        // Show SA Map
        Utils.GenerateImage(saMap, "SA_Map.png", Application.dataPath);
        */

        // Tabu Search
        //var tabuSearch = new TabuSearch();
    }

    public void Test() // TODO: hacer esto un "Unit-Test" para despues empaquetar
    {
        /*
        var m = new int[,] {
            { 0, 1, 1, 0, 1,},
            { 1, 1, 1, 0, 0,},
            { 1, 1, 1, 1, 1,},
            { 1, 0, 1, 0, 1,},
            { 1, 1, 1, 1, 0,}
        };
        */

        var m = new int[,]
        {
            { 1, 2 },
            { 4, 3 },
        };

        var map = Map.MatrixToMap(m,2,2);
        Utils.GenerateImage(map, "TestWall.png", Application.dataPath);
        var ws = map.GetWalls(1);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Experiment))]
public class ExperimentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Experiment myScript = (Experiment)target;
        if (GUILayout.Button("Run SA"))
        {
            myScript.Execute();
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Test"))
        {
            myScript.Test();
            AssetDatabase.Refresh();
        }
    }
}

#endif
