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

            string instanceFolder =
                SelectInstanceSource();

            Console.WriteLine("Instance selection starts now.");
            Console.WriteLine();

            string fileName =
                InstanceFileSelector.SelectFromFolder(instanceFolder);

            Console.WriteLine();
            Console.WriteLine("You selected:");
            Console.WriteLine(fileName);
            Console.WriteLine();

            RunHeuristicExperiment(fileName);

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static string SelectInstanceSource()
        {
            Console.WriteLine("Select instance source:");
            Console.WriteLine("1 - Generate new instances");
            Console.WriteLine("2 - Use benchmark instances");
            Console.Write("Choice: ");

            while (true)
            {
                string choice =
                    Console.ReadLine();

                if (choice == "1")
                {
                    GenerateControlledInstances();

                    return Path.GetFullPath(
                        Path.Combine(AppContext.BaseDirectory, @"..\..\..\Instances\Generated"));
                }

                if (choice == "2")
                {
                    return Path.GetFullPath(
                        Path.Combine(AppContext.BaseDirectory, @"..\..\..\Instances\Benchmark\ClassroomInstancesSet4_2"));
                }

                Console.Write("Invalid input. Please enter 1 or 2: ");
            }
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
               Path.GetFullPath(
               Path.Combine(AppContext.BaseDirectory, @"..\..\..\Instances\Generated"));

            Directory.CreateDirectory(outputFolder);

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

            Instance initialInstanceForChart =
                CloneInstance(result.bestInstance);

            TabuNeighborhoodExperimentResult tabuResult =
                RunTabuNeighborhoodExperiment(result);

            string outputFolder =
                @"Visualisation\Output";

            Directory.CreateDirectory(outputFolder);

            string initialOutputPath =
                Path.Combine(outputFolder, "initial_heuristic.html");

            string tabuOutputPath =
                Path.Combine(outputFolder, "tabu_search.html");

            string cpOutputPath =
                Path.Combine(outputFolder, "cp_solver.html");

            string comparisonOutputPath =
                Path.Combine(outputFolder, "comparison.html");

            Console.WriteLine();
            Console.WriteLine("CP solver starts now...");

            CpSolverResult cpResult =
                CpSolverRunner.Run(
                    CloneInstance(result.bestInstance),
                    timeLimitSeconds: 90);

            int cpCmax =
                cpResult.Cmax;

            Console.WriteLine("CP solver finished. CP Cmax: " + cpCmax);

            CpSolverRunner.PrintComparison(
                result.bestCmax,
                tabuResult.BestCmax,
                cpCmax);

            GantChart.CreateHtml(
                initialInstanceForChart,
                initialOutputPath,
                "Initial Heuristic - " + result.bestRule,
                result.bestCmax,
                "FEASIBLE");

            GantChart.CreateHtml(
                tabuResult.BestInstance,
                tabuOutputPath,
                "Tabu Search - " + tabuResult.BestNeighborhoodName,
                tabuResult.BestCmax,
                "FEASIBLE");

            if (cpResult.HasFeasibleSolution)
            {
                GantChart.CreateHtml(
                    cpResult.BestInstance,
                    cpOutputPath,
                    "CP Solver",
                    cpResult.Cmax,
                    "OPTIMAL / BEST FOUND");
            }

            GantChart.CreateComparisonHtml(
                comparisonOutputPath,
                Path.GetFileName(initialOutputPath),
                Path.GetFileName(tabuOutputPath),
                Path.GetFileName(cpOutputPath),
                result.bestRule.ToString(),
                tabuResult.BestNeighborhoodName,
                result.bestCmax,
                tabuResult.BestCmax,
                cpCmax);

            Console.WriteLine();
            Console.WriteLine("Comparison Gantt chart created:");
            Console.WriteLine(comparisonOutputPath);

            Process.Start(new ProcessStartInfo
            {
                FileName = Path.GetFullPath(comparisonOutputPath),
                UseShellExecute = true
            });
        }

        private static TabuNeighborhoodExperimentResult RunTabuNeighborhoodExperiment(
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
                5000;

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

            Instance bestTabuInstance =
                null;

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

                    bestTabuInstance =
                        CloneInstance(instanceCopy);
                }
            }

            Console.WriteLine(new string('-', 95));
            Console.WriteLine("Best neighborhood: " + bestNeighborhoodName);
            Console.WriteLine("Best Tabu Cmax: " + bestTabuCmax);
            Console.WriteLine("Best runtime ms: " + bestRuntime);

            return new TabuNeighborhoodExperimentResult
            {
                BestInstance = bestTabuInstance,
                BestCmax = bestTabuCmax,
                BestNeighborhoodName = bestNeighborhoodName,
                RuntimeMs = bestRuntime
            };
        }

        private static Instance CloneInstance(Instance original)
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

                    clonedJob.Operations.Add(clonedOperation);
                }

                clone.Jobs.Add(clonedJob);
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

        private class TabuNeighborhoodExperimentResult
        {
            public Instance BestInstance { get; set; }
            public int BestCmax { get; set; }
            public string BestNeighborhoodName { get; set; }
            public long RuntimeMs { get; set; }
        }
    }
}