using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JobShopSchedulingFramework.Heuristics.Initial
{
    /*
    Erstellt eine zulässige Startlösung mit der Giffler-Thompson-Heuristik.
    Die Auswahl innerhalb einer Konfliktmenge erfolgt über eine Prioritätsregel.
    */
    public class GifflerThompsonHeuristic
    {
        // Fester Seed für reproduzierbare Zufallsergebnisse.
        private static Random random = new Random(42);

        public static void SetRandomSeed(int seed)
        {
            random = new Random(seed);
        }

        /*
        Berechnet für jede Operation die verbleibende Bearbeitungszeit bis zum Jobende.
        Dieser Wert wird von LRPT, SRPT und SetupAwareLRPT verwendet.
        */
        public static void CalculateRemainingProcessingTimes(Instance instance)
        {
            foreach (Job job in instance.Jobs)
            {
                int sum = 0;

                for (int i = job.Operations.Count - 1; i >= 0; i--)
                {
                    sum += job.Operations[i].ProcessingTime;
                    job.Operations[i].RemainingProcessingTime = sum;
                }
            }
        }

        private static int GetSetupTime(
            Instance instance,
            int[] lastJobOnMachine,
            Operation op)
        {
            int previousJob = lastJobOnMachine[op.Machine];

            if (previousJob == 0)
                return 0;

            return instance.SetupTimes[previousJob - 1, op.JobID - 1];
        }

        /*
        Wählt aus der Konfliktmenge die nächste Operation entsprechend der
        gewählten Prioritätsregel aus.
        */
        private static Operation SelectByPriorityRule(
            List<Operation> conflictSet,
            PriorityRule rule,
            Instance instance,
            int[] lastJobOnMachine)
        {
            if (rule == PriorityRule.LRPT)
            {
                return conflictSet
                    .OrderByDescending(op => op.RemainingProcessingTime)
                    .First();
            }

            if (rule == PriorityRule.LPT)
            {
                return conflictSet
                    .OrderByDescending(op => op.ProcessingTime)
                    .First();
            }

            if (rule == PriorityRule.SPT)
            {
                return conflictSet
                    .OrderBy(op => op.ProcessingTime)
                    .First();
            }

            if (rule == PriorityRule.SRPT)
            {
                return conflictSet
                    .OrderBy(op => op.RemainingProcessingTime)
                    .First();
            }

            if (rule == PriorityRule.SetupAwareLRPT)
            {
                return conflictSet
                    .OrderByDescending(op =>
                    {
                        int setup = GetSetupTime(instance, lastJobOnMachine, op);
                        return op.RemainingProcessingTime - setup;
                    })
                    .First();
            }

            int index = random.Next(conflictSet.Count);
            return conflictSet[index];
        }
        /*
        Erzeugt mit der Giffler-Thompson-Heuristik einen zulässigen Startplan.
        Die angegebene Prioritätsregel entscheidet bei Maschinenkonflikten,
        welche Operation als nächste eingeplant wird.
        */
        public static void CreateInitialSchedule(Instance instance, PriorityRule rule)
        {
            CalculateRemainingProcessingTimes(instance);

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
                Bestimmt die früheste mögliche Fertigstellungszeit aller aktuell
                verfügbaren Operationen.
                */
                foreach (Job job in instance.Jobs)
                {
                    if (nextOperation[job.JobID] <= job.Operations.Count)
                    {
                        Operation op = job.Operations[nextOperation[job.JobID] - 1];

                        int setup = GetSetupTime(instance, lastJobOnMachine, op);

                        int start = Math.Max(
                            nextOperationReadyTime[op.JobID],
                            machineReadyTime[op.Machine] + setup);

                        int completion = start + op.ProcessingTime;

                        if (completion < cStar)
                        {
                            cStar = completion;
                            selectedMachine = op.Machine;
                        }
                    }
                }

                /*
                Bildet die Konfliktmenge auf der ausgewählten Maschine.
                Betrachtet werden Operationen, die vor cStar starten könnten.
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
                            machineReadyTime[op.Machine] + setup);

                        if (op.Machine == selectedMachine && start < cStar)
                            conflictSet.Add(op);
                    }
                }

                Operation selected = SelectByPriorityRule(
                    conflictSet,
                    rule,
                    instance,
                    lastJobOnMachine);

                int setupSelected = GetSetupTime(
                    instance,
                    lastJobOnMachine,
                    selected);

                int startTime = Math.Max(
                    nextOperationReadyTime[selected.JobID],
                    machineReadyTime[selected.Machine] + setupSelected);

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