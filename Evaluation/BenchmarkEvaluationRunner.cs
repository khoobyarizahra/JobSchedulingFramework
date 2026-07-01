using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods;
using JobShopSchedulingFramework.Models;
using JobShopSchedulingFramework.Results;

namespace JobShopSchedulingFramework.Evaluation
{
    public static class BenchmarkEvaluationRunner
    {
        private const string AlgorithmWithoutTimeLimit =
            "Team F Tabu Search (Ohne Zeitlimit)";

        private const string AlgorithmWith90Seconds =
            "Team F Tabu Search (90s)";

        private const int TimeLimit90Seconds = 90;

        public static void Run()
        {
            Console.WriteLine();
            Console.WriteLine("=======================================");
            Console.WriteLine(" FULL BENCHMARK EVALUATION");
            Console.WriteLine(" Team F Tabu Search");
            Console.WriteLine("=======================================");
            Console.WriteLine();

            string benchmarkFolder =
                Path.Combine(
                    GetProjectRootFolder(),
                    "Instances",
                    "Benchmark");

            if (!Directory.Exists(benchmarkFolder))
            {
                Console.WriteLine("Benchmark folder not found:");
                Console.WriteLine(benchmarkFolder);
                return;
            }

            List<string> instanceFiles =
                Directory.GetFiles(
                        benchmarkFolder,
                        "*.txt",
                        SearchOption.TopDirectoryOnly)
                    .OrderBy(file => GetSortingKey(Path.GetFileNameWithoutExtension(file)))
                    .ToList();

            Console.WriteLine("Found benchmark instances: " + instanceFiles.Count);
            Console.WriteLine();

            List<CsvResultRow> resultRows =
                new List<CsvResultRow>();

            int processedInstances =
                0;

            foreach (string file in instanceFiles)
            {
                processedInstances++;

                string fileNameWithoutExtension =
                    Path.GetFileNameWithoutExtension(file);

                (string instanceSetName, string instanceName) =
                    ExtractInstanceInfo(fileNameWithoutExtension);

                Console.WriteLine();
                Console.WriteLine("=======================================");
                Console.WriteLine(
                    "Instance " +
                    processedInstances +
                    " / " +
                    instanceFiles.Count);
                Console.WriteLine(Path.GetFileName(file));
                Console.WriteLine("=======================================");
                Console.WriteLine();

                /*
                First, the constructive heuristic is executed.
                It creates the initial solution that is used as the starting
                point for both Tabu Search run modes.
                */
                InitialHeuristicResult initialResult =
                    HeuristicExperiment.Run(file);

                int instanceDependentMaxIterations =
                    CalculateInstanceDependentMaxIterations(initialResult.bestInstance);

                /*
                Extended mode:
                This mode does not use the fixed 90-second time limit.
                It stops when the instance-dependent iteration limit is reached
                or when the technical 300-second safety limit inside the solver
                is reached.
                */
                CsvResultRow extendedRow =
                    RunSingleTabuForCsv(
                        initialResult,
                        instanceSetName,
                        instanceName,
                        AlgorithmWithoutTimeLimit,
                        maxIterations: instanceDependentMaxIterations,
                        timeLimitSeconds: 0);

                resultRows.Add(extendedRow);

                /*
                90-second mode:
                This mode now uses the same instance-dependent iteration limit
                as the first stopping criterion. The 90-second time limit remains
                as a hard upper bound.
                */
                CsvResultRow timeLimitedRow =
                    RunSingleTabuForCsv(
                        initialResult,
                        instanceSetName,
                        instanceName,
                        AlgorithmWith90Seconds,
                        maxIterations: instanceDependentMaxIterations,
                        timeLimitSeconds: TimeLimit90Seconds);

                resultRows.Add(timeLimitedRow);

                /*
                The result file is overwritten after each processed instance.
                This keeps the benchmark progress available even if a later
                instance takes long or the program is interrupted.
                */
                string temporaryOutputFile =
                    CsvResultWriter.WriteResultsToFile(
                        resultRows,
                        "TeamF_BenchmarkResults.csv",
                        append: false);

                Console.WriteLine();
                Console.WriteLine("Progress saved:");
                Console.WriteLine(temporaryOutputFile);
                Console.WriteLine("Rows written so far: " + resultRows.Count);
            }

            Console.WriteLine();
            Console.WriteLine("Full benchmark evaluation finished.");
            Console.WriteLine("Total rows written: " + resultRows.Count);
        }

        private static CsvResultRow RunSingleTabuForCsv(
            InitialHeuristicResult initialResult,
            string instanceSetName,
            string instanceName,
            string algorithmName,
            int maxIterations,
            int timeLimitSeconds)
        {
            Console.WriteLine();
            Console.WriteLine("Running: " + algorithmName);
            Console.WriteLine("Max iterations: " + maxIterations);
            Console.WriteLine("Time limit seconds: " + timeLimitSeconds);

            if (timeLimitSeconds == 0)
            {
                Console.WriteLine("Extended mode uses instance-dependent maxIterations.");
                Console.WriteLine("Technical safety limit inside TabuSearchSolver: 300 seconds.");
            }
            else
            {
                Console.WriteLine("90s mode uses instance-dependent maxIterations first.");
                Console.WriteLine("The 90-second time limit remains the hard upper bound.");
            }

            Console.WriteLine();

            /*
            A copy of the initial instance is used for each run mode.
            This prevents the first Tabu Search run from modifying the solution
            object that is needed for the second run.
            */
            Instance instanceCopy =
                CloneInstance(initialResult.bestInstance);

            Stopwatch stopwatch =
                Stopwatch.StartNew();

            TabuSearchSolver tabuSearch =
                new TabuSearchSolver(
                    maxIterations: maxIterations,
                    timeLimitSeconds: timeLimitSeconds,
                    neighborhoodDefinition: new AllPairSwapNeighborhood());

            int tabuCmax =
                tabuSearch.Run(instanceCopy);

            stopwatch.Stop();

            return new CsvResultRow
            {
                InstanceSetName = instanceSetName,
                InstanceName = instanceName,
                Algorithm = algorithmName,
                Status = "Feasible",
                ObjectiveValue = tabuCmax,
                ComputationTimeSeconds = stopwatch.Elapsed.TotalSeconds
            };
        }

        private static int CalculateInstanceDependentMaxIterations(
            Instance instance)
        {
            int operationCount =
                instance.Jobs
                    .Sum(job => job.Operations.Count);

            if (operationCount <= 0)
            {
                throw new InvalidOperationException(
                    "Instance contains no operations.");
            }

            /*
            The number of operations is used as an indicator of instance size.

            Small instances receive a larger iteration budget because each
            iteration is relatively cheap. Large instances receive a smaller
            iteration budget because critical path analysis, critical block
            construction and neighborhood evaluation become more expensive.
            */
            if (operationCount <= 60)
            {
                return 250_000;
            }

            if (operationCount <= 100)
            {
                return 200_000;
            }

            if (operationCount <= 200)
            {
                return 120_000;
            }

            if (operationCount <= 300)
            {
                return 80_000;
            }

            if (operationCount <= 450)
            {
                return 40_000;
            }

            if (operationCount <= 650)
            {
                return 20_000;
            }

            if (operationCount <= 850)
            {
                return 10_000;
            }

            return 8_000;
        }

        private static string GetProjectRootFolder()
        {
            return Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    @"..\..\.."));
        }

        private static (string InstanceSetName, string InstanceName) ExtractInstanceInfo(
            string fileNameWithoutExtension)
        {
            string instanceName =
                fileNameWithoutExtension;

            int lastUnderscoreIndex =
                fileNameWithoutExtension.LastIndexOf('_');

            if (lastUnderscoreIndex < 0)
            {
                return (fileNameWithoutExtension, instanceName);
            }

            string instanceSetName =
                fileNameWithoutExtension.Substring(
                    0,
                    lastUnderscoreIndex);

            if (instanceSetName == "Team F")
            {
                instanceSetName =
                    "TeamF";
            }

            return (instanceSetName, instanceName);
        }

        private static string GetSortingKey(
            string fileName)
        {
            /*
            Classroom instances are sorted numerically by set and instance number.
            This avoids the lexical order problem where Set10 would appear before Set2.
            */
            if (fileName.StartsWith("ClassroomInstanceSet"))
            {
                string remaining =
                    fileName.Replace("ClassroomInstanceSet", "");

                string[] parts =
                    remaining.Split('_');

                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int setNumber) &&
                    int.TryParse(parts[1], out int instanceNumber))
                {
                    return $"A_{setNumber:D3}_{instanceNumber:D3}";
                }
            }

            /*
            Team instances are sorted after the classroom instances.
            */
            if (fileName.StartsWith("Team"))
            {
                return $"B_{fileName}";
            }

            return $"Z_{fileName}";
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
    }
}