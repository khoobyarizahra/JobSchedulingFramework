using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /*
    Berechnet die früheste zulässige Startzeit einer Operation.
    Berücksichtigt werden Job-Vorgänger, Maschinen-Vorgänger und Setup-Zeiten.
    */
    public static class EarliestStartCalculator
    {
        public static int Calculate(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders,
            Operation operation)
        {
            int earliestStart = 0;

            Operation? jobPredecessor =
                OperationPredecessorFinder.GetJobPredecessor(
                    instance,
                    operation);

            if (jobPredecessor != null)
            {
                // Die Operation darf erst nach ihrem Vorgänger im selben Job starten.
                earliestStart =
                    Math.Max(
                        earliestStart,
                        jobPredecessor.EndTime);
            }

            Operation? machinePredecessor =
                OperationPredecessorFinder.GetMachinePredecessor(
                    machineOrders,
                    operation);

            if (machinePredecessor != null)
            {
                int setupTime =
                    instance.SetupTimes[
                        machinePredecessor.JobID - 1,
                        operation.JobID - 1];

                // Auf derselben Maschine muss zusätzlich die Setup-Zeit berücksichtigt werden.
                earliestStart =
                    Math.Max(
                        earliestStart,
                        machinePredecessor.EndTime + setupTime);
            }

            return earliestStart;
        }
    }
}