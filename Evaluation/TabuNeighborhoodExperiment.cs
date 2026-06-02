using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods;
using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace JobShopSchedulingFramework.Evaluation
{
    public static class TabuNeighborhoodExperiment
    {
        public static void Run(
            Instance instance,
            int maxIterations,
            int tabuTenure)
        {
            List<(string Name, INeighborhoodDefinition Neighborhood)> neighborhoods =
                new List<(string Name, INeighborhoodDefinition Neighborhood)>
                {
                    ("N1 - Adjacent Swaps", new AdjacentSwapNeighborhood()),
                    ("N2 - Restricted Block Swaps", new RestrictedBlockSwapNeighborhood()),
                    ("N3 - All Pair Swaps", new AllPairSwapNeighborhood())
                };

            Console.WriteLine();
            Console.WriteLine("TABU NEIGHBORHOOD EXPERIMENT");
            Console.WriteLine(new string('=', 80));

            Console.WriteLine(
                "Neighborhood".PadRight(35) + " | " +
                "Best Cmax".PadRight(12) + " | " +
                "Runtime ms");

            Console.WriteLine(new string('-', 80));

            foreach (var item in neighborhoods)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                TabuSearchSolver solver =
                 new TabuSearchSolver(
                maxIterations,
                item.Neighborhood);

                int bestCmax =
                    solver.Run(instance);

                stopwatch.Stop();

                Console.WriteLine(
                    item.Name.PadRight(35) + " | " +
                    bestCmax.ToString().PadRight(12) + " | " +
                    stopwatch.ElapsedMilliseconds);
            }

            Console.WriteLine(new string('=', 80));
        }
    }
}