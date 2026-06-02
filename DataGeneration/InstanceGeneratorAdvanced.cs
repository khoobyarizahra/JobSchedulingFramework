using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JobShopSchedulingFramework.DataGeneration
{

    /*
     INSTANCE GENERATOR ADVANCED

     This class creates artificial Job Shop Scheduling instances.

     Important:
     - It does NOT define Job, Operation, or Instance again.
     - It uses the existing model classes from the Models folder.
     - Therefore the project has one shared data structure.
    */
    public class InstanceGeneratorAdvanced
    {
        // Random object for reproducible random numbers.
        private readonly Random random;

        // Defines which type of instance should be generated.
        private readonly InstanceType instanceType;

        // Minimum processing time for generated Operations.
        private int minProcessingTime;

        // Maximum processing time for generated Operations.
        private int maxProcessingTime;

        /*
         Setup time ratio:
         Example:
         setupTimeRatio = 0.20 means that setup times are roughly
         20% of the average processing time.
        */
        private double setupTimeRatio;

        /*
         Constructor.

         seed:
         A fixed seed makes the generated instances reproducible.
         This means: same seed + same parameters = same instance.

         instanceType:
         Controls whether we generate normal, setup-heavy, bottleneck, etc.
         instances.
        */
        public InstanceGeneratorAdvanced(int seed, InstanceType instanceType)
        {
            this.random = new Random(seed);
            this.instanceType = instanceType;

            SetParameters();
        }

        /*
         Generate creates one complete scheduling instance.

         numberOfJobs:
         Number of Jobs in the instance.

         numberOfMachines:
         Number of available machines.

         Returns:
         An Instance object that can be used directly by your heuristic.
        */
        public Instance Generate(int numberOfJobs, int numberOfMachines)
        {
            // Create the existing Instance model from Models/Instance.cs.
            Instance instance = new Instance();

            // Store basic meta information.
            instance.NumJobs = numberOfJobs;
            instance.NumMachines = numberOfMachines;

            // Create setup matrix: SetupTimes[fromJob - 1, toJob - 1].
            instance.SetupTimes = new int[numberOfJobs, numberOfJobs];

            // Generate Jobs and their Operations.
            GenerateJobs(instance);

            // Generate sequence-dependent setup times.
            GenerateSetupTimes(instance);

            return instance;
        }

        /*
         SetParameters defines the processing-time range and setup intensity.

         This keeps the Generate method clean.
        */
        private void SetParameters()
        {
            if (instanceType == InstanceType.Normal)
            {
                minProcessingTime = 10;
                maxProcessingTime = 100;
                setupTimeRatio = 0.20;
            }
            else if (instanceType == InstanceType.LongProcessingTimes)
            {
                minProcessingTime = 50;
                maxProcessingTime = 250;
                setupTimeRatio = 0.20;
            }
            else if (instanceType == InstanceType.SetupHeavy)
            {
                minProcessingTime = 10;
                maxProcessingTime = 100;
                setupTimeRatio = 0.40;
            }
            else if (instanceType == InstanceType.BottleneckMachine)
            {
                minProcessingTime = 10;
                maxProcessingTime = 100;
                setupTimeRatio = 0.30;
            }
            else
            {
                // MixedRealistic
                minProcessingTime = 10;
                maxProcessingTime = 150;
                setupTimeRatio = 0.30;
            }
        }

        /*
         GenerateJobs creates all Jobs of the instance.

         Each job receives a random number of Operations.
         Each Operation has:
         - JobID
         - OperationID
         - Machine
         - ProcessingTime
        */
        private void GenerateJobs(Instance instance)
        {
            // In a job shop, a job can have fewer Operations than machines.
            int minOperationsPerJob = 2;

            // A job should not use more machines than available.
            int maxOperationsPerJob = instance.NumMachines;

            // Create Jobs with IDs 1, 2, ..., NumJobs.
            for (int jobID = 1; jobID <= instance.NumJobs; jobID++)
            {
                // Use existing Job class from Models/Job.cs.
                Job job = new Job(jobID);

                // Random number of Operations for this job.
                int numberOfOperations = random.Next(
                    minOperationsPerJob,
                    maxOperationsPerJob + 1
                );

                // Select machines for the Operations.
                List<int> machines = SelectMachines(
                    instance.NumMachines,
                    numberOfOperations
                );

                // OperationID starts at 1 because your project uses 1-based IDs.
                int operationID = 1;

                foreach (int machine in machines)
                {
                    // Generate processing time depending on the instance type.
                    int processingTime = GenerateProcessingTime(machine);

                    // Use existing Operation class from Models/Operation.cs.
                    Operation operation = new Operation(
                        jobID,
                        operationID,
                        machine,
                        processingTime
                    );

                    // Add Operation to the job.
                    job.Operations.Add(operation);

                    operationID++;
                }

                // Add complete job to the instance.
                instance.Jobs.Add(job);
            }
        }

        /*
         SelectMachines chooses the machines used by one job.

         Important:
         - One Machine should not appear twice in the same job.
         - The order is randomly shuffled because Operation order matters.
        */
        private List<int> SelectMachines(int numberOfMachines, int numberOfOperations)
        {
            List<int> machines;

            if (instanceType == InstanceType.BottleneckMachine)
            {
                // Machine 1 appears in every job.
                // This creates a bottleneck and makes the instance harder.
                machines = new List<int> { 1 };

                // Select remaining machines from 2, 3, ..., numberOfMachines.
                List<int> otherMachines = Enumerable.Range(2, numberOfMachines - 1)
                    .OrderBy(x => random.Next())
                    .Take(numberOfOperations - 1)
                    .ToList();

                machines.AddRange(otherMachines);
            }
            else
            {
                // Select random machines without repetition.
                machines = Enumerable.Range(1, numberOfMachines)
                    .OrderBy(x => random.Next())
                    .Take(numberOfOperations)
                    .ToList();
            }

            // Shuffle final Machine order.
            // This creates the technological order of the job.
            return machines.OrderBy(x => random.Next()).ToList();
        }

        /*
         GenerateProcessingTime creates the duration of one Operation.

         For bottleneck instances, Machine 1 receives longer processing times.
         For mixed realistic instances, some Operations are much longer.
        */
        private int GenerateProcessingTime(int machine)
        {
            if (instanceType == InstanceType.BottleneckMachine && machine == 1)
            {
                // Longer processing times on the bottleneck Machine.
                return random.Next(maxProcessingTime, maxProcessingTime * 2 + 1);
            }

            if (instanceType == InstanceType.MixedRealistic)
            {
                // 70% normal Operations.
                if (random.Next(100) < 70)
                {
                    return random.Next(minProcessingTime, maxProcessingTime + 1);
                }

                // 30% long Operations.
                return random.Next(maxProcessingTime, maxProcessingTime * 2 + 1);
            }

            // Standard case.
            return random.Next(minProcessingTime, maxProcessingTime + 1);
        }

        /*
         GenerateSetupTimes creates sequence-dependent setup times.

         SetupTimes[i, j] means:
         setup time from job i+1 to job j+1.

         Example:
         SetupTimes[0, 2] = setup time from job 1 to job 3.
        */
        private void GenerateSetupTimes(Instance instance)
        {
            // Approximate average processing time.
            double meanProcessingTime = (minProcessingTime + maxProcessingTime) / 2.0;

            // Average setup time based on the selected setup ratio.
            int meanSetupTime = Math.Max(
                1,
                (int)(setupTimeRatio * meanProcessingTime)
            );

            // Lower and upper setup-time bounds.
            int minSetup = Math.Max(1, (int)(0.5 * meanSetupTime));
            int maxSetup = Math.Max(minSetup + 1, (int)(1.5 * meanSetupTime));

            // Fill setup matrix.
            for (int fromJob = 0; fromJob < instance.NumJobs; fromJob++)
            {
                for (int toJob = 0; toJob < instance.NumJobs; toJob++)
                {
                    if (fromJob == toJob)
                    {
                        // No setup from a job to itself.
                        instance.SetupTimes[fromJob, toJob] = 0;
                    }
                    else
                    {
                        // Random setup time between two different Jobs.
                        instance.SetupTimes[fromJob, toJob] =
                            random.Next(minSetup, maxSetup + 1);
                    }
                }
            }
        }
    }
}