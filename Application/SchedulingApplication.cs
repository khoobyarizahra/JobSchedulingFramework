using JobShopSchedulingFramework.Data;
using JobShopSchedulingFramework.DataGeneration;
using JobShopSchedulingFramework.Evaluation;
using JobShopSchedulingFramework.Heuristics.Tabu;
using JobShopSchedulingFramework.Models;
using JobShopSchedulingFramework.Visualisation;
using System;
using System.Diagnostics;
using System.IO;

namespace JobShopSchedulingFramework.Application
{
    public static class SchedulingApplication
    {
        public static void Run(string[] args)
        {
            PrintHeader();

            GenerateControlledInstances();

            string fileName =
                @"Instances\Generated\SetupHeavy_10x5_seed43.txt";

            RunHeuristicExperiment(fileName);

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void PrintHeader()
        {
            Console.WriteLine("=======================================");
            Console.WriteLine(" JOB SHOP SCHEDULING PROJECT");
            Console.WriteLine(" Giffler-Thompson + Tabu Search");
            Console.WriteLine("=======================================");
            Console.WriteLine();
        }

        private static void GenerateControlledInstances()
        {
            int numberOfJobs = 10;
            int numberOfMachines = 5;

            InstanceType type = InstanceType.SetupHeavy;
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

            RunTabuSearchExperiment(result);

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

            string fullPath = Path.GetFullPath(outputPath);

            Process.Start(new ProcessStartInfo
            {
                FileName = fullPath,
                UseShellExecute = true
            });
        }

        private static void RunTabuSearchExperiment(InitialHeuristicResult result)
        {
            Console.WriteLine();
            Console.WriteLine("=======================================");
            Console.WriteLine(" TABU SEARCH EXPERIMENT");
            Console.WriteLine("=======================================");

            int initialCmax = result.bestCmax;

            Console.WriteLine("Initial best rule: " + result.bestRule);
            Console.WriteLine("Initial Cmax: " + initialCmax);

            Stopwatch stopwatch = Stopwatch.StartNew();

            TabuSearchSolver tabuSearch =
                new TabuSearchSolver(
                    maxIterations: 100,
                    tabuTenure: 10);

            int tabuCmax =
                tabuSearch.Run(result.bestInstance);

            stopwatch.Stop();

            int improvement = initialCmax - tabuCmax;

            double improvementPercent =
                initialCmax > 0
                    ? (double)improvement / initialCmax * 100.0
                    : 0.0;

            Console.WriteLine();
            Console.WriteLine("TABU SEARCH RESULT");
            Console.WriteLine("Initial Cmax: " + initialCmax);
            Console.WriteLine("Tabu Cmax: " + tabuCmax);
            Console.WriteLine("Improvement: " + improvement);
            Console.WriteLine("Improvement %: " + improvementPercent.ToString("F2") + "%");
            Console.WriteLine("Runtime ms: " + stopwatch.ElapsedMilliseconds);
        }
    }
}