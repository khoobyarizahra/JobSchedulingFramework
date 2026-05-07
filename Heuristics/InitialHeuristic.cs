using Project_Scheduling_ZahraAndCarolin.Models;
using System;
using System.Collections.Generic;
using System.Text;

/*
INITIAL HEURISTIC (Giffler-Thompson)
Kern des Algorithmus:
- konstruiert einen aktiven Schedule
- nutzt Prioritätsregeln zur Konfliktauflösung
*/

namespace Project_Scheduling_ZahraAndCarolin.Heuristics
{

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
    }
}
