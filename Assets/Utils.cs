using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Utils
{
    public static int BitArrayToInt(BitArray bits)
    {
        if (bits.Length != 8)
            throw new ArgumentException("El BitArray debe tener exactamente 8 bits.");

        byte[] bytes = new byte[1];
        bits.CopyTo(bytes, 0);
        return bytes[0];
    }

    public static BitArray IntToBitArray(int number)
    {
        return new BitArray(new[] { number });
    }

    public static void GenerateImage(Map map, string fileName, string path)
    {
        var colors = new Dictionary<int, Color>();
        colors.Add(0, Color.white);

        var ((cords, rooms, tiles), w, h) = map.ToTileMatrix();
        var t = new Texture2D(w, h);
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                var c = rooms[j, i];

                if (!colors.ContainsKey(c))
                {
                    colors.Add(c, new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f)));
                    t.SetPixel(j, i, colors[c]);
                }
                t.SetPixel(j, i, colors[c]);
            }
        }

        byte[] pngData = t.EncodeToPNG();

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Console.WriteLine("Folder created successfully.");
        }

        string filePath = Path.Combine(path, fileName);
        File.WriteAllBytes(filePath, pngData);

        Debug.Log("Textura guardada como " + fileName + " en la ruta: " + filePath);
    }

    // FIX?: Esto se podria hacer de otra forma pa que sea menos ciclos.
    [Obsolete]
    public static List<Vector2Int> GetNeigborPositions(List<Vector2Int> pos, List<Vector2Int> dirs)
    {
        var toR = new List<Vector2Int>();
        toR.AddRange(pos);

        for (int i = 0; i < pos.Count; i++)
        {
            for (int j = 0; j < dirs.Count; j++)
            {
                var n = pos[i] + dirs[j];
                if (!toR.Contains(n))
                    toR.Add(n);
            }
        }
        return toR;
    }

    public static T RandomRullete<T>(this List<T> list, Func<T, float> predicate)
    {
        if (list.Count <= 0)
        {
            return default(T);
        }

        var pairs = new List<Tuple<T, float>>();
        for (int i = 0; i < list.Count(); i++)
        {
            var value = predicate(list[i]);
            pairs.Add(new Tuple<T, float>(list[i], value));
        }

        var total = pairs.Sum(p => p.Item2);
        var rand = Random.Range(0.0f, total);

        var cur = 0f;
        for (int i = 0; i < pairs.Count; i++)
        {
            cur += pairs[i].Item2;
            if (rand <= cur)
            {
                return pairs[i].Item1;
            }
        }
        return default(T);
    }

    public static T GetRandom<T>(this List<T> list)
    {
        if (list.Count <= 0)
        {
            return default(T);
        }

        return list[Random.Range(0, list.Count)];
    }   

}