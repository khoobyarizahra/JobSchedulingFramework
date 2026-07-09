
using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /*
    Berechnet aus einer gegebenen Maschinenreihenfolge die Start- und Endzeiten
    aller Operationen neu. Dabei werden sowohl Job-Reihenfolgen als auch
    Maschinen-Reihenfolgen berücksichtigt.
    */
    public static class ScheduleRecalculator
    {
        /*
        Gibt true zurück, wenn alle Operationen zulässig eingeplant werden konnten.
        Falls durch die Reihenfolgen ein Zyklus entsteht, können nicht alle
        Operationen geplant werden und die Methode gibt false zurück.
        */
        public static bool Recalculate(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders,
            out int cmax)
        {
            cmax = 0;

            // Alle Operationen werden gemeinsam betrachtet, unabhängig von Job oder Maschine.
            List<Operation> allOperations =
                instance.Jobs.SelectMany(job => job.Operations).ToList();
            /*
           Zählt für jede Operation, wie viele Vorgänger noch nicht eingeplant wurden.
           Vorgänger entstehen aus der Job-Reihenfolge und aus der Maschinenreihenfolge.
           */
            Dictionary<Operation, int> openPredecessorCount =
                new Dictionary<Operation, int>();

            // Speichert die direkten Nachfolger jeder Operation für die spätere Freigabe.
            Dictionary<Operation, List<Operation>> directSuccessors =
                new Dictionary<Operation, List<Operation>>();

            foreach (Operation operation in allOperations)
            {
                openPredecessorCount[operation] = 0;
                directSuccessors[operation] = new List<Operation>();

                // Alte Zeiten werden gelöscht, da der Schedule vollständig neu berechnet wird.
                operation.StartTime = 0;
                operation.EndTime = 0;
            }

            // Fügt die festen Vorgängerbeziehungen innerhalb jedes Jobs hinzu.
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

            // Fügt die Vorgängerbeziehungen aus der aktuellen Maschinenreihenfolge hinzu.
            foreach (var pair in machineOrders)
            {
                List<Operation> operationsOnMachine = pair.Value;

                for (int i = 0; i < operationsOnMachine.Count - 1; i++)
                {
                    Operation before = operationsOnMachine[i];
                    Operation after = operationsOnMachine[i + 1];

                    directSuccessors[before].Add(after);
                    openPredecessorCount[after]++;
                }
            }
            // Operationen ohne offene Vorgänger können direkt eingeplant werden.
            Queue<Operation> readyOperations =
                new Queue<Operation>(
                    allOperations.Where(operation =>
                        openPredecessorCount[operation] == 0));

            int numberOfScheduledOperations = 0;
            /*
           Berechnet die früheste zulässige Startzeit der aktuellen Operation.
           Dabei werden der Job-Vorgänger, der Maschinen-Vorgänger und mögliche
           Setup-Zeiten berücksichtigt.
           */

            while (readyOperations.Count > 0)
            {
                Operation current = readyOperations.Dequeue();

                int earliestStart =
                    EarliestStartCalculator.Calculate(
                        instance,
                        machineOrders,
                        current);

                current.StartTime = earliestStart;
                current.EndTime =
                    current.StartTime + current.ProcessingTime;

                cmax = Math.Max(cmax, current.EndTime);
                numberOfScheduledOperations++;
                /*
                Nach dem Einplanen der aktuellen Operation wird sie als Vorgänger erledigt.
                Dadurch können ihre Nachfolger eventuell planbar werden.
                */
                foreach (Operation successor in directSuccessors[current])
                {
                    openPredecessorCount[successor]--;

                    if (openPredecessorCount[successor] == 0)
                    {
                        readyOperations.Enqueue(successor);
                    }
                }
            }
            /*
            Wenn nicht alle Operationen geplant wurden, enthält die Kombination aus
            Job-Reihenfolge und Maschinenreihenfolge einen Zyklus und ist unzulässig.
            */

            return numberOfScheduledOperations == allOperations.Count;
        }
    }
}