using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Optimization.Terminators;
using Optimization.Evaluators;
using Problem.Evaluators;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ACOExperiment : MonoBehaviour
{
    public string path = "Output";

    public void Execute()
    {
        // init Seed
        UnityEngine.Random.InitState(117);

        //Create Graph
        var graph = TestUtils.ExampleGraph();

        // Terminator
        var terminator = new TimeTerminator() { maxTime = 100f}; // TODO: Cheacar si esto

        // Evaluator
        var evaluator = new WeightedAgregateEvaluator()
        {
            evs = new (IEvaluator, float)[]
            {
                (new VoidEvaluator(), 0.3f),
                (new ExteriorWallEvaluator(), 0.3f),
                (new CornerEvaluator(), 0.4f)
            }
        };

        // ACO Constructive
        var aco = new ACO();
        var acoMap = aco.Execute(graph, 5, 0.9f, evaluator, terminator);

        // Generate images
        for (int i = 0; i < acoMap.Count; i++)
        {
            Utils.GenerateImage(acoMap[i], "aco_Map_"+i+".png", Application.dataPath +"/"+ path);
        }
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