using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JobShopSchedulingFramework.DataGeneration
{
   
    public class TestInstanceGenerator
    {
        private readonly Random random;
        private readonly TestInstanceType type;

        private const int MinProcessingTime = 10;
        private const int MaxProcessingTime = 100;

        private const int StandardMinSetup = 10;
        private const int StandardMaxSetup = 50;

        public TestInstanceGenerator(
        int seed,
        TestInstanceType type)
        {
            this.random = new Random(seed);
            this.type = type;
        }
        public Instance Generate(int numberOfJobs, int numberOfMachines)
        {
            ValidateInputParameters(numberOfJobs, numberOfMachines);

            Instance instance = new Instance
            {
                NumJobs = numberOfJobs,
                NumMachines = numberOfMachines,
                SetupTimes = new int[numberOfJobs, numberOfJobs]
            };

            GenerateJobs(instance);
            GenerateSetupTimes(instance);
            ValidateInstance(instance);

            return instance;
        }

        public void GenerateAndSave(
        int numberOfJobs,
        int numberOfMachines,
        string outputPath)
        {
            string? directory =
                Path.GetDirectoryName(outputPath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Instance instance =
                Generate(numberOfJobs, numberOfMachines);

            SaveInstance(instance, outputPath);
        }
        private void ValidateInputParameters(
        int numberOfJobs,
        int numberOfMachines)
        {
            if (numberOfJobs < 2)
            {
                throw new ArgumentException(
                    "An instance must contain at least 2 jobs.");
            }

            if (numberOfMachines < 2)
            {
                throw new ArgumentException(
                    "An instance must contain at least 2 machines.");
            }
        }

        private void GenerateJobs(Instance instance)
        {
            for (int jobId = 1; jobId <= instance.NumJobs; jobId++)
            {
                Job job = new Job(jobId);

                int operationCount = DetermineOperationCount(instance.NumMachines);

                List<int> machineOrder = SelectMachineOrder(
                    instance.NumMachines,
                    operationCount);

                for (int operationId = 1; operationId <= operationCount; operationId++)
                {
                    int machine = machineOrder[operationId - 1];
                    int processingTime = GenerateProcessingTime(machine);

                    Operation operation = new Operation(
                        jobId,
                        operationId,
                        machine,
                        processingTime);

                    job.Operations.Add(operation);
                }

                instance.Jobs.Add(job);
            }
        }

        private int DetermineOperationCount(int numberOfMachines)
        {
            if (type == TestInstanceType.Full)
            {
                return numberOfMachines;
            }

            if (type == TestInstanceType.Partial)
            {
                int minOperations =
                    Math.Max(3, numberOfMachines / 2);

                return random.Next(
                    minOperations,
                    numberOfMachines + 1);
            }

            throw new InvalidOperationException("Unknown test instance type.");
        }

        private List<int> SelectMachineOrder(
    int numberOfMachines,
    int operationCount)
        {
            List<int> selectedMachines =
                Enumerable
                    .Range(1, numberOfMachines)
                    .OrderBy(_ => random.Next())
                    .Take(operationCount)
                    .ToList();

            return selectedMachines;
        }

        private int GenerateProcessingTime(int machine)
        {
            return random.Next(
                MinProcessingTime,
                MaxProcessingTime + 1);
        }

        private void GenerateSetupTimes(Instance instance)
        {
            for (int fromJob = 0; fromJob < instance.NumJobs; fromJob++)
            {
                for (int toJob = 0; toJob < instance.NumJobs; toJob++)
                {
                    if (fromJob == toJob)
                    {
                        instance.SetupTimes[fromJob, toJob] = 0;
                    }
                    else
                    {
                        instance.SetupTimes[fromJob, toJob] =
                            random.Next(
                                StandardMinSetup,
                                StandardMaxSetup + 1);
                    }
                }
            }
        }

        private void ValidateInstance(Instance instance)
        {
            if (instance.NumJobs <= 0)
                throw new InvalidOperationException("Invalid instance: number of jobs must be positive.");

            if (instance.NumMachines <= 0)
                throw new InvalidOperationException("Invalid instance: number of machines must be positive.");

            if (instance.Jobs.Count != instance.NumJobs)
                throw new InvalidOperationException("Invalid instance: job count does not match NumJobs.");

            if (instance.SetupTimes.GetLength(0) != instance.NumJobs ||
                instance.SetupTimes.GetLength(1) != instance.NumJobs)
            {
                throw new InvalidOperationException("Invalid instance: setup matrix has wrong dimensions.");
            }

            for (int i = 0; i < instance.NumJobs; i++)
            {
                if (instance.SetupTimes[i, i] != 0)
                    throw new InvalidOperationException("Invalid instance: setup diagonal must be zero.");
            }

            foreach (Job job in instance.Jobs)
            {
                if (job.Operations.Count == 0)
                    throw new InvalidOperationException($"Invalid job {job.JobID}: job has no operations.");

                HashSet<int> usedMachines = new HashSet<int>();

                for (int index = 0; index < job.Operations.Count; index++)
                {
                    Operation operation = job.Operations[index];

                    if (operation.JobID != job.JobID)
                        throw new InvalidOperationException($"Invalid operation in job {job.JobID}: wrong JobID.");

                    if (operation.OperationID != index + 1)
                        throw new InvalidOperationException($"Invalid job {job.JobID}: OperationIDs must be consecutive.");

                    if (operation.Machine < 1 || operation.Machine > instance.NumMachines)
                        throw new InvalidOperationException($"Invalid operation: machine {operation.Machine} is outside valid range.");

                    if (operation.ProcessingTime <= 0)
                        throw new InvalidOperationException("Invalid operation: processing time must be positive.");

                    if (!usedMachines.Add(operation.Machine))
                        throw new InvalidOperationException($"Invalid job {job.JobID}: machine {operation.Machine} appears more than once.");
                }
            }
        }

        private void SaveInstance(Instance instance, string outputPath)
        {
            using StreamWriter writer = new StreamWriter(outputPath);

            writer.WriteLine("#Meta infos");
            writer.WriteLine($"{instance.NumJobs},{instance.NumMachines}");

            writer.WriteLine("#Processing times");

            foreach (Job job in instance.Jobs)
            {
                List<string> values = new List<string>
                {
                    job.Operations.Count.ToString()
                };

                foreach (Operation operation in job.Operations)
                {
                    values.Add(operation.Machine.ToString());
                    values.Add(operation.ProcessingTime.ToString());
                }

                writer.WriteLine(string.Join(",", values));
            }

            writer.WriteLine("#Setup times");

            for (int i = 0; i < instance.NumJobs; i++)
            {
                List<string> row = new List<string>();

                for (int j = 0; j < instance.NumJobs; j++)
                {
                    row.Add(instance.SetupTimes[i, j].ToString());
                }

                writer.WriteLine(string.Join(",", row));
            }
        }
    }
}
