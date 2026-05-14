using JobShopSchedulingFramework.Data;
using JobShopSchedulingFramework.DataGeneration;
using JobShopSchedulingFramework.Evaluation;
using JobShopSchedulingFramework.Models;
using System;

/*
 USING STATEMENTS

 These imports allow access to classes from other folders/namespaces.

 Data:
 - InstanceWriter
 - reading/writing functionality

 DataGeneration:
 - InstanceGeneratorAdvanced
 - InstanceType

 Experiments:
 - HeuristicExperiment

 Models:
 - Instance class

 System:
 - Console output
 - basic C# functionality
*/

namespace JobShopSchedulingFramework.Application
{
    /*
     SCHEDULING APPLICATION

     This class controls the complete workflow of the project.

     Important architecture idea:
     Program.cs should stay extremely small.

     Therefore:
     - Program.cs only starts the application
     - SchedulingApplication coordinates the real workflow

     This improves:
     - readability
     - maintainability
     - scalability
     - debugging
    */
    public static class SchedulingApplication
    {
        /*
         RUN METHOD

         This is the central starting method of the project.

         Workflow:
         1. Print project header
         2. Generate reproducible instances
         3. Select one instance
         4. Run heuristic experiment
         5. Keep console open
        */
        public static void Run(string[] args)
        {
            /*
             Prints a clean title in the console.
            */
            PrintHeader();

            /*
             STEP 1:
             Generate several reproducible instances.

             Reproducible means:
             same seed + same parameters
             -> same instance
            */
            GenerateControlledInstances();

            /*
             STEP 2:
             Select one generated instance for testing.

             This file will later be read by the heuristic experiment.
            */
            string fileName =
                @"Instances\Generated\SetupHeavy_10x5_seed43.txt";

            /*
             STEP 3:
             Run comparison of all priority rules
             on the selected instance.
            */
            RunHeuristicExperiment(fileName);

            /*
             Keeps console window open
             when starting from Visual Studio.
            */
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /*
         PRINT HEADER

         Only responsible for clean console output.

         Separating this into its own method
         keeps Run() cleaner and easier to read.
        */
        private static void PrintHeader()
        {
            Console.WriteLine("=======================================");
            Console.WriteLine(" JOB SHOP SCHEDULING PROJECT");
            Console.WriteLine(" Giffler-Thompson Initial Heuristic");
            Console.WriteLine("=======================================");
            Console.WriteLine();
        }

        /*
         GENERATE CONTROLLED INSTANCES

         Purpose:
         Create several reproducible scheduling instances.

         Important scientific idea:
         Random instances should still be reproducible.

         Therefore:
         We use fixed seeds.

         Same:
         - seed
         - instance type
         - number of jobs
         - number of machines

         -> always produces the exact same instance.

         Different seeds:
         -> different instances.
        */
        private static void GenerateControlledInstances()
        {
            /*
             Number of jobs in each generated instance.
            */
            int numberOfJobs = 10;

            /*
             Number of available machines.
            */
            int numberOfMachines = 5;

            /*
             Defines the type/category of generated instances.

             Example:
             SetupHeavy means:
             - relatively large setup times
             - useful for testing setup-aware heuristics
            */
            InstanceType type = InstanceType.SetupHeavy;

            /*
             Base seed for reproducibility.

             The seed controls the random number generator.

             Important:
             Same seed -> same random sequence.
            */
            int baseSeed = 42;

            /*
             Number of instances to generate.
            */
            int numberOfInstances = 5;

            /*
             Folder where generated instances are saved.
            */
            string outputFolder =
                @"Instances\Generated";

            /*
             InstanceWriter is responsible only for:
             saving instances into text files.

             Important architecture principle:
             - generator creates instances
             - writer saves instances
            */
            InstanceWriter writer =
                new InstanceWriter();

            /*
             Generate multiple instances.

             Each iteration uses:
             a different seed.
            */
            for (int i = 1; i <= numberOfInstances; i++)
            {
                /*
                 Create a unique seed for this instance.

                 Example:
                 43, 44, 45, ...

                 This ensures:
                 - different instances
                 - still reproducible
                */
                int seed = baseSeed + i;

                /*
                 Create generator object.

                 Parameters:
                 - seed
                 - instance type

                 The generator internally creates:
                 - jobs
                 - operations
                 - processing times
                 - setup times
                */
                InstanceGeneratorAdvanced generator =
                    new InstanceGeneratorAdvanced(seed, type);

                /*
                 Generate one scheduling instance.
                */
                Instance instance =
                    generator.Generate(
                        numberOfJobs,
                        numberOfMachines
                    );

                /*
                 Build descriptive file name.

                 Example:
                 SetupHeavy_10x5_seed43.txt

                 This is very useful later because:
                 - experiments become traceable
                 - instances become reproducible
                 - debugging becomes easier
                */
                string fileName =
                    $@"{outputFolder}\{type}_{numberOfJobs}x{numberOfMachines}_seed{seed}.txt";

                /*
                 Save generated instance into file.
                */
                writer.WriteToFile(instance, fileName);

                /*
                 Console output for user feedback.
                */
                Console.WriteLine(
                    "Generated: " + fileName
                );
            }

            Console.WriteLine();
        }

        /*
         RUN HEURISTIC EXPERIMENT

         Executes the experimental comparison
         of all priority rules on one instance.

         The actual experiment logic is separated into:
         HeuristicExperiment.cs

         This keeps the application layer clean.
        */
        private static void RunHeuristicExperiment(string fileName)
        {
            /*
             Console output:
             which instance is currently used.
            */
            Console.WriteLine("Selected instance:");
            Console.WriteLine(fileName);
            Console.WriteLine();

            /*
             Run complete heuristic experiment.

             Internally this:
             - loads the instance
             - runs all priority rules
             - calculates Cmax
             - compares results
            */
            HeuristicExperiment.Run(fileName);
        }
    }
}