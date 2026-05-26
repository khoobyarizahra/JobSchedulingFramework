using JobShopSchedulingFramework.Heuristics.Metaheuristics;
using JobShopSchedulingFramework.Models;
using System.Collections.Generic;
using System.Linq;

namespace JobShopSchedulingFramework.Heuristics.Tabu
{
    public static class CriticalPathFinder
    {
        public static List<Operation> FindCriticalPath(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders)
        {
            //wir starten mit der Operation, die am spätesten fertig wird,
            //da diese den aktuellen Cmax bestimmt.
            //Dafür müssen wir alle Operationen aller Jobs betrachten
            //und diejenige mit der größten Endzeit auswählen.
            Operation current = instance.jobs
                .SelectMany(job => job.operations)
                .OrderByDescending(op => op.endTime)
                .First();

            //Hier speichern wir den kritischen Pfad rückwärts,
            //da wir von hinten nach vorne durch die Vorgänger laufen.
            List<Operation> criticalPathBackwards =
                new List<Operation>();

            //Wir laufen solange rückwärts,
            //bis keine Vorgänger mehr existieren.
            while (current != null)
            {
                //Die aktuelle Operation gehört zum kritischen Pfad
                //und wird deshalb gespeichert.
                criticalPathBackwards.Add(current);

                //Es gibt zwei mögliche Vorgänger:
                //1. Job-Vorgänger
                //2. Maschinen-Vorgänger
                //
                //Diese Methoden aus ScheduleOrderHelper liefern
                //die entsprechenden Vorgänger zurück.
                Operation jobPredecessor =
                    ScheduleOrderHelper.GetJobPredecessor(instance, current);

                Operation machinePredecessor =
                    ScheduleOrderHelper.GetMachinePredecessor(machineOrders, current);

                //valueFromJob beschreibt,
                //wie stark der Job-Vorgänger den Start
                //der aktuellen Operation beeinflusst hat.
                //
                //-1 bedeutet:
                //Es existiert kein Vorgänger.
                int valueFromJob = -1;

                //Falls ein Job-Vorgänger existiert,
                //verwenden wir dessen Endzeit.
                if (jobPredecessor != null)
                    valueFromJob = jobPredecessor.endTime;

                //valueFromMachine beschreibt,
                //wie stark der Maschinen-Vorgänger den Start
                //der aktuellen Operation beeinflusst hat.
                int valueFromMachine = -1;

                if (machinePredecessor != null)
                {
                    //Beim Maschinen-Vorgänger müssen zusätzlich
                    //die Setupzeiten berücksichtigt werden.
                    int setup =
                        instance.setupTimes[machinePredecessor.jobID - 1,
                                            current.jobID - 1];

                    valueFromMachine =
                        machinePredecessor.endTime + setup;
                }

                //Wenn weder ein Job- noch ein Maschinen-Vorgänger existiert,
                //haben wir den Anfang des kritischen Pfades erreicht.
                if (jobPredecessor == null && machinePredecessor == null)
                    break;

                //Wir folgen dem Vorgänger,
                //der den größeren Einfluss auf den Startzeitpunkt hatte.
                //
                //Falls beide gleich groß sind,
                //bevorzugen wir den Maschinen-Vorgänger,
                //da unsere Neighborhood auf Maschinenkonflikten basiert.
                if (machinePredecessor != null &&
                    valueFromMachine >= valueFromJob)
                {
                    current = machinePredecessor;
                }
                else if (jobPredecessor != null)
                {
                    current = jobPredecessor;
                }
                else
                {
                    break;
                }
            }

            //Da wir den Pfad rückwärts aufgebaut haben,
            //müssen wir die Reihenfolge jetzt umdrehen.
            criticalPathBackwards.Reverse();

            return criticalPathBackwards;
        }

        //Nachdem wir den kritischen Pfad gefunden haben,
        //können wir daraus die kritischen Blöcke extrahieren.
        //
        //Ein kritischer Block ist eine zusammenhängende Sequenz
        //von Operationen derselben Maschine,
        //die alle auf dem kritischen Pfad liegen.
        public static List<CriticalBlock> ExtractCriticalBlocks(
            List<Operation> criticalPath)
        {
            //Liste aller kritischen Blöcke,
            //die später zurückgegeben wird.
            List<CriticalBlock> blocks =
                new List<CriticalBlock>();

            //Falls der kritische Pfad leer ist,
            //existieren auch keine kritischen Blöcke.
            if (criticalPath.Count == 0)
                return blocks;

            //currentBlock speichert den aktuellen Block,
            //den wir gerade aufbauen.
            //
            //Wir starten mit der Maschine
            //der ersten Operation des kritischen Pfades.
            CriticalBlock currentBlock =
                new CriticalBlock(criticalPath[0].machine);

            //Die erste Operation gehört definitiv
            //zum ersten kritischen Block.
            currentBlock.operations.Add(criticalPath[0]);

            //Nun laufen wir durch die restlichen Operationen
            //des kritischen Pfades.
            for (int i = 1; i < criticalPath.Count; i++)
            {
                Operation operation = criticalPath[i];

                //Falls die aktuelle Operation
                //auf derselben Maschine liegt,
                //gehört sie zum aktuellen Block.
                if (operation.machine == currentBlock.machine)
                {
                    currentBlock.operations.Add(operation);
                }
                else
                {
                    //Falls die Maschine wechselt,
                    //ist der aktuelle Block abgeschlossen.
                    //
                    //Wir speichern nur Blöcke,
                    //die mindestens zwei Operationen besitzen,
                    //da nur dort ein Swap sinnvoll ist.
                    if (currentBlock.operations.Count >= 2)
                        blocks.Add(currentBlock);

                    //Jetzt starten wir einen neuen Block
                    //für die neue Maschine.
                    currentBlock =
                        new CriticalBlock(operation.machine);

                    //Die aktuelle Operation gehört sofort
                    //zum neuen Block.
                    currentBlock.operations.Add(operation);
                }
            }

            //Nachdem alle Operationen verarbeitet wurden,
            //müssen wir den letzten Block separat prüfen.
            //
            //Der letzte Block wird nicht automatisch gespeichert,
            //weil danach kein Maschinenwechsel mehr kommt.
            if (currentBlock.operations.Count >= 2)
                blocks.Add(currentBlock);

            return blocks;
        }
    }
}