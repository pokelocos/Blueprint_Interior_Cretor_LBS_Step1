using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumbersSet
{
    public Dictionary<int, List<string>> dict = new Dictionary<int, List<string>>();
    public Dictionary<string, List<int>> dictKeys;

    private (string, string)[] words = new (string, string)[]
    {
        ("101xxxxx","Concave Corner RU"),
        ("xx101xxx","Concave Corner RD"),
        ("1xxxxx10","Concave Corner LD"),
        ("xxxx101x","Concave Corner LU"),
        ("000xxxxx","Convex Corner RU"),
        ("xx000xxx","Convex Corner RD"),
        ("0xxxxx00","Convex Corner LD"),
        ("xxxx000x","Convex Corner LU"),
        ("0xxxxxxx", "Wall Rigth"),
        ("xx0xxxxx", "Wall Up"),
        ("xxxx0xxx", "Wall Left"),
        ("xxxxxx0x", "Wall Down"),
    };

    public NumbersSet()
    {
        var all = GetAll();

        for (int i = 0; i < all.Count; i++)
        {
            var ws = CheckMasks(all[i], words);
            AddWord(i, ws);
        }

        dictKeys = InvertDictionary(dict);

        foreach (var item in dictKeys)
        {
            var msg = "Word: " + item.Key + "\n";
            foreach (var v in item.Value)
            {
                msg += v + ", ";
            }
            Debug.Log(msg);
        }
    }

    private static Dictionary<string, List<int>> InvertDictionary(Dictionary<int, List<string>> original)
    {
        var toR = new Dictionary<string, List<int>>();

        foreach (var kvp in original)
        {
            int key = kvp.Key;
            List<string> values = kvp.Value;

            foreach (string value in values)
            {
                if (!toR.ContainsKey(value))
                {
                    toR[value] = new List<int>();
                }
                toR[value].Add(key);
            }
        }

        return toR;
    }

    private List<string> CheckMasks(BitArray value, (string, string)[] mask)
    {
        var toR = new List<string>();
        for (int j = 0; j < mask.Length; j++)
        {
            var x = true;
            for (int k = 0; k < 8; k++)
            {
                var v = mask[j].Item1[k];

                if (v == 'x')
                    continue;

                var A = value.Get(k);
                var B = ('1' == v);

                if (value[k] != ('1' == v))
                {
                    x = false;
                    break;
                }
            }

            if(x) toR.Add(mask[j].Item2);
        }
        return toR;
    }

    private void AddWord(int i, List<string> ws)
    {
        if(dict.ContainsKey(i))
        {
            dict[i].AddRange(ws);
        }
        else
        {
            dict.Add(i, ws);
        }
    }

    private List<BitArray> GetAll()
    {
        var all = new List<BitArray>();

        for (int i = 0; i < 256; i++)
        {
            all.Add(Utils.IntToBitArray(i));
        }
        return all;
    }
}

public static class Vector2Extension
{
    public static float MinAbs(this Vector2 v)
    {
        return (Mathf.Abs(v.x) < Mathf.Abs(v.y))? v.x : v.y;
    }


}