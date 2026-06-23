using JobShopSchedulingFramework.Data;
using JobShopSchedulingFramework.DataGeneration;
using JobShopSchedulingFramework.Evaluation;
using JobShopSchedulingFramework.ExactSolvers;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods;
using JobShopSchedulingFramework.Models;
using JobShopSchedulingFramework.Results;
using JobShopSchedulingFramework.Visualisation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace JobShopSchedulingFramework.Application
{
    public static class SchedulingApplication
    {
        private const int TimeLimit90Seconds = 90;

        // Safety limit for the 90s run.
        // The actual stopping criterion is the time limit.
        private const int MaxIterationsForTimeLimitedRun = 1_000_000;

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

            TabuRunMode runMode =
                SelectTabuRunMode();

            RunHeuristicExperiment(
                fileName,
                runMode);

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

        private static TabuRunMode SelectTabuRunMode()
        {
            Console.WriteLine();
            Console.WriteLine("Select Tabu Search run mode:");
            Console.WriteLine("1 - Tabu Search with 90 seconds time limit");
            Console.WriteLine("2 - Tabu Search extended run without fixed time limit");
            Console.WriteLine("3 - Run both modes");
            Console.Write("Choice: ");

            while (true)
            {
                string? choice =
                    Console.ReadLine();

                if (choice == "1")
                {
                    return TabuRunMode.TimeLimit90Seconds;
                }

                if (choice == "2")
                {
                    return TabuRunMode.ExtendedWithoutFixedTimeLimit;
                }

                if (choice == "3")
                {
                    return TabuRunMode.Both;
                }

                Console.Write("Invalid input. Please enter 1, 2, or 3: ");
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
                .GenerateAndSave(
                    10,
                    5,
                    Path.Combine(outputFolder, "Team_Instance1_10x5_Partial.txt"));

            new TestInstanceGenerator(102, TestInstanceType.Full)
                .GenerateAndSave(
                    10,
                    5,
                    Path.Combine(outputFolder, "Team_Instance2_10x5_Full.txt"));

            new TestInstanceGenerator(103, TestInstanceType.Partial)
                .GenerateAndSave(
                    15,
                    10,
                    Path.Combine(outputFolder, "Team_Instance3_15x10_Partial.txt"));

            new TestInstanceGenerator(104, TestInstanceType.Full)
                .GenerateAndSave(
                    15,
                    10,
                    Path.Combine(outputFolder, "Team_Instance4_15x10_Full.txt"));

            new TestInstanceGenerator(105, TestInstanceType.Partial)
                .GenerateAndSave(
                    20,
                    15,
                    Path.Combine(outputFolder, "Team_Instance5_20x15_Partial.txt"));

            Console.WriteLine();
            Console.WriteLine("Test instances generated successfully in:");
            Console.WriteLine(outputFolder);
            Console.WriteLine();
        }

        private static void RunHeuristicExperiment(
            string fileName,
            TabuRunMode runMode)
        {
            Console.WriteLine("Selected instance:");
            Console.WriteLine(fileName);
            Console.WriteLine();

            InitialHeuristicResult result =
                HeuristicExperiment.Run(fileName);

            Instance initialInstanceForChart =
                CloneInstance(result.bestInstance);

            List<TabuNeighborhoodExperimentResult> tabuResults =
                RunFinalTabuSearch(
                    result,
                    runMode);

            TabuNeighborhoodExperimentResult bestTabuResult =
                tabuResults
                    .OrderBy(resultItem => resultItem.BestCmax)
                    .ThenBy(resultItem => resultItem.RuntimeMs)
                    .First();

            string instanceName =
                Path.GetFileNameWithoutExtension(fileName);

            // Store all generated Gantt charts in the Results folder.
            // Each instance gets its own subfolder so that files are not overwritten.
            string outputFolder =
                Path.Combine(
                    GetProjectRootFolder(),
                    "Results",
                    "GanttCharts",
                    instanceName);

            Directory.CreateDirectory(outputFolder);

            string initialOutputPath =
                Path.Combine(
                    outputFolder,
                    instanceName + "_initial_heuristic.html");

            string cpOutputPath =
                Path.Combine(
                    outputFolder,
                    instanceName + "_cp_solver.html");

            string comparisonOutputPath =
                Path.Combine(
                    outputFolder,
                    instanceName + "_comparison.html");

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
                bestTabuResult.BestCmax,
                cpCmax);

            GantChart.CreateHtml(
                initialInstanceForChart,
                initialOutputPath,
                "Initial Heuristic - " + result.bestRule,
                result.bestCmax,
                "FEASIBLE");

            string bestTabuOutputPath =
                "";

            foreach (TabuNeighborhoodExperimentResult tabuResult in tabuResults)
            {
                string tabuOutputPath =
                    Path.Combine(
                        outputFolder,
                        instanceName + "_" + tabuResult.FileSuffix + ".html");

                GantChart.CreateHtml(
                    tabuResult.BestInstance,
                    tabuOutputPath,
                    "Tabu Search - " + tabuResult.BestNeighborhoodName + " - " + tabuResult.RunLabel,
                    tabuResult.BestCmax,
                    "FEASIBLE");

                if (tabuResult == bestTabuResult)
                {
                    bestTabuOutputPath =
                        tabuOutputPath;
                }
            }

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
                Path.GetFileName(bestTabuOutputPath),
                Path.GetFileName(cpOutputPath),
                result.bestRule.ToString(),
                bestTabuResult.BestNeighborhoodName + " - " + bestTabuResult.RunLabel,
                result.bestCmax,
                bestTabuResult.BestCmax,
                cpCmax);

            PrintFinalSummary(
                result,
                tabuResults,
                cpResult);

            List<CsvResultRow> csvRows =
                CreateCsvResultRows(
                    fileName,
                    tabuResults);

            string csvOutputPath =
                CsvResultWriter.WriteResults(
                    csvRows);

            Console.WriteLine();
            Console.WriteLine("CSV result file updated:");
            Console.WriteLine(csvOutputPath);

            Console.WriteLine();
            Console.WriteLine("Gantt charts created in:");
            Console.WriteLine(outputFolder);

            Console.WriteLine();
            Console.WriteLine("Comparison Gantt chart created:");
            Console.WriteLine(comparisonOutputPath);

            Process.Start(new ProcessStartInfo
            {
                FileName = Path.GetFullPath(comparisonOutputPath),
                UseShellExecute = true
            });
        }

        private static List<TabuNeighborhoodExperimentResult> RunFinalTabuSearch(
            InitialHeuristicResult result,
            TabuRunMode runMode)
        {
            Console.WriteLine();
            Console.WriteLine("=======================================");
            Console.WriteLine(" FINAL TABU SEARCH N3 - All Pair Critical Block Swap");
            Console.WriteLine("=======================================");

            List<TabuNeighborhoodExperimentResult> results =
                new List<TabuNeighborhoodExperimentResult>();

            if (runMode == TabuRunMode.TimeLimit90Seconds ||
                runMode == TabuRunMode.Both)
            {
                TabuNeighborhoodExperimentResult timeLimitedResult =
                    RunSingleFinalTabuSearch(
                        result,
                        "N3 - All Pair Critical Block Swaps",
                        new AllPairSwapNeighborhood(),
                        "90s",
                        "tabu_search_90s",
                        maxIterations: MaxIterationsForTimeLimitedRun,
                        timeLimitSeconds: TimeLimit90Seconds);

                results.Add(timeLimitedResult);
            }

            if (runMode == TabuRunMode.ExtendedWithoutFixedTimeLimit ||
                runMode == TabuRunMode.Both)
            {
                int extendedMaxIterations =
                    CalculateExtendedMaxIterations(
                        result.bestInstance);

                TabuNeighborhoodExperimentResult extendedResult =
                    RunSingleFinalTabuSearch(
                        result,
                        "N3 - All Pair Critical Block Swaps",
                        new AllPairSwapNeighborhood(),
                        "Extended without fixed time limit",
                        "tabu_search_extended",
                        maxIterations: extendedMaxIterations,
                        timeLimitSeconds: 0);

                results.Add(extendedResult);
            }

            Console.WriteLine();
            Console.WriteLine("FINAL TABU SUMMARY");
            Console.WriteLine("---------------------------------------");

            foreach (TabuNeighborhoodExperimentResult item in results)
            {
                Console.WriteLine(
                    item.RunLabel.PadRight(35) +
                    " | Cmax: " +
                    item.BestCmax.ToString().PadRight(8) +
                    " | Runtime: " +
                    item.RuntimeMs +
                    " ms");
            }

            return results;
        }

        private static TabuNeighborhoodExperimentResult RunSingleFinalTabuSearch(
            InitialHeuristicResult result,
            string neighborhoodName,
            INeighborhoodDefinition neighborhoodDefinition,
            string runLabel,
            string fileSuffix,
            int maxIterations,
            int timeLimitSeconds)
        {
            Console.WriteLine();
            Console.WriteLine("Running final Tabu Search:");
            Console.WriteLine("Neighborhood: " + neighborhoodName);
            Console.WriteLine("Run mode: " + runLabel);

            if (timeLimitSeconds > 0)
            {
                Console.WriteLine("Stopping criterion: time limit");
                Console.WriteLine("Time limit: " + timeLimitSeconds + " seconds");
                Console.WriteLine("Max iterations safety limit: " + maxIterations);
            }
            else
            {
                Console.WriteLine("Stopping criterion: maxIterations only");
                Console.WriteLine("Max iterations: " + maxIterations);
                Console.WriteLine("Time limit: none");
            }

            Instance instanceCopy =
                CloneInstance(result.bestInstance);

            Stopwatch stopwatch =
                Stopwatch.StartNew();

            TabuSearchSolver tabuSearch =
                new TabuSearchSolver(
                    maxIterations: maxIterations,
                    timeLimitSeconds: timeLimitSeconds,
                    neighborhoodDefinition: neighborhoodDefinition);

            Console.WriteLine();

            if (timeLimitSeconds > 0)
            {
                Console.WriteLine("Tabu Search is running now.");
                Console.WriteLine(
                    "Please wait. This run uses a " +
                    timeLimitSeconds +
                    "-second time limit.");
            }
            else
            {
                Console.WriteLine("Tabu Search is running now.");
                Console.WriteLine(
                    "Please wait. This extended run has no fixed time limit and stops after " +
                    maxIterations +
                    " iterations.");
            }

            Console.WriteLine();

            int tabuCmax =
                tabuSearch.Run(instanceCopy);

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("Result for " + neighborhoodName + " - " + runLabel);
            Console.WriteLine("Tabu Cmax: " + tabuCmax);
            Console.WriteLine("Runtime: " + stopwatch.Elapsed.TotalSeconds.ToString("F2") + " s");
            Console.WriteLine("Runtime ms: " + stopwatch.ElapsedMilliseconds);

            return new TabuNeighborhoodExperimentResult
            {
                BestInstance = instanceCopy,
                BestCmax = tabuCmax,
                BestNeighborhoodName = neighborhoodName,
                RunLabel = runLabel,
                FileSuffix = fileSuffix,
                RuntimeMs = stopwatch.ElapsedMilliseconds,
                MaxIterations = maxIterations,
                TimeLimitSeconds = timeLimitSeconds
            };
        }

        /// <summary>
        /// Calculates the maximum number of iterations for the extended run.
        /// 
        /// The extended mode does not use a fixed time limit. Therefore, the runtime
        /// is controlled indirectly by the number of iterations.
        /// 
        /// Larger instances need fewer iterations because one iteration is more expensive:
        /// more operations create larger machine orders, more critical blocks, and more
        /// neighborhood moves.
        /// </summary>
        private static int CalculateExtendedMaxIterations(
            Instance instance)
        {
            int operationCount =
                instance.Jobs
                    .Sum(job => job.Operations.Count);

            if (operationCount <= 60)
            {
                return 200_000;
            }

            if (operationCount <= 120)
            {
                return 120_000;
            }

            if (operationCount <= 200)
            {
                return 80_000;
            }

            if (operationCount <= 350)
            {
                return 50_000;
            }

            return 30_000;
        }

        private static void PrintFinalSummary(
            InitialHeuristicResult initialResult,
            List<TabuNeighborhoodExperimentResult> tabuResults,
            CpSolverResult cpResult)
        {
            Console.WriteLine();
            Console.WriteLine("=======================================");
            Console.WriteLine(" FINAL RESULT SUMMARY");
            Console.WriteLine("=======================================");

            Console.WriteLine(
                "Initial heuristic".PadRight(40) +
                " | Cmax: " +
                initialResult.bestCmax.ToString().PadRight(8) +
                " | Rule: " +
                initialResult.bestRule);

            foreach (TabuNeighborhoodExperimentResult tabuResult in tabuResults)
            {
                double improvementPercent =
                    initialResult.bestCmax > 0
                        ? (double)(initialResult.bestCmax - tabuResult.BestCmax) / initialResult.bestCmax * 100.0
                        : 0.0;

                string stoppingCriterion =
                    tabuResult.TimeLimitSeconds > 0
                        ? "time limit"
                        : "maxIterations";

                Console.WriteLine(
                    ("Tabu Search N3 (" + tabuResult.RunLabel + ")").PadRight(55) +
                    " | Cmax: " +
                    tabuResult.BestCmax.ToString().PadRight(8) +
                    " | Runtime: " +
                    (tabuResult.RuntimeMs / 1000.0).ToString("F2").PadRight(8) +
                    " s | Stop: " +
                    stoppingCriterion.PadRight(13) +
                    " | Improvement: " +
                    improvementPercent.ToString("F2") +
                    "%");
            }

            if (cpResult.HasFeasibleSolution)
            {
                Console.WriteLine(
                    "CP Solver".PadRight(40) +
                    " | Cmax: " +
                    cpResult.Cmax.ToString().PadRight(8) +
                    " | Status: FEASIBLE");
            }
            else
            {
                Console.WriteLine(
                    "CP Solver".PadRight(40) +
                    " | Status: NO FEASIBLE SOLUTION FOUND");
            }
        }

        /// <summary>
        /// Creates the CSV result rows required for the project submission.
        ///
        /// The CSV submission contains only the results of the final metaheuristic.
        /// CP solver and initial heuristic results are used for comparison, but they
        /// are not written to the submission CSV.
        /// </summary>
        private static List<CsvResultRow> CreateCsvResultRows(
            string instanceFileName,
            List<TabuNeighborhoodExperimentResult> tabuResults)
        {
            List<CsvResultRow> rows =
                new List<CsvResultRow>();

            string instanceName =
                Path.GetFileNameWithoutExtension(
                    instanceFileName);

            string instanceSetName =
                ExtractInstanceSetName(
                    instanceFileName);

            foreach (TabuNeighborhoodExperimentResult tabuResult in tabuResults)
            {
                string algorithmName;

                if (tabuResult.TimeLimitSeconds > 0)
                {
                    algorithmName =
                        "Team Tabu Search N3 (90s)";
                }
                else
                {
                    algorithmName =
                        "Team Tabu Search N3 (without time limit)";
                }

                rows.Add(
                    new CsvResultRow
                    {
                        InstanceSetName = instanceSetName,
                        InstanceName = instanceName,
                        Algorithm = algorithmName,
                        Status = "FEASIBLE",
                        ObjectiveValue = tabuResult.BestCmax,
                        ComputationTimeSeconds = tabuResult.RuntimeMs / 1000.0
                    });
            }

            return rows;
        }

        /// <summary>
        /// Extracts the instance set name from the instance file name.
        ///
        /// Example:
        /// ClassroomInstanceSet3_3.txt -> ClassroomInstanceSet3
        /// </summary>
        private static string ExtractInstanceSetName(
            string instanceFileName)
        {
            string instanceName =
                Path.GetFileNameWithoutExtension(
                    instanceFileName);

            int lastUnderscoreIndex =
                instanceName.LastIndexOf('_');

            if (lastUnderscoreIndex > 0)
            {
                return instanceName.Substring(
                    0,
                    lastUnderscoreIndex);
            }

            string? parentFolder =
                Path.GetFileName(
                    Path.GetDirectoryName(instanceFileName));

            if (!string.IsNullOrWhiteSpace(parentFolder))
            {
                return parentFolder;
            }

            return "UnknownInstanceSet";
        }

        /// <summary>
        /// Returns the project root folder.
        /// 
        /// During execution, relative paths start in bin/Debug/netX.
        /// This method moves three levels up to the project folder.
        /// </summary>
        private static string GetProjectRootFolder()
        {
            return Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    @"..\..\.."));
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

        private enum TabuRunMode
        {
            TimeLimit90Seconds,
            ExtendedWithoutFixedTimeLimit,
            Both
        }

        private class TabuNeighborhoodExperimentResult
        {
            public Instance BestInstance { get; set; } = null!;
            public int BestCmax { get; set; }
            public string BestNeighborhoodName { get; set; } = "";
            public string RunLabel { get; set; } = "";
            public string FileSuffix { get; set; } = "";
            public long RuntimeMs { get; set; }
            public int MaxIterations { get; set; }
            public int TimeLimitSeconds { get; set; }
        }
    }
}