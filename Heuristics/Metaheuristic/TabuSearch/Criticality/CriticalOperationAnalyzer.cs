using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality
{
    /*
     CRITICAL OPERATION ANALYZER

     Diese Klasse berechnet kritische Operationen direkt nach der Logik aus dem Artikel.

     Wichtig:
     Es wird KEIN kritischer Pfad rekonstruiert.
     Es wird auch KEIN allgemeiner Graph mit topologischer Sortierung aufgebaut.

     Stattdessen wird direkt mit den vier Nachbarschaftsbeziehungen gearbeitet:

     PJ_i = Vorgängeroperation desselben Jobs
     PM_i = Vorgängeroperation auf derselben Maschine
     SJ_i = Nachfolgeroperation desselben Jobs
     SM_i = Nachfolgeroperation auf derselben Maschine

     Danach gilt:

     r_i = frühestmöglicher Startzeitpunkt von Operation i

     q_i = Länge des längsten Restpfades ab Operation i
           inklusive Bearbeitungszeit von Operation i selbst

     Operation i ist kritisch genau dann, wenn:

     r_i + q_i == Cmax
    */
    public static class CriticalOperationAnalyzer
    {
        public static CriticalOperationAnalysisResult Analyze(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders)
        {
            // Alle Operationen in einer Liste sammeln
            List<Operation> allOperations =
                instance.Jobs
                .SelectMany(job => job.Operations)
                .ToList();
            //JobPredecessor als Dictionary aufbauen, damit wir schnell auf Job-Vorgänger zugreifen können
            Dictionary<Operation, Operation?> jobPredecessor =
                BuildJobPredecessors(instance);
            //JobSuccessor als Dictionary aufbauen, damit wir schnell auf Job-Nachfolger zugreifen können
            Dictionary<Operation, Operation?> jobSuccessor =
                BuildJobSuccessors(instance);
            //MachinePredecessor als Dictionary aufbauen, damit wir schnell auf Maschinen-Vorgänger zugreifen können
            Dictionary<Operation, Operation?> machinePredecessor =
                BuildMachinePredecessors(machineOrders);
            //MachineSuccessor als Dictionary aufbauen, damit wir schnell auf Maschinen-Nachfolger zugreifen können
            Dictionary<Operation, Operation?> machineSuccessor =
                BuildMachineSuccessors(machineOrders);
            // Alle r_i-Werte berechnen und in einem Dictionary speichern, als key verwenden wir die Operation
            Dictionary<Operation, int> releaseDates =
                CalculateReleaseDates(
                    allOperations,
                    jobPredecessor,
                    machinePredecessor,
                    instance);
            // Alle q_i-Werte berechnen und in einem Dictionary speichern, als key verwenden wir die Operation
            Dictionary<Operation, int> tails =
                CalculateTails(
                    allOperations,
                    jobSuccessor,
                    machineSuccessor,
                    instance);
            // Cmax berechnen, als Maximum über alle Operationen von r_i + q_i
            int cmax =
                allOperations.Max(operation =>
                    releaseDates[operation] + tails[operation]);
            //result-Objekt erstellen und mit den berechneten Werten füllen
            CriticalOperationAnalysisResult result =
                new CriticalOperationAnalysisResult();

            result.releaseTimes = releaseDates;
            result.tails = tails;
            result.cmax = cmax;
            //wir iterieren über alle Operationen und fügen diejenigen zur Menge der kritischen Operationen hinzu, für die r_i + q_i == Cmax gilt
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
         Baut PJ_i.

         PJ_i ist die direkte Vorgängeroperation im selben Job.

         Beispiel:
         Job 1: O1 -> O2 -> O3

         PJ(O1) = null
         PJ(O2) = O1
         PJ(O3) = O2
        */

        private static Dictionary<Operation, Operation?> BuildJobPredecessors(
            Instance instance)
        {
            Dictionary<Operation, Operation?> predecessors =
                new Dictionary<Operation, Operation?>();
            // Wir iterieren über alle Jobs und deren Operationen, um die Vorgängerbeziehung zu bestimmen
            foreach (Job job in instance.Jobs)
            {
                for (int i = 0; i < job.Operations.Count; i++)
                {
                    Operation operation = job.Operations[i];
                    // Wenn es die erste Operation im Job ist, hat sie keinen Vorgänger, sonst ist der Vorgänger die vorherige Operation im Job
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
         Baut SJ_i.

         SJ_i ist die direkte Nachfolgeroperation im selben Job.
        */
        private static Dictionary<Operation, Operation?> BuildJobSuccessors(
            Instance instance)
        {
            Dictionary<Operation, Operation?> successors =
                new Dictionary<Operation, Operation?>();
            // Wir iterieren über alle Jobs in Instance und deren Operationen, um die Nachfolgerbeziehung zu bestimmen
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
         Baut PM_i.

         PM_i ist die direkte Vorgängeroperation auf derselben Maschine.

         Die Reihenfolge kommt aus machineOrders.

         Beispiel:
         Maschine 2: A -> B -> C

         PM(A) = null
         PM(B) = A
         PM(C) = B
        */
        private static Dictionary<Operation, Operation?> BuildMachinePredecessors(
            Dictionary<int, List<Operation>> machineOrders)
        {
            Dictionary<Operation, Operation?> predecessors =
                new Dictionary<Operation, Operation?>();

            //wir iterieren über alle Maschinen und deren Operationen, um die Vorgängerbeziehung zu bestimmen
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
         Baut SM_i.

         SM_i ist die direkte Nachfolgeroperation auf derselben Maschine.
        */
        private static Dictionary<Operation, Operation?> BuildMachineSuccessors(
            Dictionary<int, List<Operation>> machineOrders)
        {
            Dictionary<Operation, Operation?> successors =
                new Dictionary<Operation, Operation?>();
            //wir iterieren über alle Maschinen und deren Operationen, um die Nachfolgerbeziehung zu bestimmen
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
         Berechnet alle r_i-Werte.

         Artikel-Logik:

         r_i = max(
             r_PJ_i + p_PJ_i,
             r_PM_i + p_PM_i + setup(PM_i, i)
         )

         Falls es keinen Vorgänger gibt, wird der entsprechende Wert als 0 betrachtet.

         Wir verwenden eine Bellman-artige Iteration:
         Die Werte werden wiederholt aktualisiert, bis sich nichts mehr ändert.
        */
        private static Dictionary<Operation, int> CalculateReleaseDates(
            List<Operation> allOperations,
            Dictionary<Operation, Operation?> jobPredecessor,
            Dictionary<Operation, Operation?> machinePredecessor,
            Instance instance)
        {
            // Alle r_i-Werte initial auf 0 setzen
            Dictionary<Operation, int> r =
                allOperations.ToDictionary(
                    operation => operation,
                    operation => 0);
            //changed-Flag, um zu überprüfen, ob sich in einer Iteration etwas geändert hat
            //Mit Änderungen meinen wir, dass mindestens ein r_i-Wert aktualisiert wurde, weil er einen größeren Wert angenommen hat,
            //was bedeutet,deren Nachfolger möglicherweise auch aktualisiert werden müssen
            bool changed = true;
            //maxIterations, um eine Endlosschleife zu verhindern,
            //falls die Werte nicht konvergieren (z.B. wegen eines Zyklus in der Maschinenreihenfolge)
            //allOperations.Count * allOperations.Count ist eine konservative Schätzung,
            //da im schlimmsten Fall jeder r_i-Wert von jedem anderen r_i-Wert abhängen könnte
            int maxIterations =
                allOperations.Count * allOperations.Count;

            int iteration = 0;

            while (changed)
            {
                //zuerst setzen wir das changed-Flag auf false, und wenn wir in der Iteration feststellen,
                //dass sich ein r_i-Wert ändert, setzen wir es wieder auf true
                changed = false;
                iteration++;

                if (iteration > maxIterations)
                {
                    throw new InvalidOperationException(
                        "Die r_i-Werte konvergieren nicht. Wahrscheinlich enthält die Maschinenreihenfolge einen Zyklus.");
                }
                // Wir iterieren über alle Operationen und berechnen die neuen r_i-Werte basierend auf den Vorgängerbeziehungen
                foreach (Operation operation in allOperations)
                {
                    //candidateFromJob berechnet den Wert von Job-Vorgänger, candidateFromMachine berechnet den Wert von Maschinen-Vorgänger
                    int candidateFromJob = 0;
                    int candidateFromMachine = 0;
                    // Wir schauen uns den Job-Vorgänger an, falls vorhanden, und berechnen den Beitrag zum r_i-Wert
                    //? Operator wird verwendet, um zu überprüfen, ob es einen Vorgänger gibt. Wenn ja,
                    //wird der Wert berechnet, sonst bleibt er 0
                    Operation? pj =
                        jobPredecessor[operation];

                    if (pj != null)
                    {
                        // Wenn es einen Job-Vorgänger gibt, berechnen wir den Beitrag zum r_i-Wert
                        candidateFromJob =
                            r[pj] + pj.ProcessingTime;
                    }
                    // Wir schauen uns den Maschinen-Vorgänger an, falls vorhanden, und berechnen den Beitrag zum r_i-Wert inklusive Rüstzeit
                    Operation? pm =
                        machinePredecessor[operation];
                    // Wenn es einen Maschinen-Vorgänger gibt, berechnen wir den Beitrag zum r_i-Wert inklusive Rüstzeit
                    if (pm != null)
                    {
                        int setup =
                            GetSetupTime(
                                instance,
                                pm,
                                operation);

                        candidateFromMachine =
                            r[pm] + pm.ProcessingTime + setup;
                    }
                    // Der neue r_i-Wert ist das Maximum aus beiden Beiträgen
                    //weil die Operation erst starten kann, wenn sowohl der Job-Vorgänger als auch der Maschinen-Vorgänger fertig sind
                    int newR =
                        Math.Max(
                            candidateFromJob,
                            candidateFromMachine);
                    // Wenn der neue r_i-Wert größer ist als der bisherige,
                    // aktualisieren wir ihn und setzen das changed-Flag auf true, damit die Iteration fortgesetzt wird
                    //weil sich durch die Aktualisierung eines r_i-Werts auch die r_i-Werte der Nachfolger ändern können,
                    //müssen wir so lange iterieren, bis sich nichts mehr ändert
                    //also endet die Iteration, wenn alle r_i-Werte stabil sind und sich nicht mehr ändern
                    if (newR > r[operation])
                    {
                        //wenn der neue r_i-Wert größer ist, aktualisieren wir ihn im Dictionary
                        r[operation] = newR;
                        //und setzen das changed-Flag auf true, damit die Iteration fortgesetzt wird
                        changed = true;
                    }
                }
            }

            return r;
        }

        /*
         Berechnet alle q_i-Werte.

         Artikel-Logik mit q inklusive eigener Bearbeitungszeit:

         q_i = p_i + max(
             q_SJ_i,
             setup(i, SM_i) + q_SM_i
         )

         Falls es keinen Nachfolger gibt, wird der entsprechende Wert als 0 betrachtet.

         Für letzte Operationen gilt dadurch automatisch:

         q_i = p_i
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

            int maxIterations =
                allOperations.Count * allOperations.Count;

            int iteration = 0;

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

                        candidateFromMachine =
                            setup + q[sm];
                    }

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
         Gibt die reihenfolgeabhängige Rüstzeit zurück.

         Wichtig:
         SetupTimes ist nach Jobs indexiert.

         Da JobID bei euch bei 1 beginnt, verwenden wir JobID - 1.
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