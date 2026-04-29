// Operation class: represents one processing step of a job
public class Operation
{
    public int jobID;
    public int operationID;
    public int machine;
    public int processingTime;

 // scheduling results
    public int startTime;
    public int endTime;

    //for Giffler & Thompson  
    public int ltt;

    public Operation(int jobID, int operationID, int machine, int processingTime)
    {
        this.jobID = jobID;
        this.operationID = operationID;
        this.machine = machine;
        this.processingTime = processingTime;
    }
}

// Job class: contains all operations of one job
public class Job
{
    public int jobID;
    public List<Operation> operations;

    public Job(int jobID)
    {
        this.jobID = jobID;
        this.operations = new List<Operation>();
    }
}

// Instance class: represents the whole scheduling problem
public class Instance
{
    public int numJobs;
    public int numMachines;
    public List<Job> jobs;
    public int[,] setupTimes;

    public Instance()
    {
        this.jobs = new List<Job>();
    }
}

public class InstanceReader
{
    public static Instance ReadFromFile(string fileName)
    {
        // read all lines of the file
        string[] lines = File.ReadAllLines(fileName);

        Instance instance = new Instance();

        int line = 0;

        // skip "#Meta infos"
        line++;

        // read number of jobs and machines
        string[] meta = lines[line].Split(',');
        instance.numJobs = int.Parse(meta[0]);
        instance.numMachines = int.Parse(meta[1]);
        line++;

        // skip "#Processing times"
        line++;

        // read jobs and their operations
        for (int jobID = 1; jobID <= instance.numJobs; jobID++)
        {
            string[] values = lines[line].Split(',');

            Job job = new Job(jobID);

            // first value = number of operations
            int numberOfOperations = int.Parse(values[0]);

            // index to move through machine/time pairs
            int valueIndex = 1;

            // read all operations of this job
            for (int opID = 1; opID <= numberOfOperations; opID++)
            {
                int machine = int.Parse(values[valueIndex]);
                int time = int.Parse(values[valueIndex + 1]);

                Operation op = new Operation(jobID, opID, machine, time);

                job.operations.Add(op);

                valueIndex += 2; // move to next (machine, time) pair
            }

            instance.jobs.Add(job);
            line++;
        }

        // skip "#Setup times"
        line++;

        // initialize setup time matrix
        instance.setupTimes = new int[instance.numJobs, instance.numJobs];

        // read setup times as 2D matrix
        for (int i = 0; i < instance.numJobs; i++)
        {
            string[] row = lines[line].Split(',');

            for (int j = 0; j < instance.numJobs; j++)
            {
                instance.setupTimes[i, j] = int.Parse(row[j]);
            }

            line++;
        }

        return instance;
    }
}

//initialheuristic first without setup Times
public class InitialHeuristic
{
    // Step 1: calculate LTT values for all operations
    public static void CalculateLTTValues(Instance instance)
    {
        foreach (Job job in instance.jobs)
        {
            int sum = 0;

            // go backwards through the operations of one job
            for (int i = job.operations.Count - 1; i >= 0; i--)
            {
                sum += job.operations[i].processingTime;

                // LTT = processing time of this operation
                //       + processing times of all following operations
                job.operations[i].ltt = sum;
            }
        }
    }

    // Step 2: create an initial schedule including setup times
    public static void CreateInitialSchedule(Instance instance)
    {
        // Similar to r_ij in the exercise:
        // stores when the next operation of each job is ready
        int[] nextOperationReadyTime = new int[instance.numJobs + 1];

        // stores when each machine is available again
        int[] machineReadyTime = new int[instance.numMachines + 1];

        // stores which job was processed last on each machine
        // needed to calculate setup times
        int[] lastJobOnMachine = new int[instance.numMachines + 1];

        // stores which operation is next for each job
        int[] nextOperation = new int[instance.numJobs + 1];

        // at the beginning, the first operation of each job is ready
        for (int jobID = 1; jobID <= instance.numJobs; jobID++)
        {
            nextOperation[jobID] = 1;
        }

        // count all operations
        int totalOperations = 0;

        foreach (Job job in instance.jobs)
        {
            totalOperations += job.operations.Count;
        }

        int scheduledOperations = 0;

        // repeat until all operations are scheduled
        while (scheduledOperations < totalOperations)
        {
            Operation bestOperation = null;
            int bestStartTime = 0;
            int bestEndTime = int.MaxValue;

            // check the next possible operation of every job
            foreach (Job job in instance.jobs)
            {
                if (nextOperation[job.jobID] <= job.operations.Count)
                {
                    Operation currentOperation = job.operations[nextOperation[job.jobID] - 1];

                    int setupTime = 0;

                    // find the job that was processed last on this machine
                    int previousJob = lastJobOnMachine[currentOperation.machine];

                    // no initial setup time is needed if the machine is still unused
                    if (previousJob != 0)
                    {
                        setupTime = instance.setupTimes[previousJob - 1, currentOperation.jobID - 1];
                    }

                    // earliest start time:
                    // operation must wait until:
                    // 1. the job is ready
                    // 2. the machine is free
                    // 3. the setup time on this machine is finished
                    int startTime = Math.Max(
                        nextOperationReadyTime[currentOperation.jobID],
                        machineReadyTime[currentOperation.machine] + setupTime
                    );

                    int endTime = startTime + currentOperation.processingTime;

                    // rule 1: choose operation with earliest end time
                    if (endTime < bestEndTime)
                    {
                        bestOperation = currentOperation;
                        bestStartTime = startTime;
                        bestEndTime = endTime;
                    }
                    else if (endTime == bestEndTime)
                    {
                        // rule 2: if equal, choose larger LTT
                        if (currentOperation.ltt > bestOperation.ltt)
                        {
                            bestOperation = currentOperation;
                            bestStartTime = startTime;
                            bestEndTime = endTime;
                        }
                        // rule 3: if still equal, choose larger processing time
                        else if (currentOperation.ltt == bestOperation.ltt &&
                                 currentOperation.processingTime > bestOperation.processingTime)
                        {
                            bestOperation = currentOperation;
                            bestStartTime = startTime;
                            bestEndTime = endTime;
                        }
                    }
                }
            }

            // save start and end time of selected operation
            bestOperation.startTime = bestStartTime;
            bestOperation.endTime = bestEndTime;

            // update readiness time for the next operation of this job
            nextOperationReadyTime[bestOperation.jobID] = bestEndTime;

            // update machine availability
            machineReadyTime[bestOperation.machine] = bestEndTime;

            // remember which job was last processed on this machine
            lastJobOnMachine[bestOperation.machine] = bestOperation.jobID;

            // move to the next operation of this job
            nextOperation[bestOperation.jobID]++;

            scheduledOperations++;
        }
    }
}
public class Program
{
    public static void Main(string[] args)
    {
        // 1. Read instance from file
        Instance instance = InstanceReader.ReadFromFile("ExampleInstanceSmall.txt");

        // 2. Calculate LTT values
        InitialHeuristic.CalculateLTTValues(instance);

        // 3. Create initial schedule
        InitialHeuristic.CreateInitialSchedule(instance);

        // 4. Print schedule
        Console.WriteLine("INITIAL SCHEDULE:");
        Console.WriteLine();

        foreach (Job job in instance.jobs)
        {
            Console.WriteLine("Job " + job.jobID);

            foreach (Operation op in job.operations)
            {
                Console.WriteLine(
                    "  Operation " + op.operationID +
                    " | Job " + op.jobID +
                    " | Machine " + op.machine +
                    " | Start " + op.startTime +
                    " | End " + op.endTime +
                    " | ProcTime " + op.processingTime +
                    " | LTT " + op.ltt
                );
            }

            Console.WriteLine();
        }
    }
}