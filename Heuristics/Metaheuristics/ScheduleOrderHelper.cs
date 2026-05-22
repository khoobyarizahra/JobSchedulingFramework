using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristics
{
    /*
     SCHEDULE ORDER HELPER

     Diese Klasse stellt Hilfsmethoden für die Tabu Search bereit.

     Zentrale Idee:
     Die Tabu Search verändert nicht direkt Start- und Endzeiten,
     sondern zuerst nur die Reihenfolge der Operationen auf den Maschinen.

     Beispiel:
     M1: J1O1 -> J3O2 -> J2O1

     Nach einem Move werden aus diesen Maschinenreihenfolgen
     die Startzeiten, Endzeiten und der neue Cmax neu berechnet.
    */
    public static class ScheduleOrderHelper
    {
        /*
         Erstellt aus dem aktuellen Schedule die Reihenfolge
         der Operationen auf jeder Maschine.

         Die aktuelle Reihenfolge ergibt sich aus den Startzeiten.
        */
        public static Dictionary<int, List<Operation>> BuildMachineOrders(Instance instance)
        {
            Dictionary<int, List<Operation>> machineOrders =
                new Dictionary<int, List<Operation>>();

            // Für jede Maschine wird eine eigene Operationsliste angelegt.
            for (int machine = 1; machine <= instance.numMachines; machine++)
            {
                machineOrders[machine] = new List<Operation>();
            }

            // Jede Operation wird zu der Maschine hinzugefügt, auf der sie bearbeitet wird.
            foreach (Job job in instance.jobs)
            {
                foreach (Operation operation in job.operations)
                {
                    machineOrders[operation.machine].Add(operation);
                }
            }

            // Auf jeder Maschine wird nach Startzeit sortiert.
            // Dadurch entsteht die tatsächliche Maschinenreihenfolge des aktuellen Schedules.
            foreach (int machine in machineOrders.Keys.ToList())
            {
                machineOrders[machine] = machineOrders[machine]
                    .OrderBy(op => op.startTime)
                    .ThenBy(op => op.endTime)
                    .ToList();
            }

            return machineOrders;
        }

        /*
         Kopiert die Maschinenreihenfolgen.

         Wichtig für die Tabu Search:
         Ein Nachbar soll getestet werden, ohne die aktuelle Lösung direkt zu verändern.
        */
        public static Dictionary<int, List<Operation>> CopyMachineOrders(
            Dictionary<int, List<Operation>> original)
        {
            Dictionary<int, List<Operation>> copy =
                new Dictionary<int, List<Operation>>();

            foreach (var pair in original)
            {
                // Neue Liste, aber gleiche Operation-Objekte.
                // Das reicht, weil hier nur die Reihenfolge kopiert wird.
                copy[pair.Key] = new List<Operation>(pair.Value);
            }

            return copy;
        }

        /*
         Wendet einen Move auf die Maschinenreihenfolge an.

         Der Move enthält:
         - die betroffene Maschine
         - die Position der ersten Operation
         - die Position der zweiten Operation

         Diese Methode ändert NUR die Reihenfolge.
         Die Zeiten werden danach separat neu berechnet.
        */
        public static void ApplyMove(
            Dictionary<int, List<Operation>> machineOrders,
            Move move)
        {
            // Hole die Operationsliste der Maschine, auf der der Move stattfindet.
            List<Operation> operationsOnMachine = machineOrders[move.machine];

            // Prüfe, ob die gespeicherten Positionen gültig sind.
            if (move.firstIndex < 0 || move.firstIndex >= operationsOnMachine.Count ||
                move.secondIndex < 0 || move.secondIndex >= operationsOnMachine.Count)
            {
                throw new InvalidOperationException(
                    "Der Move enthält ungültige Positionen für die Maschinenreihenfolge.");
            }

            // Wenn beide Indizes gleich sind, würde sich nichts ändern.
            if (move.firstIndex == move.secondIndex)
                return;

            // Vertausche die beiden Operationen in der Liste.
            // wir definieren temp als Operation, damit wir die Werte zwischenspeichern können, bevor wir sie vertauschen.
            Operation temp = operationsOnMachine[move.firstIndex];
            //jetzt wird die erste Operation an die Stelle der zweiten Operation gesetzt.
            operationsOnMachine[move.firstIndex] = operationsOnMachine[move.secondIndex];
            //jetzt wird die zweite Operation an die Stelle der ersten Operation gesetzt.
            //Da wir die erste Operation in temp zwischengespeichert haben,
            //können wir sie jetzt an die Stelle der zweiten Operation setzen.
            operationsOnMachine[move.secondIndex] = temp;
        }

        /*
         Berechnet aus den Maschinenreihenfolgen einen neuen Schedule.

         Dabei werden zwei Abhängigkeiten berücksichtigt:

         1. Job-Reihenfolge:
            J1O1 muss vor J1O2 liegen.

         2. Maschinen-Reihenfolge:
            Wenn auf M2 A vor B steht, darf B erst nach A starten.

         Zusätzlich werden Rüstzeiten zwischen zwei aufeinanderfolgenden Jobs
         auf derselben Maschine berücksichtigt.

         Rückgabe:
         true  = gültiger Schedule
         false = unzulässiger Schedule, weil ein Zyklus entstanden ist
        */
        public static bool RecalculateScheduleFromMachineOrders(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders,
            out int cmax)
        {
            //zuesrt wird cmax auf 0 gesetzt,
            //damit wir später den maximalen Endzeitpunkt der Operationen berechnen können.
            cmax = 0;
            // Diese Datenstrukturen helfen uns, die Abhängigkeiten zu verfolgen.
            // remainingPredecessors zählt, wie viele Vorgänger eine Operation noch hat, bevor sie eingeplant werden kann.
            Dictionary<Operation, int> remainingPredecessors =
                new Dictionary<Operation, int>();
            // successors speichert, welche Operationen von einer bestimmten Operation abhängen.
            Dictionary<Operation, List<Operation>> successors =
                new Dictionary<Operation, List<Operation>>();

            // Alle Operationen vorbereiten.
            // Start- und Endzeiten zurücksetzen, damit wir sie neu berechnen können.
            foreach (Job job in instance.jobs)
            {
                foreach (Operation operation in job.operations)
                {
                    operation.startTime = 0;
                    operation.endTime = 0;
                    // Alle Operationen starten mit 0 verbleibenden Vorgängern.
                    remainingPredecessors[operation] = 0;
                    // Alle Operationen haben zunächst keine Nachfolger.
                    successors[operation] = new List<Operation>();
                }
            }

            // Job-Abhängigkeiten einfügen: J1O1 -> J1O2 -> J1O3
            //hier wird die Reihenfolge der Operationen innerhalb eines Jobs berücksichtigt.
            // Wenn J1O1 vor J1O2 liegt, dann muss J1O2 erst eingeplant werden, wenn J1O1 fertig ist.
            foreach (Job job in instance.jobs)
            {
                for (int i = 0; i < job.operations.Count - 1; i++)
                {
                    //before ist die vorherige Operation, after ist die nächste Operation im selben Job.
                    Operation before = job.operations[i];
                    //after ist die nächste Operation im selben Job, die erst eingeplant werden kann, wenn before fertig ist.
                    Operation after = job.operations[i + 1];
                    //hier wird die Abhängigkeit zwischen before und after festgelegt.
                    successors[before].Add(after);
                    //Wir erhöhen die Anzahl der verbleibenden Vorgänger von after,
                    //weil before ein Vorgänger von after ist.
                    remainingPredecessors[after]++;
                }
            }

            // Maschinen-Abhängigkeiten einfügen: A -> B -> C auf derselben Maschine
            //pair ist ein Key-Value-Paar aus der Dictionary machineOrders,
            //wobei Key die Maschinennummer und Value die Liste der Operationen auf dieser Maschine ist.
            //wir benutzen pair in each loop, weil wir eine Dictionary durchlaufen. 
            foreach (var pair in machineOrders)
            {
                List<Operation> operationsOnMachine = pair.Value;

                for (int i = 0; i < operationsOnMachine.Count - 1; i++)
                {
                    //before ist die vorherige Operation auf derselben Maschine,
                    //after ist die nächste Operation auf derselben Maschine.
                    Operation before = operationsOnMachine[i];
                    //after ist die nächste Operation auf derselben Maschine,
                    //die erst eingeplant werden kann, wenn before fertig ist.
                    Operation after = operationsOnMachine[i + 1];
                    //hier wird die Abhängigkeit zwischen before und after festgelegt.
                    successors[before].Add(after);
                    remainingPredecessors[after]++;
                }
            }
            // Alle Operationen, die keine Vorgänger haben, können sofort eingeplant werden. Queue für die topologische Sortierung.
            //Queue ist eine Datenstruktur, die nach dem First-In-First-Out-Prinzip arbeitet. Hier werden Operationen gespeichert,
            //die bereit sind, eingeplant zu werden, weil alle ihre Vorgänger bereits eingeplant wurden.
            Queue<Operation> readyOperations = new Queue<Operation>();

            // Operationen ohne Vorgänger können zuerst eingeplant werden.
            foreach (var pair in remainingPredecessors)
            {
                if (pair.Value == 0)
                    //enqueue fügt ein Element am Ende der Queue hinzu. Hier werden alle Operationen, die keine Vorgänger haben,
                    //in die readyOperations Queue eingefügt, damit sie als erstes eingeplant werden können.
                    readyOperations.Enqueue(pair.Key);
            }

            //wir brauchen scheduledCounter, um zu zählen,
            //wie viele Operationen tatsächlich eingeplant wurden. Das ist wichtig, um am Ende zu überprüfen,
            int scheduledCounter = 0;
            //wir brauchen totalOperations, um die Gesamtzahl der Operationen zu kennen, damit wir am Ende überprüfen können,
            int totalOperations = instance.jobs.Sum(job => job.operations.Count);

            // Topologische Verarbeitung:
            // Eine Operation wird erst eingeplant, wenn alle Vorgänger fertig sind.
            // readyOperations.Count > 0, solange es noch Operationen gibt, die eingeplant werden können.
            while (readyOperations.Count > 0)
            {
                //dequeue entfernt das Element am Anfang der Queue und gibt es zurück. Hier wird die nächste Operation, die eingeplant werden kann,
                //aus der readyOperations Queue entfernt und in der Variable operation gespeichert.
                Operation operation = readyOperations.Dequeue();

                Operation jobPredecessor =
                    GetJobPredecessor(instance, operation);

                Operation machinePredecessor =
                    GetMachinePredecessor(machineOrders, operation);

                int earliestByJob = 0;

                if (jobPredecessor != null)
                    earliestByJob = jobPredecessor.endTime;

                int earliestByMachine = 0;

                if (machinePredecessor != null)
                {
                    int setup =
                        instance.setupTimes[machinePredecessor.jobID - 1,
                                            operation.jobID - 1];

                    earliestByMachine =
                        machinePredecessor.endTime + setup;
                }

                // Startzeit ist der früheste Zeitpunkt, an dem Job und Maschine bereit sind.
                operation.startTime =
                    Math.Max(earliestByJob, earliestByMachine);

                operation.endTime =
                    operation.startTime + operation.processingTime;

                cmax = Math.Max(cmax, operation.endTime);
                scheduledCounter++;

                // Nachfolger werden freigegeben, wenn alle ihre Vorgänger eingeplant wurden.
                foreach (Operation successor in successors[operation])
                {
                    remainingPredecessors[successor]--;

                    if (remainingPredecessors[successor] == 0)
                        readyOperations.Enqueue(successor);
                }
            }

            // Wenn nicht alle Operationen eingeplant wurden, enthält der Graph einen Zyklus.
            return scheduledCounter == totalOperations;
        }

        /*
         Liefert die vorherige Operation desselben Jobs.
         Beispiel: Vorgänger von J2O3 ist J2O2.
        */
        public static Operation GetJobPredecessor(
            Instance instance,
            Operation operation)
        {
            if (operation.operationID <= 1)
                return null;

            //-1, weil die IDs bei 1 beginnen, aber die Liste bei 0 indiziert ist.
            Job job = instance.jobs[operation.jobID - 1];
            //-2, weil wir die vorherige Operation wollen, also eine Position zurückgehen müssen.
            return job.operations[operation.operationID - 2];
        }

        /*
         Liefert die vorherige Operation auf derselben Maschine.
         Beispiel: M2: A -> B -> C
         Vorgänger von C ist B.
        */
        public static Operation GetMachinePredecessor(
            Dictionary<int, List<Operation>> machineOrders,
            Operation operation)
        {
            //hole die Liste der Operationen auf der Maschine, auf der die gegebene Operation bearbeitet wird.
            List<Operation> operationsOnMachine =
                machineOrders[operation.machine];
            //Finde den Index der gegebenen Operation in der Liste der Operationen auf derselben Maschine.
            int index = operationsOnMachine.IndexOf(operation);
            //wenn die Index -Position 0  ist, bedeutet das, dass die gegebene Operation die erste Operation auf dieser Maschine ist
            //und somit keinen Vorgänger hat. In diesem Fall wird null zurückgegeben.
            if (index <= 0)
                return null;
            //Ansonsten wird die Operation zurückgegeben,
            //die direkt vor der gegebenen Operation in der Liste steht, also die vorherige Operation auf derselben Maschine.
            return operationsOnMachine[index - 1];
        }
    }
}