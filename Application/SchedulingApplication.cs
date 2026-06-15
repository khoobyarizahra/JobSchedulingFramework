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

            string? instanceFolder =
                SelectApplicationAction();

            if (instanceFolder == null)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

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

        private static string? SelectApplicationAction()
        {
            Console.WriteLine("Select action:");
            Console.WriteLine("1 - Generate test instances");
            Console.WriteLine("2 - Solve generated instances");
            Console.WriteLine("3 - Solve benchmark instances");
            Console.WriteLine("4 - Exit");
            Console.Write("Choice: ");

            while (true)
            {
                string? choice =
                    Console.ReadLine();

                if (choice == "1")
                {
                    GenerateTestInstances();
                    return null;
                }

                if (choice == "2")
                {
                    return Path.GetFullPath(
                        Path.Combine(
                            AppContext.BaseDirectory,
                            @"..\..\..\Instances\Generated"));
                }

                if (choice == "3")
                {
                    return Path.GetFullPath(
                        Path.Combine(
                            AppContext.BaseDirectory,
                            @"..\..\..\Instances\Benchmark"));
                }

                if (choice == "4")
                {
                    return null;
                }

                Console.Write("Invalid input. Please enter 1, 2, 3, or 4: ");
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

        private static void GenerateTestInstances()
        {
            string outputFolder =
                Path.GetFullPath(
                    Path.Combine(
                        AppContext.BaseDirectory,
                        @"..\..\..\Instances\Generated"));

            Directory.CreateDirectory(outputFolder);

            new TestInstanceGenerator(101, TestInstanceType.Partial)
                .GenerateAndSave(10, 5, Path.Combine(outputFolder, "Team_Instance1_10x5_Partial.txt"));

            new TestInstanceGenerator(102, TestInstanceType.Full)
                .GenerateAndSave(10, 5, Path.Combine(outputFolder, "Team_Instance2_10x5_Full.txt"));

            new TestInstanceGenerator(103, TestInstanceType.Partial)
                .GenerateAndSave(15, 10, Path.Combine(outputFolder, "Team_Instance3_15x10_Partial.txt"));

            new TestInstanceGenerator(104, TestInstanceType.Full)
                .GenerateAndSave(15, 10, Path.Combine(outputFolder, "Team_Instance4_15x10_Full.txt"));

            new TestInstanceGenerator(105, TestInstanceType.Partial)
                .GenerateAndSave(20, 15, Path.Combine(outputFolder, "Team_Instance5_20x15_Partial.txt"));

            Console.WriteLine();
            Console.WriteLine("Test instances generated successfully in:");
            Console.WriteLine(outputFolder);
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
            RunFinalTabuSearch(result);

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

        private static TabuNeighborhoodExperimentResult RunFinalTabuSearch(
    InitialHeuristicResult result)
        {
            Console.WriteLine();
            Console.WriteLine("=======================================");
            Console.WriteLine(" FINAL TABU SEARCH COMPARISON WITH 90S LIMIT");
            Console.WriteLine("=======================================");

            TabuNeighborhoodExperimentResult n3Result =
                RunSingleFinalTabuSearch(
                    result,
                    "N3 - All Pair Critical Block Swaps",
                    new AllPairSwapNeighborhood());
            TabuNeighborhoodExperimentResult setupResult =
                RunSingleFinalTabuSearch(
                    result,
                    "N5 - Setup Heavy Machine Moves",
                    new SetupHeavyMachineNeighborhood());

            TabuNeighborhoodExperimentResult combinedResult =
                RunSingleFinalTabuSearch(
                    result,
                    "Combined Neighborhood",
                    new CombinedNeighborhood());

            Console.WriteLine();
            Console.WriteLine("FINAL 90S TABU COMPARISON");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("N3 Cmax: " + n3Result.BestCmax);
            Console.WriteLine("N5 Cmax: " + setupResult.BestCmax);
            Console.WriteLine("Combined Cmax: " + combinedResult.BestCmax);

            if (combinedResult.BestCmax <= n3Result.BestCmax)
            {
                Console.WriteLine("Selected final neighborhood: Combined Neighborhood");
                return combinedResult;
            }

            Console.WriteLine("Selected final neighborhood: N3 - All Pair Critical Block Swaps");
            return n3Result;
        }

        private static TabuNeighborhoodExperimentResult RunSingleFinalTabuSearch(
    InitialHeuristicResult result,
    string neighborhoodName,
    INeighborhoodDefinition neighborhoodDefinition)
        {
            Console.WriteLine();
            Console.WriteLine("Running final Tabu Search with: " + neighborhoodName);

            Instance instanceCopy =
                CloneInstance(result.bestInstance);

            Stopwatch stopwatch =
                Stopwatch.StartNew();

            int maxIterationsForTenure =
            Math.Max(
                5000,
                result.bestInstance.NumJobs *
                result.bestInstance.NumMachines *
                25);

            TabuSearchSolver tabuSearch =
                new TabuSearchSolver(
                    maxIterations: maxIterationsForTenure,
                    timeLimitSeconds: 90,
                    neighborhoodDefinition: neighborhoodDefinition);

            int tabuCmax =
                tabuSearch.Run(instanceCopy);

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("Result for " + neighborhoodName);
            Console.WriteLine("Tabu Cmax: " + tabuCmax);
            Console.WriteLine("Runtime ms: " + stopwatch.ElapsedMilliseconds);

            return new TabuNeighborhoodExperimentResult
            {
                BestInstance = instanceCopy,
                BestCmax = tabuCmax,
                BestNeighborhoodName = neighborhoodName,
                RuntimeMs = stopwatch.ElapsedMilliseconds
            };
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

            int maxIterations = 5000;

            int neighborhoodExperimentTimeLimitSeconds =
                int.MaxValue;

            List<(string Name, INeighborhoodDefinition Neighborhood)> neighborhoods =
            new List<(string Name, INeighborhoodDefinition Neighborhood)>
            {
                ("N1 - Adjacent Critical Block Swaps", new AdjacentSwapNeighborhood()),
                ("N2 - Restricted First/Last Block Swaps", new RestrictedBlockSwapNeighborhood()),
                ("N3 - All Pair Critical Block Swaps", new AllPairSwapNeighborhood()),
                ("N4 - Critical Block Insert Moves", new CriticalBlockInsertNeighborhood()),
                ("N5 - Setup Heavy Machine Moves", new SetupHeavyMachineNeighborhood())
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
                timeLimitSeconds: neighborhoodExperimentTimeLimitSeconds,
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