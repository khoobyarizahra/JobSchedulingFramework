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
            Console.WriteLine(new string('=', 90));

            Console.WriteLine(
                "Neighborhood".PadRight(35) + " | " +
                "Best Cmax".PadRight(12) + " | " +
                "Runtime ms".PadRight(12) + " | " +
                "Iterations");

            Console.WriteLine(new string('-', 90));

            // 0 means: no fixed time limit.
            // This experiment compares the neighborhood definitions with the same
            // algorithmic iteration limit. The final Tabu Search for the benchmark
            // evaluation still uses only N3 / AllPairSwapNeighborhood.
            int timeLimitSeconds =
                0;

            foreach (var item in neighborhoods)
            {
                Instance instanceCopy =
                    CloneInstance(instance);

                Stopwatch stopwatch =
                    Stopwatch.StartNew();

                TabuSearchSolver solver =
                    new TabuSearchSolver(
                        maxIterations,
                        timeLimitSeconds,
                        item.Neighborhood);

                int bestCmax =
                    solver.Run(instanceCopy);

                stopwatch.Stop();

                Console.WriteLine(
                    item.Name.PadRight(35) + " | " +
                    bestCmax.ToString().PadRight(12) + " | " +
                    stopwatch.ElapsedMilliseconds.ToString().PadRight(12) + " | " +
                    maxIterations);
            }

            Console.WriteLine(new string('=', 90));
        }

        private static Instance CloneInstance(
            Instance original)
        {
            Instance clone =
                new Instance();

            clone.NumJobs =
                original.NumJobs;

            clone.NumMachines =
                original.NumMachines;

            foreach (Job originalJob in original.Jobs)
            {
                Job clonedJob =
                    new Job(originalJob.JobID);

                foreach (Operation originalOperation in originalJob.Operations)
                {
                    Operation clonedOperation =
                        new Operation(
                            originalOperation.JobID,
                            originalOperation.OperationID,
                            originalOperation.Machine,
                            originalOperation.ProcessingTime);

                    clonedOperation.StartTime =
                        originalOperation.StartTime;

                    clonedOperation.EndTime =
                        originalOperation.EndTime;

                    clonedOperation.remainingProcessingTime =
                        originalOperation.remainingProcessingTime;

                    clonedJob.Operations.Add(
                        clonedOperation);
                }

                clone.Jobs.Add(
                    clonedJob);
            }

            if (original.SetupTimes != null)
            {
                int rows =
                    original.SetupTimes.GetLength(0);

                int columns =
                    original.SetupTimes.GetLength(1);

                clone.SetupTimes =
                    new int[rows, columns];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        clone.SetupTimes[i, j] =
                            original.SetupTimes[i, j];
                    }
                }
            }

            return clone;
        }
    }
}