using Optimization.Evaluators;
using Optimization.Terminators;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Optimization.Neigbors;
using Problem.Neigbors;
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

        // Simulated Annealing
        var simulatedAnnealing = new SimulatedAnnealing();
        var saMap = simulatedAnnealing.Execute(
            pointMap,
            1000f,
            evaluator,
            new ManualTerminator(),
            neigbor); // neighbor function

        // Show SA Map
        Utils.GenerateImage(saMap, "SA_Map.png", Application.dataPath);

        // Tabu Search
        //var tabuSearch = new TabuSearch();
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
        if (GUILayout.Button("Run"))
        {
            myScript.Execute();
            AssetDatabase.Refresh();
        }
    }
}

#endif
