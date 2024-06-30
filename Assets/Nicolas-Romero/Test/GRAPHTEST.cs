using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class GRAPHTEST : MonoBehaviour
{
    public Graph GenerateGraph()
    {
        var graph = new Graph(this.gameObject.name);

        var indexs = new Dictionary<NODETEST, int>();

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i).GetComponent<NODETEST>();
            var node = new Graph.Node()
            {
                name = child.name,
                id = (i + 1),
                pos = child.transform.position,
                minArea = child.minArea,
                maxArea = child.maxArea,
                color = child.color
            };
            indexs.Add(child, i);
            graph.nodes.Add(node);
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i).GetComponent<NODETEST>();
            for (int j = 0; j < child.other.Count; j++)
            {
                var other = child.other[j].GetComponent<NODETEST>();
                var edge = new Graph.Edge()
                {
                    n1 = graph.nodes[i],
                    n2 = graph.nodes[indexs[other]]
                };
                graph.edges.Add(edge);
            }
        }
        return graph;
    }
}
