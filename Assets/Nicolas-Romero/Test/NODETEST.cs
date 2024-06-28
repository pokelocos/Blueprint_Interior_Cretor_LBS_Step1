using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NODETEST : MonoBehaviour
{
    public enum NodeType
    {
        Room,
        Hall,
        hallway_H,
        hallway_W,
        Service,
    }

    public Vector2Int minArea;
    public Vector2Int maxArea;

    [HideInInspector]
    public NodeType type;
    public Color color;

    public List<GameObject> other = new();

    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position,1);
        foreach (var item in other)
        {
            if (item == null)
                continue;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, item.transform.position);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NODETEST))]
[CanEditMultipleObjects]
public class NODETESTEditor : Editor
{
    private NODETEST.NodeType[] types = new NODETEST.NodeType[]
    {
        NODETEST.NodeType.Room,
        NODETEST.NodeType.Hall,
        NODETEST.NodeType.hallway_H,
        NODETEST.NodeType.hallway_W,
        NODETEST.NodeType.Service,
    };

    private (Vector2Int,Vector2Int) GetSize(NODETEST.NodeType type)
    {
        switch(type)
        {
            case NODETEST.NodeType.Room:
                return (new Vector2Int(4, 4), new Vector2Int(6, 6));
            case NODETEST.NodeType.Hall:
                return (new Vector2Int(6, 6), new Vector2Int(10, 10));
            case NODETEST.NodeType.hallway_H:
                return (new Vector2Int(2, 6), new Vector2Int(3, 10));
            case NODETEST.NodeType.hallway_W:
                return (new Vector2Int(6, 2), new Vector2Int(10, 3));
            case NODETEST.NodeType.Service:
                return (new Vector2Int(2, 2), new Vector2Int(3, 3));
            default:
                return (new Vector2Int(4, 4), new Vector2Int(6, 6));
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        foreach (var item in targets)
        {
            var node = (item as NODETEST);
            var prev = node.type;
            node.type = (NODETEST.NodeType)EditorGUILayout.EnumPopup("Type", node.type);
            if(node.type != prev)
            {
                (node.minArea, node.maxArea) = GetSize(node.type);
            }
        }

        if (GUILayout.Button("Rand Color"))
        {
            foreach (var item in targets)
            {
                NODETEST node = (NODETEST)item;
                node.color = new Color(
                    Random.Range(0f, 1f),
                    Random.Range(0f, 1f),
                    Random.Range(0f, 1f));
            }
        }

        if(GUILayout.Button("Rand Room Type"))
        {
            foreach (var item in targets)
            {
                NODETEST node = (NODETEST)item;
                node.type = types[Random.Range(0, types.Length)];
                (node.minArea, node.maxArea) = GetSize(node.type);
            }
        }
    }
}
#endif
