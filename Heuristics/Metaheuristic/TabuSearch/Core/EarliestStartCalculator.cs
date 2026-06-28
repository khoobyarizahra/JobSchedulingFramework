using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    // Helper class for calculating the earliest feasible start time of an operation.
    public static class EarliestStartCalculator
    {
        // Calculates the earliest start time of an operation based on
        // the predecessor in the same job and the predecessor on the same machine.
        public static int Calculate(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders,
            Operation operation)
        {
            int earliestStart =
                0;

            Operation? jobPredecessor =
                OperationPredecessorFinder.GetJobPredecessor(
                    instance,
                    operation);

            if (jobPredecessor != null)
            {
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

                earliestStart =
                    Math.Max(
                        earliestStart,
                        machinePredecessor.EndTime + setupTime);
            }

            return earliestStart;
        }
    }
}