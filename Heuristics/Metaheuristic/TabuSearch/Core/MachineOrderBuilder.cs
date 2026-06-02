using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    //Diese Klasse ist verantwortlich für den Aufbau der Reihenfolge der Operationen auf den Maschinen
    //basierend auf den Startzeiten der Operationen im aktuellen Zeitplan.
    public static class MachineOrderBuilder
    {
        //Diese Methode erstellt ein Dictionary, das für jede Maschine eine Liste von Operationen enthält,
        //die auf dieser Maschine ausgeführt werden, sortiert nach ihren Startzeiten.
        public static Dictionary<int, List<Operation>> Build(Instance instance)
        {
            Dictionary<int, List<Operation>> machineOrders =
                new Dictionary<int, List<Operation>>();
            //Iteriere über alle Jobs und deren Operationen, um die Operationen den entsprechenden Maschinen zuzuordnen.
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
            //Sortiere die Operationen auf jeder Maschine nach ihren Startzeiten, um die Reihenfolge der Ausführung zu bestimmen.
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