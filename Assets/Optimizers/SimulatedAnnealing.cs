using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class SimulatedAnnealing
{
    private float currentTemp;
    private float coolingRate;

    private Func<List<Map>,List<Map>> Selector;
    private Func<Map,float> Evaluate; // normalizado
    private Func<bool> Terminator;
    
    public Map Ejecute(Map init, float temp,Func<Map,float> evaluator,Func<List<Map>,List<Map>> selector)
    {
        // Init
        Map best = init;
        currentTemp = temp;
        Evaluate = evaluator;
        Selector = selector;

        // Algorithm
        var fitness = Evaluate.Invoke(best);
        while (currentTemp > 1)
        {
            var neighbor = GetNeighbors(best);
            var selected = Selector.Invoke(neighbor);

            for (int i = 0; i < selected.Count; i++)
            {
                if (Terminator.Invoke())
                {
                    return best;
                }

                var neig = selected[i];
                var newFitness = Evaluate.Invoke(neig);

                var diff = newFitness - fitness;

                if (diff > 0)
                {
                    best = neig;
                    fitness = newFitness;
                }
                else
                {
                    var prob = Mathf.Exp(-diff / currentTemp);
                    if (prob > UnityEngine.Random.Range(0.0f, 1.0f))
                    {
                        best = neig;
                        fitness = newFitness;
                    }
                }

                currentTemp *= 1 - coolingRate;
            }
        }

        return best;
    }

    public List<Map> GetNeighbors(Map map)
    {
        return new List<Map>(); // TODO: Implement
    }

}
