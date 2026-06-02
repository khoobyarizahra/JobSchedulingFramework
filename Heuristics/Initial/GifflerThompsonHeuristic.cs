using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;
using System.Text;

/*
INITIAL HEURISTIC (Giffler-Thompson)
Kern des Algorithmus:
- konstruiert einen aktiven Schedule
- nutzt Prioritätsregeln zur Konfliktauflösung
*/

namespace JobShopSchedulingFramework.Heuristics.Initial
{

    public class GifflerThompsonHeuristic
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
            foreach (Job job in instance.Jobs)
            {
                int sum = 0;

                for (int i = job.Operations.Count - 1; i >= 0; i--)
                {
                    sum += job.Operations[i].ProcessingTime;
                    job.Operations[i].remainingProcessingTime = sum;
                }
            }
        }

        //Setup-Zeit bestimmen: abhängig vom vorherigen Job auf dieser Maschine
        private static int GetSetupTime(Instance instance, int[] lastJobOnMachine, Operation op)
        {
            int previousJob = lastJobOnMachine[op.Machine];

            if (previousJob == 0)
                return 0;

            return instance.SetupTimes[previousJob - 1, op.JobID - 1];
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
                return conflictSet.OrderByDescending(op => op.ProcessingTime).First();
            }

            if (rule == PriorityRule.SPT)
            {
                return conflictSet.OrderBy(op => op.ProcessingTime).First();
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
            int[] nextOperationReadyTime = new int[instance.NumJobs + 1];
            int[] machineReadyTime = new int[instance.NumMachines + 1];
            int[] lastJobOnMachine = new int[instance.NumMachines + 1];
            int[] nextOperation = new int[instance.NumJobs + 1];

            for (int j = 1; j <= instance.NumJobs; j++)
                nextOperation[j] = 1;

            int totalOperations = instance.Jobs.Sum(job => job.Operations.Count);
            int scheduledOperations = 0;

            while (scheduledOperations < totalOperations)
            {
                int cStar = int.MaxValue;
                int selectedMachine = -1;

                /*
                  Schritt 1:
                  Finde früheste Fertigstellungszeit C*
                 */
                foreach (Job job in instance.Jobs)
                {
                    if (nextOperation[job.JobID] <= job.Operations.Count)
                    {
                        Operation op = job.Operations[nextOperation[job.JobID] - 1];

                        int setup = GetSetupTime(instance, lastJobOnMachine, op);

                        int start = Math.Max(
                            nextOperationReadyTime[op.JobID],
                            machineReadyTime[op.Machine] + setup
                        );

                        int completion = start + op.ProcessingTime;

                        if (completion < cStar)
                        {
                            cStar = completion;
                            selectedMachine = op.Machine;
                        }
                    }
                }

                /*
                 Schritt 2:
                 Konfliktmenge bilden
                 */
                List<Operation> conflictSet = new List<Operation>();

                foreach (Job job in instance.Jobs)
                {
                    if (nextOperation[job.JobID] <= job.Operations.Count)
                    {
                        Operation op = job.Operations[nextOperation[job.JobID] - 1];

                        int setup = GetSetupTime(instance, lastJobOnMachine, op);

                        int start = Math.Max(
                            nextOperationReadyTime[op.JobID],
                            machineReadyTime[op.Machine] + setup
                        );

                        if (op.Machine == selectedMachine && start < cStar)
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
                    nextOperationReadyTime[selected.JobID],
                    machineReadyTime[selected.Machine] + setupSelected
                );

                int endTime = startTime + selected.ProcessingTime;

                selected.StartTime = startTime;
                selected.EndTime = endTime;

                nextOperationReadyTime[selected.JobID] = endTime;
                machineReadyTime[selected.Machine] = endTime;
                lastJobOnMachine[selected.Machine] = selected.JobID;

                nextOperation[selected.JobID]++;
                scheduledOperations++;
            }
        }
    }
}
