using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;
using System.Diagnostics;
/*
namespace ISILab.AI.Optimization
{
    public class HillClimbing
    {
        Func<IOptimizable, List<IOptimizable>> GetNeighbors;
        public double Nlog = 0;
        public double NNlog = 0;
        public double Elog = 0;

        public float _nbrsTimer = 0;
        public float _fitTimer = 0;

        public HillClimbing(IPopulation population, IEvaluator evaluator, ISelection selection, Func<IOptimizable, List<IOptimizable>> getNeighbors,  ITermination termination) : base( population, evaluator, selection, termination)
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
            var clock = new Stopwatch();

            var last = Population.Generations.Last();
            
            clock.Restart();
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