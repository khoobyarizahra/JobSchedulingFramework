using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    // Hilfsklasse zur Berechnung der Start- und Endzeiten der Operationen basierend auf der gegebenen Maschinenreihenfolge und den Job-Abhängigkeiten.
    public static class ScheduleRecalculator
    {
        //Die Methode Recalculate berechnet die Start- und Endzeiten der Operationen
        //basierend auf der gegebenen Maschinenreihenfolge (machineOrders) und den Job-Abhängigkeiten in der Instanz.
        //Als Parameter erhält sie die Instanz, die Maschinenreihenfolge und gibt die berechnete Cmax zurück.
        //Die Methode gibt einen booleschen Wert zurück, der angibt, ob die Berechnung erfolgreich war (d.h. ob alle Operationen geplant werden konnten).
        public static bool Recalculate(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders,
            out int cmax)
        {
            // Initialisierung von cmax, der die maximale Fertigstellungszeit aller Operationen darstellt.
            cmax = 0;
            // Alle Operationen aus allen Jobs in der Instanz sammeln.
            List<Operation> allOperations =
                instance.Jobs.SelectMany(job => job.Operations).ToList();
            // openPredecessorCount speichert die Anzahl der noch offenen Vorgängeroperationen für jede Operation, bevor sie geplant werden kann.
            Dictionary<Operation, int> openPredecessorCount =
                new Dictionary<Operation, int>();
            // directSuccessors speichert die direkten Nachfolgeroperationen für jede Operation, basierend auf den Job-Abhängigkeiten und der Maschinenreihenfolge.
            Dictionary<Operation, List<Operation>> directSuccessors =
                new Dictionary<Operation, List<Operation>>();
            // Initialisierung der Datenstrukturen für alle Operationen.
            foreach (Operation operation in allOperations)
            {
                // Alle Operationen haben zu Beginn keine offenen Vorgänger, da wir sie später basierend auf den Job-Abhängigkeiten
                // und der Maschinenreihenfolge aktualisieren werden.
                openPredecessorCount[operation] = 0;
                directSuccessors[operation] = new List<Operation>();
                // Start- und Endzeiten aller Operationen werden auf 0 gesetzt, da sie neu berechnet werden.
                operation.StartTime = 0;
                operation.EndTime = 0;
            }
            // Aufbau der direkten Nachfolgerbeziehungen basierend auf den Job-Abhängigkeiten.
            //Iteration über alle Jobs und deren Operationen, um die direkten Nachfolger zu bestimmen.
            //Jede Operation (außer der letzten) hat als direkten Nachfolger die nächste Operation im selben Job.
            foreach (Job job in instance.Jobs)
            {
                for (int i = 0; i < job.Operations.Count - 1; i++)
                {
                    Operation before = job.Operations[i];
                    Operation after = job.Operations[i + 1];

                    directSuccessors[before].Add(after);
                    openPredecessorCount[after]++;
                }
            }
            // Aufbau der direkten Nachfolgerbeziehungen basierend auf der Maschinenreihenfolge.
            foreach (var pair in machineOrders)
            {
                //Zuerst die Operationen auf der aktuellen Maschine extrahieren, um die direkten Nachfolger basierend auf der Reihenfolge zu bestimmen.
                List<Operation> operationsOnMachine = pair.Value;
                // Jede Operation (außer der letzten) hat als direkten Nachfolger die nächste Operation auf derselben Maschine.
                for (int i = 0; i < operationsOnMachine.Count - 1; i++)
                {
                    Operation before = operationsOnMachine[i];
                    Operation after = operationsOnMachine[i + 1];

                    directSuccessors[before].Add(after);
                    openPredecessorCount[after]++;
                }
            }
            // Initialisierung der Warteschlange mit den Operationen, die keine offenen Vorgänger haben und somit sofort geplant werden können.
            Queue<Operation> readyOperations =
                new Queue<Operation>(
                    allOperations.Where(operation =>
                        openPredecessorCount[operation] == 0));
            // Anzahl der geplanten Operationen, um am Ende zu überprüfen, ob alle Operationen erfolgreich geplant wurden.
            int numberOfScheduledOperations = 0;
            // Hauptschleife zur Planung der Operationen, bis keine mehr bereit sind.
            while (readyOperations.Count > 0)
            {
                // Die nächste Operation aus der Warteschlange entnehmen, die geplant werden soll.
                Operation current = readyOperations.Dequeue();
                // Berechnung der frühesten Startzeit für die aktuelle Operation basierend auf den bereits geplanten Vorgängeroperationen und der Maschinenreihenfolge.
                int earliestStart =
                    EarliestStartCalculator.Calculate(
                        instance,
                        machineOrders,
                        current);
                // Setzen der Start- und Endzeiten der aktuellen Operation basierend auf der berechneten frühesten Startzeit und der Verarbeitungszeit.
                current.StartTime = earliestStart;
                current.EndTime =
                    current.StartTime + current.ProcessingTime;
                // Aktualisierung von cmax, um die maximale Fertigstellungszeit aller geplanten Operationen zu verfolgen.
                cmax = Math.Max(cmax, current.EndTime);
                // Erhöhung der Anzahl der geplanten Operationen, um am Ende überprüfen zu können, ob alle Operationen erfolgreich geplant wurden.
                numberOfScheduledOperations++;
                // Aktualisierung der offenen Vorgängeranzahl für die direkten Nachfolger der aktuellen Operation.
                foreach (Operation successor in directSuccessors[current])
                {
                    // Reduzierung der Anzahl der offenen Vorgänger für den Nachfolger, da die aktuelle Operation nun geplant ist.
                    openPredecessorCount[successor]--;
                    // Wenn der Nachfolger keine offenen Vorgänger mehr hat, wird er zur Warteschlange der bereitstehenden Operationen hinzugefügt, da er nun geplant werden kann.
                    if (openPredecessorCount[successor] == 0)
                    {
                        readyOperations.Enqueue(successor);
                    }
                }
            }

            return numberOfScheduledOperations == allOperations.Count;
        }
    }
}