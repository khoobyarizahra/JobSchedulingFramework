using JobShopSchedulingFramework.Data;
using JobShopSchedulingFramework.DataGeneration;
using JobShopSchedulingFramework.Evaluation;
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
            Console.WriteLine(" Giffler-Thompson Initial Heuristic");
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
    }
}