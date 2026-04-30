using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/*
 PRIORITY RULES
 Hier definieren wir alle Prioritätsregeln, die wir vergleichen wollen.
 Vorteil von enum:
 - typsicher
 - keine Tippfehler (kein String!)
 - einfach iterierbar im Experiment
 */
public enum PriorityRule
{
    LRPT,              // Longest Remaining Processing Time (das beste für unsere Ziefunktion laut dem Artikel)
    LPT,               // Longest Processing Time
    SPT,               // Shortest Processing Time
    SRPT,              // Shortest Remaining Processing Time
    Random,            // Zufällige Auswahl (mit Seed reproduzierbar)
    SetupAwareLRPT     // Eigene Erweiterung: berücksichtigt Setup Times
}


  //Eine Operation gehört zu genau einem Job und wird auf genau einer Maschine ausgeführt.

public class Operation
{
    public int jobID;
    public int operationID;
    public int machine;
    public int processingTime;

    // Ergebnisse des Schedulings
    public int startTime;
    public int endTime;

    // Für LRPT / SRPT
    public int remainingProcessingTime;

    public Operation(int jobID, int operationID, int machine, int processingTime)
    {
        this.jobID = jobID;
        this.operationID = operationID;
        this.machine = machine;
        this.processingTime = processingTime;
    }
}


 //Ein Job besteht aus einer festen Reihenfolge von Operationen.

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

/*
 INSTANCE Repräsentiert das gesamte Scheduling-Problem:
  - Anzahl Jobs
  - Anzahl Maschinen
  - alle Jobs
  - Setup-Zeiten zwischen Jobs
 */
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

//INSTANCE READER Liest die Instanz aus einer Datei ein.
 
public class InstanceReader
{
    public static Instance ReadFromFile(string fileName)
    {
        string[] lines = File.ReadAllLines(fileName);
        Instance instance = new Instance();

        int line = 0;

        line++; // Skip "#Meta infos"

        // Anzahl Jobs und Maschinen
        string[] meta = lines[line].Split(',');
        instance.numJobs = int.Parse(meta[0]);
        instance.numMachines = int.Parse(meta[1]);
        line++;

        line++; // Skip "#Processing times"

        // Jobs und Operationen einlesen
        for (int jobID = 1; jobID <= instance.numJobs; jobID++)
        {
            string[] values = lines[line].Split(',');
            Job job = new Job(jobID);

            int numberOfOperations = int.Parse(values[0]);
            int valueIndex = 1;

            for (int opID = 1; opID <= numberOfOperations; opID++)
            {
                int machine = int.Parse(values[valueIndex]);
                int processingTime = int.Parse(values[valueIndex + 1]);

                job.operations.Add(new Operation(jobID, opID, machine, processingTime));

                valueIndex += 2;
            }

            instance.jobs.Add(job);
            line++;
        }

        line++; // Skip "#Setup times"

        instance.setupTimes = new int[instance.numJobs, instance.numJobs];

        // Setup-Matrix einlesen
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

/*
 INITIAL HEURISTIC (Giffler-Thompson)
 Kern des Algorithmus:
  - konstruiert einen aktiven Schedule
  - nutzt Prioritätsregeln zur Konfliktauflösung
 */
public class InitialHeuristic
{
    // Random mit Seed → reproduzierbare Ergebnisse!
    private static Random random = new Random(42);

    public static void SetRandomSeed(int seed)
    {
        random = new Random(seed);
    }

    
     //Berechnet Remaining Processing Time (LRPT)

    public static void CalculateRemainingProcessingTimes(Instance instance)
    {
        foreach (Job job in instance.jobs)
        {
            int sum = 0;

            for (int i = job.operations.Count - 1; i >= 0; i--)
            {
                sum += job.operations[i].processingTime;
                job.operations[i].remainingProcessingTime = sum;
            }
        }
    }

    //Setup-Zeit bestimmen: abhängig vom vorherigen Job auf dieser Maschine
    private static int GetSetupTime(Instance instance, int[] lastJobOnMachine, Operation op)
    {
        int previousJob = lastJobOnMachine[op.machine];

        if (previousJob == 0)
            return 0;

        return instance.setupTimes[previousJob - 1, op.jobID - 1];
    }

    /*
     PRIORITY RULE SELECTION
     Entscheidet, welche Operation aus der Konfliktmenge gewählt wird.
     */
    private static Operation SelectByPriorityRule(
        List<Operation> conflictSet,
        PriorityRule rule,
        Instance instance,
        int[] lastJobOnMachine)
    {
        if (rule == PriorityRule.LRPT)
        {
            return conflictSet.OrderByDescending(op => op.remainingProcessingTime).First();
        }

        if (rule == PriorityRule.LPT)
        {
            return conflictSet.OrderByDescending(op => op.processingTime).First();
        }

        if (rule == PriorityRule.SPT)
        {
            return conflictSet.OrderBy(op => op.processingTime).First();
        }

        if (rule == PriorityRule.SRPT)
        {
            return conflictSet.OrderBy(op => op.remainingProcessingTime).First();
        }

        /*
         Setup-aware Erweiterung:
         Berücksichtigt Rüstzeit direkt in der Entscheidung
         */
        if (rule == PriorityRule.SetupAwareLRPT)
        {
            return conflictSet
                .OrderByDescending(op =>
                {
                    int setup = GetSetupTime(instance, lastJobOnMachine, op);
                    return op.remainingProcessingTime - setup;
                })
                .First();
        }

        // Random Auswahl (Seed sorgt für Reproduzierbarkeit)
        int index = random.Next(conflictSet.Count);
        return conflictSet[index];
    }

    //Giffler-Thompson Algorithmus
    public static void CreateInitialSchedule(Instance instance, PriorityRule rule)
    {
        int[] nextOperationReadyTime = new int[instance.numJobs + 1];
        int[] machineReadyTime = new int[instance.numMachines + 1];
        int[] lastJobOnMachine = new int[instance.numMachines + 1];
        int[] nextOperation = new int[instance.numJobs + 1];

        for (int j = 1; j <= instance.numJobs; j++)
            nextOperation[j] = 1;

        int totalOperations = instance.jobs.Sum(job => job.operations.Count);
        int scheduledOperations = 0;

        while (scheduledOperations < totalOperations)
        {
            int cStar = int.MaxValue;
            int selectedMachine = -1;

            /*
              Schritt 1:
              Finde früheste Fertigstellungszeit C*
             */
            foreach (Job job in instance.jobs)
            {
                if (nextOperation[job.jobID] <= job.operations.Count)
                {
                    Operation op = job.operations[nextOperation[job.jobID] - 1];

                    int setup = GetSetupTime(instance, lastJobOnMachine, op);

                    int start = Math.Max(
                        nextOperationReadyTime[op.jobID],
                        machineReadyTime[op.machine] + setup
                    );

                    int completion = start + op.processingTime;

                    if (completion < cStar)
                    {
                        cStar = completion;
                        selectedMachine = op.machine;
                    }
                }
            }

            /*
             Schritt 2:
             Konfliktmenge bilden
             */
            List<Operation> conflictSet = new List<Operation>();

            foreach (Job job in instance.jobs)
            {
                if (nextOperation[job.jobID] <= job.operations.Count)
                {
                    Operation op = job.operations[nextOperation[job.jobID] - 1];

                    int setup = GetSetupTime(instance, lastJobOnMachine, op);

                    int start = Math.Max(
                        nextOperationReadyTime[op.jobID],
                        machineReadyTime[op.machine] + setup
                    );

                    if (op.machine == selectedMachine && start < cStar)
                        conflictSet.Add(op);
                }
            }

            /*
             Schritt 3:
             Prioritätsregel anwenden
             */
            Operation selected = SelectByPriorityRule(
                conflictSet, rule, instance, lastJobOnMachine);

            int setupSelected = GetSetupTime(instance, lastJobOnMachine, selected);

            int startTime = Math.Max(
                nextOperationReadyTime[selected.jobID],
                machineReadyTime[selected.machine] + setupSelected
            );

            int endTime = startTime + selected.processingTime;

            selected.startTime = startTime;
            selected.endTime = endTime;

            nextOperationReadyTime[selected.jobID] = endTime;
            machineReadyTime[selected.machine] = endTime;
            lastJobOnMachine[selected.machine] = selected.jobID;

            nextOperation[selected.jobID]++;
            scheduledOperations++;
        }
    }

    public static int CalculateCmax(Instance instance)
    {
        return instance.jobs
            .SelectMany(j => j.operations)
            .Max(op => op.endTime);
    }
}

/*
 * MAIN
 Führt experimentellen Vergleich aller Regeln durch.
 */
public class Program
{
    public static void Main(string[] args)
    {
        string fileName = "ExampleInstanceSmall.txt";

        int bestCmax = int.MaxValue;
        PriorityRule bestRule = PriorityRule.LRPT;

        Console.WriteLine("EXPERIMENTAL COMPARISON\n");

        foreach (PriorityRule rule in Enum.GetValues(typeof(PriorityRule)))
        {
            Instance instance = InstanceReader.ReadFromFile(fileName);

            InitialHeuristic.SetRandomSeed(42);
            InitialHeuristic.CalculateRemainingProcessingTimes(instance);
            InitialHeuristic.CreateInitialSchedule(instance, rule);

            int cmax = InitialHeuristic.CalculateCmax(instance);

            Console.WriteLine(rule + " -> Cmax = " + cmax);

            if (cmax < bestCmax)
            {
                bestCmax = cmax;
                bestRule = rule;
            }
        }

        Console.WriteLine("\nBest rule: " + bestRule);
        Console.WriteLine("Best Cmax: " + bestCmax);
    }
}