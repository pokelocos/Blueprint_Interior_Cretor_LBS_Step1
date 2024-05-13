using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        var (mtx, w, h) = map.ToTileMatrix();
        var t = new Texture2D(map.Width, map.Height);
        for (int i = 0; i < map.Width; i++)
        {
            for (int j = 0; j < map.Height; j++)
            {
                var c = mtx[i, j].roomID;

                if (!colors.ContainsKey(c))
                {
                    colors.Add(c, new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f)));
                    t.SetPixel(i, j, colors[c]);
                }
                t.SetPixel(i, j, colors[c]);
            }
        }

        byte[] pngData = t.EncodeToPNG();
        string filePath = Path.Combine(path, fileName);
        File.WriteAllBytes(filePath, pngData);

        Debug.Log("Textura guardada como " + fileName + " en la ruta: " + filePath);
    }

    // FIX?: Esto se podria hacer de otra forma pa que sea menos ciclos.
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
}