using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    public static class MoveFastEvaluator
    {
        public static int EstimateSwapCmax(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders,
            CriticalOperationAnalysisResult analysisResult,
            Move move)
        {
            List<Operation> machineSequence =
                machineOrders[move.Machine];

            Operation a =
                machineSequence[move.MachineIndex1];

            Operation b =
                machineSequence[move.MachineIndex2];

            int rA =
                analysisResult.releaseTimes[a];

            int rB =
                analysisResult.releaseTimes[b];

            int qA =
                analysisResult.tails[a];

            int qB =
                analysisResult.tails[b];

            int setupAB =
                GetSetupTime(instance, a, b);

            int setupBA =
                GetSetupTime(instance, b, a);

            int estimatedAAfterB =
                rB + b.ProcessingTime + setupBA + qA;

            int estimatedBAfterA =
                rA + a.ProcessingTime + setupAB + qB;

            return Math.Max(
                estimatedAAfterB,
                estimatedBAfterA);
        }

        public static double EstimateEvaluationValue(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders,
            CriticalOperationAnalysisResult analysisResult,
            Move move,
            MoveTabuList tabuList,
            int iterationsSinceImprovement)
        {
            int estimatedCmax =
                EstimateSwapCmax(
                    instance,
                    machineOrders,
                    analysisResult,
                    move);

            int frequencyPenalty =
                tabuList.GetFrequencyPenalty(move);

            double penaltyRate;

            if (iterationsSinceImprovement < 300)
            {
                penaltyRate = 0.005;
            }
            else if (iterationsSinceImprovement < 1000)
            {
                penaltyRate = 0.015;
            }
            else
            {
                penaltyRate = 0.03;
            }

            return estimatedCmax +
                   estimatedCmax * penaltyRate * frequencyPenalty;
        }

        private static int GetSetupTime(
            Instance instance,
            Operation before,
            Operation after)
        {
            return instance.SetupTimes[
                before.JobID - 1,
                after.JobID - 1];
        }
    }
}