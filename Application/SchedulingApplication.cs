using JobShopSchedulingFramework.Data;
using JobShopSchedulingFramework.DataGeneration;
using JobShopSchedulingFramework.Evaluation;
using JobShopSchedulingFramework.ExactSolvers;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods;
using JobShopSchedulingFramework.Models;
using JobShopSchedulingFramework.Visualisation;
using System.Diagnostics;

namespace JobShopSchedulingFramework.Application
{
    public static class SchedulingApplication
    {
        public static void Run(string[] args)
        {
            PrintHeader();

            GenerateControlledInstances();

            Console.WriteLine("Instance selection starts now.");
            Console.WriteLine();

            string fileName =
                InstanceFileSelector.SelectFromFolder(
                    @"Instances\Generated");

            Console.WriteLine();
            Console.WriteLine("You selected:");
            Console.WriteLine(fileName);
            Console.WriteLine();

            RunHeuristicExperiment(fileName);

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void PrintHeader()
        {
            Console.WriteLine("=======================================");
            Console.WriteLine(" JOB SHOP SCHEDULING PROJECT");
            Console.WriteLine(" Giffler-Thompson + Tabu Search + CP");
            Console.WriteLine("=======================================");
            Console.WriteLine();
        }

        private static void GenerateControlledInstances()
        {
            int numberOfJobs = 10;
            int numberOfMachines = 5;
            InstanceType type = InstanceType.Normal;

            int baseSeed = 42;
            int numberOfInstances = 5;

            string outputFolder =
                @"Instances\Generated";

            InstanceWriter writer =
                new InstanceWriter();

            for (int i = 1; i <= numberOfInstances; i++)
            {
                int seed = baseSeed + i;

                InstanceGeneratorAdvanced generator =
                    new InstanceGeneratorAdvanced(seed, type);

                Instance instance =
                    generator.Generate(numberOfJobs, numberOfMachines);

                string generatedFileName =
                    $@"{outputFolder}\{type}_{numberOfJobs}x{numberOfMachines}_seed{seed}.txt";

                writer.WriteToFile(instance, generatedFileName);

                Console.WriteLine("Generated: " + generatedFileName);
            }

            Console.WriteLine();
        }

        private static void RunHeuristicExperiment(string fileName)
        {
            Console.WriteLine("Selected instance:");
            Console.WriteLine(fileName);
            Console.WriteLine();

            InitialHeuristicResult result =
                HeuristicExperiment.Run(fileName);

            int bestTabuCmax =
                RunTabuNeighborhoodExperiment(result);

            int cpCmax =
                CpSolverRunner.Run(
                    CloneInstance(result.bestInstance),
                    timeLimitSeconds: 90);

            CpSolverRunner.PrintComparison(
                result.bestCmax,
                bestTabuCmax,
                cpCmax);

            string outputPath =
                @"Visualisation\Output\gantt_chart.html";

            GantChart.CreateHtml(
                result.bestInstance,
                outputPath,
                result.bestRule.ToString(),
                result.bestCmax,
                "FEASIBLE");

            Console.WriteLine();
            Console.WriteLine("Gantt chart created:");
            Console.WriteLine(outputPath);

            string fullPath =
                Path.GetFullPath(outputPath);

            Process.Start(new ProcessStartInfo
            {
                FileName = fullPath,
                UseShellExecute = true
            });
        }

        private static int RunTabuNeighborhoodExperiment(
            InitialHeuristicResult result)
        {
            Console.WriteLine();
            Console.WriteLine("=======================================");
            Console.WriteLine(" TABU NEIGHBORHOOD EXPERIMENT");
            Console.WriteLine("=======================================");

            int initialCmax =
                result.bestCmax;

            Console.WriteLine("Initial best rule: " + result.bestRule);
            Console.WriteLine("Initial Cmax: " + initialCmax);
            Console.WriteLine();

            int maxIterations =
                100;

            List<(string Name, INeighborhoodDefinition Neighborhood)> neighborhoods =
            new List<(string Name, INeighborhoodDefinition Neighborhood)>
            {
                ("N1 - Adjacent Critical Block Swaps", new AdjacentSwapNeighborhood()),
                ("N2 - Restricted First/Last Block Swaps", new RestrictedBlockSwapNeighborhood()),
                ("N3 - All Pair Critical Block Swaps", new AllPairSwapNeighborhood()),
                ("N4 - Critical Block Insert Moves", new CriticalBlockInsertNeighborhood())
            };

            Console.WriteLine(
                "Neighborhood".PadRight(40) + " | " +
                "Cmax".PadRight(8) + " | " +
                "Improve".PadRight(10) + " | " +
                "Improve %".PadRight(12) + " | " +
                "Runtime ms");

            Console.WriteLine(new string('-', 95));

            string bestNeighborhoodName =
                "";

            int bestTabuCmax =
                int.MaxValue;

            long bestRuntime =
                0;

            foreach (var item in neighborhoods)
            {
                Instance instanceCopy =
                    CloneInstance(result.bestInstance);

                Stopwatch stopwatch =
                    Stopwatch.StartNew();

                TabuSearchSolver tabuSearch =
                new TabuSearchSolver(
                maxIterations: maxIterations,
                neighborhoodDefinition: item.Neighborhood);

                int tabuCmax =
                    tabuSearch.Run(instanceCopy);

                stopwatch.Stop();

                int improvement =
                    initialCmax - tabuCmax;

                double improvementPercent =
                    initialCmax > 0
                        ? (double)improvement / initialCmax * 100.0
                        : 0.0;

                Console.WriteLine(
                    item.Name.PadRight(40) + " | " +
                    tabuCmax.ToString().PadRight(8) + " | " +
                    improvement.ToString().PadRight(10) + " | " +
                    (improvementPercent.ToString("F2") + "%").PadRight(12) + " | " +
                    stopwatch.ElapsedMilliseconds);

                if (tabuCmax < bestTabuCmax)
                {
                    bestTabuCmax =
                        tabuCmax;

                    bestNeighborhoodName =
                        item.Name;

                    bestRuntime =
                        stopwatch.ElapsedMilliseconds;
                }
            }

            Console.WriteLine(new string('-', 95));
            Console.WriteLine("Best neighborhood: " + bestNeighborhoodName);
            Console.WriteLine("Best Tabu Cmax: " + bestTabuCmax);
            Console.WriteLine("Best runtime ms: " + bestRuntime);

            return bestTabuCmax;
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