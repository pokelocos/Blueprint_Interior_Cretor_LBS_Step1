using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Data // TODO: implementar despues de probar que el esxperimento funciona
{
    public struct generacion
    {
        public float totalTime;
        public float explorationTime;
        public float evaluatorTime;

        public float genBest;
        public float average;

        // podria sacar la evaluacion de cada uno de los optimizadores

        public int niegExplored;
        public int successfulPathCount;
        public int deadEndCount;
    }

    public (List<Map>, float) best;
    public int totalNeigsExplored;
    public List<generacion> generations = new();

    // tiempo total de ejecucion
    public float totalTime;
}

public static class Timer
{
    private static Dictionary<string,Stopwatch> watches = new();

    public static void Start(string name)
    {
        watches.TryGetValue(name, out var watch);
        if(watch == null)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            watches.Add(name, sw);
        }
        else
        {
            Debug.LogWarning("Ya exite un timer con el nombre: " + name);
        }
    }

    public static float End(string name)
    {
        watches.TryGetValue(name, out var watch);
        if(watch == null)
        {
            Debug.LogWarning("No existe un timer con el nombre: " + name);
            return -1;
        }

        watch.Stop();
        watches.Remove(name);

        return watch.ElapsedMilliseconds;
    }
}

public static class Counter
{
    private static Dictionary<string, int> counters = new();

    public static void Add(string name)
    {
        counters.TryGetValue(name, out var count);
        if(count <= 0)
        {
            counters.Add(name, 1);
        }
        else
        {
            counters[name] = count + 1;
        }
    }

    public static int Get(string name)
    {
        counters.TryGetValue(name, out var count);
        if(count <= 0)
        {
            Debug.LogWarning("No existe un contador con el nombre: " + name);
            return -1;
        }

        counters.Remove(name);

        return count;
    }
}