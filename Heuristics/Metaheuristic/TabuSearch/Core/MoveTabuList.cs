using System;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /// <summary>
    /// Manages the tabu list for the Tabu Search.
    ///
    /// When a move is executed, its reverse move is stored as tabu.
    /// This prevents the search from immediately returning to the previous solution.
    ///
    /// The tabu tenure is calculated based on the instance size and updated dynamically
    /// during the search. In addition, move frequencies are counted and can be used
    /// as long-term memory for diversification.
    /// </summary>
    public class MoveTabuList
    {
        private readonly Dictionary<string, int> tabuUntil;
        private readonly Dictionary<string, int> moveFrequency;
        private readonly Random random;

        private readonly int baseTenure;
        private readonly int minTenure;
        private readonly int maxTenure;
        private readonly int updateInterval;

        private int currentTenure;

        /// <summary>
        /// Creates a new tabu list.
        ///
        /// Important:
        /// maxIterations is intentionally not used for the tenure update interval.
        /// Otherwise, two runs of the same algorithm with different stopping criteria
        /// would follow different search trajectories.
        /// </summary>
        public MoveTabuList(
            int numberOfJobs,
            int numberOfMachines,
            int maxIterations)
        {
            tabuUntil =
                new Dictionary<string, int>();

            moveFrequency =
                new Dictionary<string, int>();

            random =
                new Random(42);

            baseTenure =
                CalculateBaseTenure(
                    numberOfJobs,
                    numberOfMachines);

            minTenure =
                Math.Max(
                    1,
                    (int)Math.Round(baseTenure * 0.8));

            maxTenure =
                Math.Max(
                    minTenure + 1,
                    (int)Math.Round(baseTenure * 1.2));

            updateInterval =
                CalculateTenureUpdateInterval(
                    numberOfJobs,
                    numberOfMachines);

            currentTenure =
                GenerateDynamicTenure();
        }

        private int CalculateBaseTenure(
            int numberOfJobs,
            int numberOfMachines)
        {
            int problemSize =
                numberOfJobs + numberOfMachines;

            return Math.Max(
                3,
                problemSize / 2);
        }

        private int CalculateTenureUpdateInterval(
            int numberOfJobs,
            int numberOfMachines)
        {
            int estimatedOperationCount =
                numberOfJobs * numberOfMachines;

            return Math.Max(
                1000,
                estimatedOperationCount * 5);
        }

        private int GenerateDynamicTenure()
        {
            return random.Next(
                minTenure,
                maxTenure + 1);
        }

        public void UpdateTenureIfNeeded(
            int iteration)
        {
            if (iteration > 0 &&
                iteration % updateInterval == 0)
            {
                currentTenure =
                    GenerateDynamicTenure();
            }
        }

        public bool IsTabu(
            Move move,
            int iteration,
            int candidateMakespan,
            int bestMakespan,
            int currentCmax)
        {
            if (candidateMakespan < bestMakespan)
            {
                return false;
            }

            string moveKey =
                move.GetKey();

            if (!tabuUntil.ContainsKey(moveKey))
            {
                return false;
            }

            return iteration <
                   tabuUntil[moveKey];
        }

        public void RegisterMove(
            Move move,
            int iteration)
        {
            string reverseKey =
                move.GetReverseKey();

            tabuUntil[reverseKey] =
                iteration + currentTenure;

            string moveKey =
                move.GetKey();

            if (!moveFrequency.ContainsKey(moveKey))
            {
                moveFrequency[moveKey] =
                    0;
            }

            moveFrequency[moveKey]++;
        }

        public int GetFrequencyPenalty(
            Move move)
        {
            string moveKey =
                move.GetKey();

            if (!moveFrequency.ContainsKey(moveKey))
            {
                return 0;
            }

            return moveFrequency[moveKey];
        }

        public void ClearShortTermMemory()
        {
            tabuUntil.Clear();
        }

        public int CurrentTenure
        {
            get { return currentTenure; }
        }
    }
}