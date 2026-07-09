using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /*
    Führt eine schnelle Vorbewertung von Moves durch.

    Statt für jeden Move sofort den kompletten Schedule neu zu berechnen,
    wird hier mit den r_i- und q_i-Werten aus der kritischen Analyse eine
    Schätzung des möglichen Makespan-Beitrags vorgenommen.
    */
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
                analysisResult.ReleaseTimes[a];

            int rB =
                analysisResult.ReleaseTimes[b];

            int qA =
                analysisResult.Tails[a];

            int qB =
                analysisResult.Tails[b];

            int setupAB =
                GetSetupTime(instance, a, b);

            int setupBA =
                GetSetupTime(instance, b, a);

            /*
            Schätzung für den Fall, dass Operation a nach Operation b liegt.

            rB beschreibt, wann b frühestens starten kann. Danach folgen die
            Bearbeitungszeit von b, die Setup-Zeit von b nach a und der Restpfad
            ab a.
            */
            int estimatedAAfterB =
                rB + b.ProcessingTime + setupBA + qA;

            /*
            Schätzung für den umgekehrten Zusammenhang:
            Operation b liegt nach Operation a.
            */
            int estimatedBAfterA =
                rA + a.ProcessingTime + setupAB + qB;

            /*
            Der größere der beiden geschätzten Pfadbeiträge ist relevant,
            weil der Makespan durch den längsten Pfad bestimmt wird.
            */
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

            /*
            Die Strafrate steigt, wenn über längere Zeit keine Verbesserung
            gefunden wurde. Dadurch werden häufig wiederholte Moves stärker
            bestraft und die Suche wird stärker diversifiziert.
            */
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