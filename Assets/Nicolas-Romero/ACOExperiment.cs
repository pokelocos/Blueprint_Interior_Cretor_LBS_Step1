using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Optimization.Terminators;
using Optimization.Evaluators;
using Problem.Evaluators;
using Optimization.Restrictions;
using Problem.Restrictions;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ACOExperiment : MonoBehaviour
{
    public GameObject graphTest;

    public string path = "Output";

    [Range(1,100)]
    public int iterations = 10;
    [Range(1, 100)]
    public int antsPerIteration = 5;
    [Range(0.01f, 10)]
    public float pheromoneIntensity = 1f;
    [Range(0.01f,099f)]
    public float evaporationRate = 0.9f;

    public string seed = "";

    public float[] evaluatorWeight = new float[] { 0.3f, 0.3f, 0.4f };

    public bool enforceGraphStructure = true;

    public void Execute()
    {
        // init Seed
        if (seed != "")
        {
            UnityEngine.Random.InitState(seed.GetHashCode());
        }
        else
        {
            var randomSeed = System.DateTime.Now.Ticks.GetHashCode();
            UnityEngine.Random.InitState(randomSeed);
        }

        //Create Graph
        // var graph = TestUtils.ExampleGraph();
        //var graph = TestUtils.ExampleGraph_Trianlge();
        var graph = graphTest.GetComponent<GRAPHTEST>().GenerateGraph();

        // Terminator
        var terminator = new AgregateTermination()
        {
            terminators = new ITerminator[]
            {
                new IterationTerminator() { maxIterations = iterations}, // numero de generaciones
                new ManualTerminator() // escape key
            },
        };

        // Evaluator
        var evaluator = new WeightedAgregateEvaluator()
        {
            evs = new (IEvaluator, float)[]
            {
                (new VoidEvaluator(), evaluatorWeight[0]),
                (new ExteriorWallEvaluator(), evaluatorWeight[1]),
                (new CornerEvaluator(), evaluatorWeight[2])
            }
        };

        // Restricion
        var restrcition = new AgregateRestriction()
        {
            res = new IRestriction[]
            {
                new ConectivityGraphRestriction(),
                new SpritingRoomRestriction(),
                new AmountRoomRestriction(),
                //new MinMaxAreaRestriction()
            }
        };

        // ACO Constructive
        var aco = new ACO();
        var (acoMap, data) = aco.Execute(graph, antsPerIteration, pheromoneIntensity, evaporationRate, evaluator, terminator, restrcition);

        // TODO: hacer algo cuando retorne acoMap.Count == 0

        // Generate images
        for (int i = 0; i < acoMap.Count; i++)
        {
            //Utils.GenerateImage(acoMap[i],graph, "aco_Map_"+i+".png", Application.dataPath +"/"+ path);
            Utils.GenerateSizedImage(acoMap[i], graph, 40, "aco_Map_" + i + ".png", Application.dataPath + "/" + path);
        }

        Utils.GenerateCSV<Data>(data, "data.csv", Application.dataPath + "/" + path);

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ACOExperiment))]
public class ACOExperimentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ACOExperiment myScript = (ACOExperiment)target;

        if (GUILayout.Button("Run ACO"))
        {
            FileUtil.DeleteFileOrDirectory(Application.dataPath + "/" + myScript.path);
            
            myScript.Execute();
            AssetDatabase.Refresh();
        }
    }
}

#endif