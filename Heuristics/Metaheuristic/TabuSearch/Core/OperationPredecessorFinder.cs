using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    // Helper class for finding predecessor operations based on job order and machine order.
    public static class OperationPredecessorFinder
    {
        // Returns the predecessor operation in the same job.
        // If the operation is the first operation of the job, null is returned.
        public static Operation? GetJobPredecessor(
            Instance instance,
            Operation operation)
        {
            Job job =
                instance.Jobs.First(job =>
                    job.JobID == operation.JobID);

            int index =
                job.Operations.FindIndex(candidate =>
                    candidate.OperationID == operation.OperationID);

            if (index <= 0)
            {
                return null;
            }

            return job.Operations[index - 1];
        }

        // Returns the predecessor operation on the same machine.
        // If the operation is the first operation on the machine, null is returned.
        public static Operation? GetMachinePredecessor(
            Dictionary<int, List<Operation>> machineOrders,
            Operation operation)
        {
            List<Operation> operationsOnMachine =
                machineOrders[operation.Machine];

            int index =
                operationsOnMachine.IndexOf(operation);

            if (index <= 0)
            {
                return null;
            }

            return operationsOnMachine[index - 1];
        }
    }
}