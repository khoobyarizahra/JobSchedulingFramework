using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/* ENUM: INSTANCE TYPES
 Defines different types of instances we want to generate.
 This allows us to test how algorithms behave under different conditions.
*/
public enum InstanceType
{
    Normal,
    LongProcessingTimes,
    SetupHeavy,
    BottleneckMachine,
    MixedRealistic
}

/* CLASS: OPERATION
 Represents ONE operation of a job.
 Each operation has:
 - a machine
 - a processing time
*/
public class Operation
{
    public int Machine;
    public int ProcessingTime;

    public Operation(int machine, int processingTime)
    {
        this.Machine = machine;
        this.ProcessingTime = processingTime;
    }
}

/* CLASS: JOB
 Represents a job, which consists of multiple operations.
*/
public class Job
{
    public int JobId;
    public List<Operation> Operations;

    public Job(int jobId)
    {
        this.JobId = jobId;
        this.Operations = new List<Operation>();
    }
}

/* CLASS: INSTANCE
 Represents the whole scheduling instance.
*/
public class Instance
{
    public int NumberOfJobs;
    public int NumberOfMachines;
    public List<Job> Jobs;
    public int[,] SetupTimes;

    public Instance(int numberOfJobs, int numberOfMachines)
    {
        this.NumberOfJobs = numberOfJobs;
        this.NumberOfMachines = numberOfMachines;
        this.Jobs = new List<Job>();

        // setup[fromJob, toJob]
        this.SetupTimes = new int[numberOfJobs, numberOfJobs];
    }
}

/* CLASS: INSTANCE GENERATOR
 Responsible for creating instances:
 - jobs
 - operations
 - processing times
 - setup times
*/
public class InstanceGenerator
{
    private Random random;
    private InstanceType instanceType;

    // Processing time range
    private int minProcessingTime;
    private int maxProcessingTime;

    /*
     Setup time ratio based on Vinod and Sridharan:
     setup time ratio = mean setup time / mean processing time
     The article uses 20%, 30%, and 40%.
    */
    private double setupTimeRatio;

    public InstanceGenerator(int seed, InstanceType instanceType)
    {
        this.random = new Random(seed);
        this.instanceType = instanceType;

        SetParameters();
    }

    public Instance Generate(int numberOfJobs, int numberOfMachines)
    {
        Instance instance = new Instance(numberOfJobs, numberOfMachines);

        GenerateJobs(instance);
        GenerateSetupTimes(instance);

        return instance;
    }

    /*
     PARAMETER CONFIGURATION

     Processing-time ranges depend on the selected instance type.

     Setup times are generated using one setup-time ratio.
     The article uses setup-time ratios of 20%, 30%, and 40%.
    */
    private void SetParameters()
    {
        if (instanceType == InstanceType.Normal)
        {
            minProcessingTime = 10;
            maxProcessingTime = 100;

            // 20% setup-time ratio
            setupTimeRatio = 0.20;
        }
        else if (instanceType == InstanceType.LongProcessingTimes)
        {
            minProcessingTime = 50;
            maxProcessingTime = 250;

            // 20% setup-time ratio
            setupTimeRatio = 0.20;
        }
        else if (instanceType == InstanceType.SetupHeavy)
        {
            minProcessingTime = 10;
            maxProcessingTime = 100;

            // 40% setup-time ratio
            setupTimeRatio = 0.40;
        }
        else if (instanceType == InstanceType.BottleneckMachine)
        {
            minProcessingTime = 10;
            maxProcessingTime = 100;

            // 30% setup-time ratio
            setupTimeRatio = 0.30;
        }
        else // MixedRealistic
        {
            minProcessingTime = 10;
            maxProcessingTime = 150;

            // 30% setup-time ratio
            setupTimeRatio = 0.30;
        }
    }

    // JOB GENERATION
    private void GenerateJobs(Instance instance)
    {
        int minOperationsPerJob = 2;
        int maxOperationsPerJob = instance.NumberOfMachines;

        for (int jobId = 1; jobId <= instance.NumberOfJobs; jobId++)
        {
            Job job = new Job(jobId);

            int numberOfOperations = random.Next(
                minOperationsPerJob,
                maxOperationsPerJob + 1
            );

            List<int> machines = SelectMachines(
                instance.NumberOfMachines,
                numberOfOperations
            );

            // for each machine we craete an operation with a processing time
            foreach (int machine in machines)
            {
                int processingTime = GenerateProcessingTime(machine);
                job.Operations.Add(new Operation(machine, processingTime));
            }

            instance.Jobs.Add(job);
        }
    }

    // MACHINE SELECTION
    private List<int> SelectMachines(int numberOfMachines, int numberOfOperations)
    {
        List<int> machines = new List<int>();

        if (instanceType == InstanceType.BottleneckMachine)
        {
            // Machine 1 appears in every job to create a bottleneck.
            machines.Add(1);

            List<int> others = Enumerable.Range(2, numberOfMachines - 1)
                .OrderBy(x => random.Next())
                .Take(numberOfOperations - 1)
                .ToList();

            machines.AddRange(others);
        }
        else
        {
            machines = Enumerable.Range(1, numberOfMachines)
                .OrderBy(x => random.Next())
                .Take(numberOfOperations)
                .ToList();
        }

        // Shuffle operation order
        return machines.OrderBy(x => random.Next()).ToList();
    }

    // PROCESSING TIME GENERATION
    private int GenerateProcessingTime(int machine)
    {
        if (instanceType == InstanceType.BottleneckMachine && machine == 1)
        {
            // Machine 1 gets longer processing times.
            return random.Next(maxProcessingTime, maxProcessingTime * 2 + 1);
        }

        if (instanceType == InstanceType.MixedRealistic)
        {
            // 70% normal operations, 30% long operations
            if (random.Next(100) < 70)
            {
                return random.Next(minProcessingTime, maxProcessingTime + 1);
            }

            return random.Next(maxProcessingTime, maxProcessingTime * 2 + 1);
        }

        return random.Next(minProcessingTime, maxProcessingTime + 1);
    }

    // SETUP TIME GENERATION
    private void GenerateSetupTimes(Instance instance)
    {
        // Mean processing time
        double meanProcessingTime = (minProcessingTime + maxProcessingTime) / 2.0;

        // Mean setup time according to setup-time ratio:
        // mean setup time = setupTimeRatio * mean processing time
        int meanSetupTime = Math.Max(1, (int)(setupTimeRatio * meanProcessingTime));

        for (int i = 0; i < instance.NumberOfJobs; i++)
        {
            for (int j = 0; j < instance.NumberOfJobs; j++)
            {
                if (i == j)
                {
                    // No setup from a job to itself.
                    instance.SetupTimes[i, j] = 0;
                }
                else
                {
                    // The article generates individual setup times from an exponential distribution.
                    // To keep this student project simple, we create controlled variation around
                    // the mean setup time: approximately 50% to 150% of the mean.
                    int minSetup = Math.Max(1, (int)(0.5 * meanSetupTime));
                    int maxSetup = Math.Max(minSetup + 1, (int)(1.5 * meanSetupTime));

                    instance.SetupTimes[i, j] = random.Next(minSetup, maxSetup + 1);
                }
            }
        }
    }
}

// CLASS: WRITER
// Writes the instance to file in the required format.
public class InstanceWriter
{
    public void WriteToFile(Instance instance, string fileName)
    {
        using (StreamWriter writer = new StreamWriter(fileName))
        {
            writer.WriteLine("#Meta infos");
            writer.WriteLine($"{instance.NumberOfJobs},{instance.NumberOfMachines}");

            writer.WriteLine("#Processing times");

            foreach (Job job in instance.Jobs)
            {
                string line = job.Operations.Count.ToString();

                foreach (Operation op in job.Operations)
                {
                    line += $",{op.Machine},{op.ProcessingTime}";
                }

                writer.WriteLine(line);
            }

            writer.WriteLine("#Setup times");

            for (int i = 0; i < instance.NumberOfJobs; i++)
            {
                string line = "";

                for (int j = 0; j < instance.NumberOfJobs; j++)
                {
                    line += instance.SetupTimes[i, j];

                    if (j < instance.NumberOfJobs - 1)
                    {
                        line += ",";
                    }
                }

                writer.WriteLine(line);
            }
        }
    }
}

// MAIN PROGRAM
public class Program
{
    public static void Main()
    {
        int jobs = 10;
        int machines = 5;
        int seed = 42;

        InstanceType type = InstanceType.SetupHeavy;

        InstanceGenerator generator = new InstanceGenerator(seed, type);
        Instance instance = generator.Generate(jobs, machines);

        string fileName = $"Instance_{type}.txt";

        InstanceWriter writer = new InstanceWriter();
        writer.WriteToFile(instance, fileName);

        Console.WriteLine("Instance created!");
        Console.WriteLine($"Type: {type}");
        Console.WriteLine($"File: {fileName}");
    }
}