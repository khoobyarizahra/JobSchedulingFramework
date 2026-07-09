using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality
{
    /*
    Analysiert einen aktuellen Schedule und bestimmt die kritischen Operationen.

    Eine Operation gilt als kritisch, wenn ihr frühester Start r_i zusammen mit
    dem längsten Restpfad q_i genau den aktuellen Makespan Cmax ergibt.
    Diese Operationen bilden die Grundlage für kritische Blöcke und gezielte
    Nachbarschaften in der Tabu Search.
    */
    public static class CriticalOperationAnalyzer
    {
        public static CriticalOperationAnalysisResult Analyze(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders)
        {
            // Alle Operationen werden gemeinsam analysiert, unabhängig von Job oder Maschine.
            List<Operation> allOperations =
                instance.Jobs
                .SelectMany(job => job.Operations)
                .ToList();

            /*
            Für jede Operation werden die direkten Beziehungen bestimmt.
            Job-Beziehungen ergeben sich aus der festen Operationsreihenfolge eines Jobs.
            Maschinen-Beziehungen ergeben sich aus der aktuellen Maschinenreihenfolge.
            */
            Dictionary<Operation, Operation?> jobPredecessor =
                BuildJobPredecessors(instance);

            Dictionary<Operation, Operation?> jobSuccessor =
                BuildJobSuccessors(instance);

            Dictionary<Operation, Operation?> machinePredecessor =
                BuildMachinePredecessors(machineOrders);

            Dictionary<Operation, Operation?> machineSuccessor =
                BuildMachineSuccessors(machineOrders);

            // r_i: frühestmöglicher Startzeitpunkt jeder Operation.
            Dictionary<Operation, int> releaseDates =
                CalculateReleaseDates(
                    allOperations,
                    jobPredecessor,
                    machinePredecessor,
                    instance);

            // q_i: längster verbleibender Restpfad ab der Operation inklusive eigener Bearbeitungszeit.
            Dictionary<Operation, int> tails =
                CalculateTails(
                    allOperations,
                    jobSuccessor,
                    machineSuccessor,
                    instance);

            /*
            Der Makespan ergibt sich als längster Gesamtpfad.
            Für jede Operation beschreibt r_i + q_i einen vollständigen Pfadanteil
            durch diese Operation.
            */
            int cmax =
                allOperations.Max(operation =>
                    releaseDates[operation] + tails[operation]);

            CriticalOperationAnalysisResult result =
                new CriticalOperationAnalysisResult();

            result.releaseTimes = releaseDates;
            result.tails = tails;
            result.cmax = cmax;

            // Kritisch sind genau die Operationen, die auf einem Cmax-bestimmenden Pfad liegen.
            foreach (Operation operation in allOperations)
            {
                int r = releaseDates[operation];
                int q = tails[operation];

                if (r + q == cmax)
                {
                    result.criticalOperations.Add(operation);
                }
            }

            return result;
        }

        /*
        Bestimmt für jede Operation den direkten Vorgänger im selben Job.
        Diese Beziehung entspricht der technologischen Reihenfolge des Jobs.
        */
        private static Dictionary<Operation, Operation?> BuildJobPredecessors(
            Instance instance)
        {
            Dictionary<Operation, Operation?> predecessors =
                new Dictionary<Operation, Operation?>();

            foreach (Job job in instance.Jobs)
            {
                for (int i = 0; i < job.Operations.Count; i++)
                {
                    Operation operation = job.Operations[i];

                    if (i == 0)
                    {
                        predecessors[operation] = null;
                    }
                    else
                    {
                        predecessors[operation] = job.Operations[i - 1];
                    }
                }
            }

            return predecessors;
        }

        /*
        Bestimmt für jede Operation den direkten Nachfolger im selben Job.
        Diese Beziehung wird für die Berechnung des Restpfads q_i benötigt.
        */
        private static Dictionary<Operation, Operation?> BuildJobSuccessors(
            Instance instance)
        {
            Dictionary<Operation, Operation?> successors =
                new Dictionary<Operation, Operation?>();

            foreach (Job job in instance.Jobs)
            {
                for (int i = 0; i < job.Operations.Count; i++)
                {
                    Operation operation = job.Operations[i];

                    if (i == job.Operations.Count - 1)
                    {
                        successors[operation] = null;
                    }
                    else
                    {
                        successors[operation] = job.Operations[i + 1];
                    }
                }
            }

            return successors;
        }

        /*
        Bestimmt für jede Operation den direkten Vorgänger auf derselben Maschine.
        Die Reihenfolge stammt aus dem aktuellen Schedule bzw. aus einem Tabu-Move.
        */
        private static Dictionary<Operation, Operation?> BuildMachinePredecessors(
            Dictionary<int, List<Operation>> machineOrders)
        {
            Dictionary<Operation, Operation?> predecessors =
                new Dictionary<Operation, Operation?>();

            foreach (var pair in machineOrders)
            {
                List<Operation> operationsOnMachine = pair.Value;

                for (int i = 0; i < operationsOnMachine.Count; i++)
                {
                    Operation operation = operationsOnMachine[i];

                    if (i == 0)
                    {
                        predecessors[operation] = null;
                    }
                    else
                    {
                        predecessors[operation] = operationsOnMachine[i - 1];
                    }
                }
            }

            return predecessors;
        }

        /*
        Bestimmt für jede Operation den direkten Nachfolger auf derselben Maschine.
        Diese Beziehung zeigt, welche Operation auf derselben Maschine danach folgt.
        */
        private static Dictionary<Operation, Operation?> BuildMachineSuccessors(
            Dictionary<int, List<Operation>> machineOrders)
        {
            Dictionary<Operation, Operation?> successors =
                new Dictionary<Operation, Operation?>();

            foreach (var pair in machineOrders)
            {
                List<Operation> operationsOnMachine = pair.Value;

                for (int i = 0; i < operationsOnMachine.Count; i++)
                {
                    Operation operation = operationsOnMachine[i];

                    if (i == operationsOnMachine.Count - 1)
                    {
                        successors[operation] = null;
                    }
                    else
                    {
                        successors[operation] = operationsOnMachine[i + 1];
                    }
                }
            }

            return successors;
        }

        /*
        Berechnet r_i für alle Operationen.

        r_i ist der frühestmögliche Startzeitpunkt einer Operation.
        Eine Operation kann erst starten, wenn sowohl ihr Job-Vorgänger als auch
        ihr Maschinen-Vorgänger abgeschlossen sind. Bei Maschinen-Vorgängern wird
        zusätzlich die reihenfolgeabhängige Setup-Zeit berücksichtigt.
        */
        private static Dictionary<Operation, int> CalculateReleaseDates(
            List<Operation> allOperations,
            Dictionary<Operation, Operation?> jobPredecessor,
            Dictionary<Operation, Operation?> machinePredecessor,
            Instance instance)
        {
            Dictionary<Operation, int> r =
                allOperations.ToDictionary(
                    operation => operation,
                    operation => 0);

            bool changed = true;
            int maxIterations = allOperations.Count * allOperations.Count;
            int iteration = 0;

            /*
            Die Werte werden iterativ erhöht, bis sie stabil sind.
            Dadurch können Abhängigkeiten berücksichtigt werden, auch wenn die
            Operationen nicht bereits in topologischer Reihenfolge vorliegen.
            */
            while (changed)
            {
                changed = false;
                iteration++;

                if (iteration > maxIterations)
                {
                    throw new InvalidOperationException(
                        "Die r_i-Werte konvergieren nicht. Wahrscheinlich enthält die Maschinenreihenfolge einen Zyklus.");
                }

                foreach (Operation operation in allOperations)
                {
                    int candidateFromJob = 0;
                    int candidateFromMachine = 0;

                    Operation? pj =
                        jobPredecessor[operation];

                    if (pj != null)
                    {
                        // Start frühestens nach Abschluss der vorherigen Operation im selben Job.
                        candidateFromJob =
                            r[pj] + pj.ProcessingTime;
                    }

                    Operation? pm =
                        machinePredecessor[operation];

                    if (pm != null)
                    {
                        int setup =
                            GetSetupTime(
                                instance,
                                pm,
                                operation);

                        // Start frühestens nach Maschinen-Vorgänger plus Setup-Zeit.
                        candidateFromMachine =
                            r[pm] + pm.ProcessingTime + setup;
                    }

                    /*
                    Beide Bedingungen müssen erfüllt sein.
                    Deshalb bestimmt das Maximum den frühestmöglichen Start.
                    */
                    int newR =
                        Math.Max(
                            candidateFromJob,
                            candidateFromMachine);

                    if (newR > r[operation])
                    {
                        r[operation] = newR;
                        changed = true;
                    }
                }
            }

            return r;
        }

        /*
        Berechnet q_i für alle Operationen.

        q_i ist der längste Restpfad ab einer Operation inklusive ihrer eigenen
        Bearbeitungszeit. Dafür werden die möglichen Nachfolger im Job und auf
        der Maschine betrachtet.
        */
        private static Dictionary<Operation, int> CalculateTails(
            List<Operation> allOperations,
            Dictionary<Operation, Operation?> jobSuccessor,
            Dictionary<Operation, Operation?> machineSuccessor,
            Instance instance)
        {
            Dictionary<Operation, int> q =
                allOperations.ToDictionary(
                    operation => operation,
                    operation => operation.ProcessingTime);

            bool changed = true;
            int maxIterations = allOperations.Count * allOperations.Count;
            int iteration = 0;

            /*
            Die Restpfade werden iterativ nach hinten fortgeschrieben.
            Der längere Nachfolgerpfad bestimmt, welcher Restpfad für q_i relevant ist.
            */
            while (changed)
            {
                changed = false;
                iteration++;

                if (iteration > maxIterations)
                {
                    throw new InvalidOperationException(
                        "Die q_i-Werte konvergieren nicht. Wahrscheinlich enthält die Maschinenreihenfolge einen Zyklus.");
                }

                foreach (Operation operation in allOperations)
                {
                    int candidateFromJob = 0;
                    int candidateFromMachine = 0;

                    Operation? sj =
                        jobSuccessor[operation];

                    if (sj != null)
                    {
                        // Restpfad über den nächsten Schritt im selben Job.
                        candidateFromJob =
                            q[sj];
                    }

                    Operation? sm =
                        machineSuccessor[operation];

                    if (sm != null)
                    {
                        int setup =
                            GetSetupTime(
                                instance,
                                operation,
                                sm);

                        // Restpfad über die nächste Operation auf derselben Maschine.
                        candidateFromMachine =
                            setup + q[sm];
                    }

                    /*
                    q_i enthält die eigene Bearbeitungszeit plus den längeren
                    der beiden möglichen Restpfade.
                    */
                    int newQ =
                        operation.ProcessingTime +
                        Math.Max(
                            candidateFromJob,
                            candidateFromMachine);

                    if (newQ > q[operation])
                    {
                        q[operation] = newQ;
                        changed = true;
                    }
                }
            }

            return q;
        }

        /*
        Gibt die reihenfolgeabhängige Setup-Zeit zwischen zwei Jobs zurück.
        Da JobIDs bei 1 beginnen, wird für den Matrixzugriff JobID - 1 verwendet.
        */
        private static int GetSetupTime(
            Instance instance,
            Operation before,
            Operation after)
        {
            return instance.SetupTimes[
                before.JobID - 1,
                after.JobID - 1];
        }
    }
}