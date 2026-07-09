using System.Collections.Generic;
using System.Linq;
using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /*
    Leitet aus einem bereits geplanten Schedule die Bearbeitungsreihenfolge
    der Operationen auf jeder Maschine ab.
    */
    public static class MachineOrderBuilder
    {
        /*
        Erstellt für jede Maschine die Liste ihrer Operationen, sortiert nach
        der Startzeit im aktuellen Schedule.
        */
        public static Dictionary<int, List<Operation>> Build(Instance instance)
        {
            Dictionary<int, List<Operation>> machineOrders =
                new Dictionary<int, List<Operation>>();

            // Ordnet jede Operation der Maschine zu, auf der sie bearbeitet wird.
            foreach (Job job in instance.Jobs)
            {
                foreach (Operation operation in job.Operations)
                {
                    if (!machineOrders.ContainsKey(operation.Machine))
                    {
                        machineOrders[operation.Machine] = new List<Operation>();
                    }

                    machineOrders[operation.Machine].Add(operation);
                }
            }

            // Sortiert die Operationen je Maschine nach ihrer Startzeit.
            foreach (int machine in machineOrders.Keys.ToList())
            {
                machineOrders[machine] =
                    machineOrders[machine]
                    .OrderBy(operation => operation.StartTime)
                    .ToList();
            }

            return machineOrders;
        }
    }
}