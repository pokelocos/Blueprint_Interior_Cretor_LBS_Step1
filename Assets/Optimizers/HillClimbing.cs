using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;
using System.Diagnostics;
using Optimization.Evaluators;
using Optimization.Terminators;
using Optimization.Neigbors;

namespace Optimization
{
    public class HillClimbing
    {
        public Map Execute(Map init, IEvaluator evaluator, ITerminator terminator, IGetNeighbors getNeighbors)
        {
            Map best = init;
            var fitness = evaluator.Execute(best);

            var stuck = false;
            while (!stuck) 
            { 
                var selected = getNeighbors.Execute(best)
                    .Select(n => (n.Item1 as Map, n.Item2))
                    .ToList(); // OPTIMIZE:.toList()!

                stuck = true;
                for (int i = 0; i < selected.Count; i++)
                {
                    var (neig, move) = selected[i];
                    var nFitness = evaluator.Execute(neig);

                    if (nFitness > fitness)
                    {
                        best = neig;
                        fitness = nFitness;
                        stuck = false;
                    }
                }
            }

            return best;
        }
    }
}

/*
namespace ISILab.AI.Optimization
{
    public class HillClimbing
    {
        Func<IOptimizable, List<IOptimizable>> GetNeighbors;

        public HillClimbing(IPopulation population,
            IEvaluator evaluator, ISelection selection,
            Func<IOptimizable, List<IOptimizable>> getNeighbors,
            ITermination termination) : base( population, evaluator, selection, termination)
        {
            GetNeighbors = getNeighbors;
        }

        public override void EvaluateFitness(IList<IOptimizable> optimizables)
        {
            foreach(var o in optimizables)
            {
                o.Fitness = Evaluator.Evaluate(o);
            }
        }

        public override void RunOnce()
        {
            var last = Population.Generations.Last();
            
            var selection = this.Selection.SelectEvaluables(1, last);

            if(selection.Count == 0)
            {
                Stop();
            }

            var best = selection.First();


            if (GetNeighbors == null)
                throw new NullReferenceException();

            clock.Restart();
            var offsprings = GetNeighbors.Invoke(best);
            clock.Stop();
            _nbrsTimer = clock.ElapsedMilliseconds / 1000f;

            Nlog = clock.ElapsedMilliseconds;
            NNlog= offsprings.Count;
            //var offsprings = GetNeighbors?.Invoke(BestCandidate); // poner exepcion por si neigthbor es null (!!!)

            clock.Restart();
            EvaluateFitness(offsprings);
            clock.Stop();
            _fitTimer = clock.ElapsedMilliseconds / 1000f;

            Elog = clock.ElapsedMilliseconds;

            Population.CreateNewGeneration(offsprings);
            Population.EndCurrentGeneration();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override void PrintClocks()
        {
            UnityEngine.Debug.Log("Neighbors: " + _nbrsTimer + "s.");
            UnityEngine.Debug.Log("Fitness: " + _fitTimer + "s.");

        }
    }
}
*/