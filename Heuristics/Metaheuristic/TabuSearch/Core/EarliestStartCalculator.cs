using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    // Hilfsklasse zur Berechnung der frühesten Startzeit einer Operation basierend auf den Job- und Maschinenabhängigkeiten.
    public static class EarliestStartCalculator
    {
        //Die Methode berechnet die früheste Startzeit für eine gegebene Operation unter Berücksichtigung der Vorgängeroperationen im Job und auf der Maschine.
        //Als Eingabe erhält sie die Instanz des Problems, die aktuellen Maschinenbelegungen (machineOrders) und die zu planende Operation.
        //Die Methode gibt die früheste Startzeit zurück, zu der die gegebene Operation beginnen kann, ohne die Abhängigkeiten zu verletzen.
        public static int Calculate(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders,
            Operation operation)
        {
            // Initialisieren der frühesten Startzeit mit 0.
            int earliestStart = 0;
            // Bestimmen des Vorgängers der Operation in der Jobreihenfolge.
            Operation jobPredecessor =
                OperationPredecessorFinder.GetJobPredecessor(instance, operation);
            // Wenn ein Vorgänger in der Jobreihenfolge existiert, aktualisieren der frühesten Startzeit basierend auf dessen Endzeit.
            if (jobPredecessor != null)
            {
                // Die früheste Startzeit muss mindestens die Endzeit des Vorgängers sein, um die Reihenfolge der Operationen im Job einzuhalten.
                earliestStart =
                    Math.Max(earliestStart, jobPredecessor.EndTime);
            }
            // Bestimmen des Vorgängers der Operation auf der Maschine.
            Operation machinePredecessor =
                OperationPredecessorFinder.GetMachinePredecessor(machineOrders, operation);
            // Wenn ein Vorgänger auf der Maschine existiert, aktualisieren der frühesten Startzeit basierend auf dessen Endzeit und der erforderlichen Rüstzeit.
            if (machinePredecessor != null)
            {
                // Die früheste Startzeit muss mindestens die Endzeit des Vorgängers auf der Maschine plus der Rüstzeit sein, um die Maschinenbelegung zu berücksichtigen.
                int setupTime =
                    instance.SetupTimes[
                        machinePredecessor.JobID - 1,
                        operation.JobID - 1];
                earliestStart =
                    Math.Max(
                        earliestStart,
                        machinePredecessor.EndTime + setupTime);
            }

            return earliestStart;
        }
    }
}