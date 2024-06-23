using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif



public class Test : MonoBehaviour
{
    public void TestEqualList()
    {
        var graph = TestUtils.ExampleGraph();

        var l1 = new List<Graph.Node>() { graph.nodes[0], graph.nodes[1] };
        var l2 = new List<Graph.Node>() { graph.nodes[1], graph.nodes[0] };
        var l3 = new List<Graph.Node>() { graph.nodes[0], graph.nodes[1] };
        var l4 = new List<Graph.Node>() { graph.nodes[0], graph.nodes[1], graph.nodes[2] };

        var dict = new Dictionary<List<Graph.Node>, string>(new ListComparer<Graph.Node>());
        dict[l1] = "List 1";
        dict[l2] = "List 2";
        dict[l3] = "List 3";
        dict[l4] = "List 4";

        Debug.Log("Value for l1: " + dict[l1]);
        Debug.Log("Value for l3: " + dict[l3]);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Test))]
public class TestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Test myScript = (Test)target;

        if (GUILayout.Button("Test equal list"))
        {
            myScript.TestEqualList();
        }
    }
}
#endif