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
        // Stores until which iteration a move remains tabu.
        private readonly Dictionary<string, int> tabuUntil;

        // Counts how often a move has already been executed.
        // This is long-term memory and is used for frequency penalties.
        private readonly Dictionary<string, int> moveFrequency;

        private readonly Random random;

        // Base value for the tabu tenure.
        private readonly int baseTenure;

        // Lower bound for the dynamic tenure.
        private readonly int minTenure;

        // Upper bound for the dynamic tenure.
        private readonly int maxTenure;

        // Defines after how many iterations the tenure is updated.
        private readonly int updateInterval;

        // Current tabu tenure used by the search.
        private int currentTenure;

        /// <summary>
        /// Creates a new tabu list.
        ///
        /// The base tenure is calculated from the instance size.
        /// The actual tenure is then randomly selected within a dynamic range.
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

            // Fixed seed for reproducible results.
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

            // The tenure is updated every 5% of the configured maximum iterations.
            updateInterval =
                Math.Max(
                    1,
                    maxIterations / 20);

            currentTenure =
                GenerateDynamicTenure();
        }

        /// <summary>
        /// Calculates the base tenure depending on the instance size.
        /// Larger instances receive a larger tabu tenure.
        /// </summary>
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

        /// <summary>
        /// Generates a new dynamic tabu tenure within the allowed tenure range.
        /// </summary>
        private int GenerateDynamicTenure()
        {
            return random.Next(
                minTenure,
                maxTenure + 1);
        }

        /// <summary>
        /// Updates the current tabu tenure if the update interval is reached.
        /// </summary>
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

        /// <summary>
        /// Checks whether a candidate move is currently tabu.
        ///
        /// Aspiration criterion:
        /// A tabu move is still allowed if it improves the global best solution.
        /// </summary>
        public bool IsTabu(
            Move move,
            int iteration,
            int candidateMakespan,
            int bestMakespan,
            int currentCmax)
        {
            // Aspiration criterion:
            // If the move creates a new global best solution, it is allowed.
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

        /// <summary>
        /// Registers an executed move.
        ///
        /// The reverse move is stored as tabu so that the algorithm cannot
        /// immediately undo the last move.
        /// </summary>
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
                moveFrequency[moveKey] = 0;
            }

            moveFrequency[moveKey]++;
        }

        /// <summary>
        /// Returns how often a move has already been executed.
        ///
        /// This value is used as a frequency penalty to discourage moves
        /// that have already been used very often.
        /// </summary>
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

        /// <summary>
        /// Clears only the short-term tabu memory.
        ///
        /// This is used during a restart. Old tabu restrictions should not block
        /// the new search trajectory. The long-term move frequencies are kept,
        /// so the algorithm still remembers which moves were used often before.
        /// </summary>
        public void ClearShortTermMemory()
        {
            tabuUntil.Clear();
        }

        /// <summary>
        /// Returns the currently used dynamic tabu tenure.
        /// </summary>
        public int CurrentTenure
        {
            get { return currentTenure; }
        }
    }
}