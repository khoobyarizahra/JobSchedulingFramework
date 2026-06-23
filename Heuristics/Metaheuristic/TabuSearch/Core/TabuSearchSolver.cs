using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods;
using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /// <summary>
    /// Executes the Tabu Search algorithm for the Job Shop Scheduling Problem.
    ///
    /// The solver starts from an existing machine order, generates neighborhood moves,
    /// evaluates promising candidates, applies tabu restrictions, and stores the best
    /// schedule found during the search.
    ///
    /// A fast move preselection is used before the exact schedule recalculation:
    /// all generated moves are first estimated cheaply, and only the most promising
    /// moves are evaluated exactly. This reduces the computational effort per iteration.
    /// </summary>
    public class TabuSearchSolver
    {
        // Maximum number of iterations used when no time limit is active.
        private readonly int maxIterations;

        // Time limit in seconds.
        // If this value is greater than zero, the time limit controls the search.
        // If this value is zero, the solver runs without a fixed time limit
        // and stops only when maxIterations is reached.
        private readonly int timeLimitSeconds;

        // Neighborhood structure used to generate moves.
        private readonly INeighborhoodDefinition neighborhood;

        // Number of moves that are evaluated exactly after the fast preselection.
        private const int MaxExactEvaluationsPerIteration = 20;

        // Controls how much information is printed during the search.
        // false = compact final output
        // true  = detailed debugging output
        private const bool VerboseOutput = false;

        // In verbose mode, detailed information is printed only every n iterations.
        private const int VerbosePrintInterval = 1000;

        // Restart is triggered after a long phase without improving the global best solution.
        private const int RestartAfterNoImprovement = 8000;

        // Number of random swaps used to perturb the best known solution during a restart.
        private const int RestartPerturbationMoves = 30;

        // Fixed seed for reproducible restart perturbations.
        private readonly Random restartRandom = new Random(123);

        public TabuSearchSolver(
            int maxIterations,
            int timeLimitSeconds,
            INeighborhoodDefinition neighborhoodDefinition)
        {
            this.maxIterations = maxIterations;
            this.timeLimitSeconds = timeLimitSeconds;
            neighborhood = neighborhoodDefinition;
        }

        /// <summary>
        /// Runs the Tabu Search on the given instance and returns the best makespan found.
        /// </summary>
        public int Run(Instance instance)
        {
            Stopwatch stopwatch =
                Stopwatch.StartNew();

            bool useTimeLimit =
                timeLimitSeconds > 0;

            // Build the initial machine orders from the current schedule stored in the instance.
            Dictionary<int, List<Operation>> currentOrders =
                ScheduleOrderHelper.BuildMachineOrders(instance);

            // Recalculate the initial schedule to ensure that the starting solution is feasible.
            bool initialFeasible =
                ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                    instance,
                    currentOrders,
                    out int currentCmax);

            if (!initialFeasible)
            {
                throw new InvalidOperationException(
                    "Initial schedule infeasible.");
            }

            // The initial solution is the best known solution at the beginning.
            int initialCmax =
                currentCmax;

            int bestCmax =
                currentCmax;

            Dictionary<int, List<Operation>> bestOrders =
                ScheduleOrderHelper.CopyMachineOrders(
                    currentOrders);

            // The tabu list stores forbidden moves and long-term frequency information.
            MoveTabuList tabuList =
                new MoveTabuList(
                    instance.NumJobs,
                    instance.NumMachines,
                    maxIterations);

            int iteration =
                0;

            // Counts how many iterations have passed since the last global improvement.
            // This value is used for restart decisions and for the dynamic frequency penalty.
            int iterationsSinceImprovement =
                0;

            // Counts all performed restarts.
            int restartCount =
                0;

            if (VerboseOutput)
            {
                Console.WriteLine();
                Console.WriteLine("TABU SEARCH PROCESS");
                Console.WriteLine("Initial Cmax: " + currentCmax);
                Console.WriteLine();

                Console.WriteLine(
                    "Iter".PadRight(8) + " | " +
                    "Current".PadRight(10) + " | " +
                    "Best".PadRight(10) + " | " +
                    "Tenure".PadRight(8) + " | " +
                    "Move");

                Console.WriteLine(new string('-', 95));
            }

            while (ShouldContinueSearch(
                useTimeLimit,
                stopwatch,
                iteration))
            {
                iteration++;

                // Update the dynamic tabu tenure if the configured update interval is reached.
                tabuList.UpdateTenureIfNeeded(
                    iteration);

                // Analyze the current solution and compute critical operations.
                CriticalOperationAnalysisResult analysisResult =
                    CriticalOperationAnalyzer.Analyze(
                        instance,
                        currentOrders);

                // Group critical operations into critical blocks on the machines.
                List<CriticalBlock> criticalBlocks =
                    CriticalBlockBuilder.BuildCriticalBlocks(
                        currentOrders,
                        analysisResult.criticalOperations);

                if (VerboseOutput &&
                    iteration <= 10)
                {
                    Console.WriteLine(
                        "Critical operations: " +
                        analysisResult.criticalOperations.Count);
                }

                // Generate all neighborhood moves based on the selected neighborhood definition.
                List<Move> moves =
                    neighborhood.GenerateMoves(
                        instance,
                        currentOrders,
                        criticalBlocks);

                if (VerboseOutput &&
                    ShouldPrintIterationDetails(iteration))
                {
                    Console.WriteLine(
                        "Iteration " + iteration +
                        " | Neighborhood: " + neighborhood.GetType().Name +
                        " | Critical blocks: " + criticalBlocks.Count +
                        " | Generated moves: " + moves.Count);
                }

                if (moves.Count == 0)
                {
                    if (VerboseOutput)
                    {
                        Console.WriteLine("No neighborhood moves found.");
                    }

                    break;
                }

                // Fast preselection:
                // Instead of recalculating a complete schedule for every generated move,
                // each move is first estimated cheaply using release/tail information.
                // Only the most promising moves are evaluated exactly afterwards.
                int exactEvaluationLimit =
                    Math.Min(
                        MaxExactEvaluationsPerIteration,
                        moves.Count);

                List<Move> promisingMoves =
                    moves
                        .OrderBy(move =>
                            MoveFastEvaluator.EstimateEvaluationValue(
                                instance,
                                currentOrders,
                                analysisResult,
                                move,
                                tabuList,
                                iterationsSinceImprovement))
                        .Take(exactEvaluationLimit)
                        .ToList();

                if (VerboseOutput &&
                    ShouldPrintIterationDetails(iteration))
                {
                    Console.WriteLine(
                        "Exactly evaluated moves: " +
                        promisingMoves.Count);
                }

                Move bestMove =
                    null;

                int bestCandidateCmax =
                    int.MaxValue;

                double bestCandidateEvaluationValue =
                    double.MaxValue;

                Dictionary<int, List<Operation>> bestCandidateOrders =
                    null;

                // Exact evaluation:
                // Only the preselected moves are applied to a copied machine order
                // and evaluated by recalculating the full schedule.
                foreach (Move move in promisingMoves)
                {
                    Dictionary<int, List<Operation>> candidateOrders =
                        ScheduleOrderHelper.CopyMachineOrders(
                            currentOrders);

                    ApplyMove(
                        candidateOrders,
                        move);

                    bool candidateFeasible =
                        ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                            instance,
                            candidateOrders,
                            out int candidateCmax);

                    if (!candidateFeasible)
                    {
                        continue;
                    }

                    // Tabu check:
                    // A tabu move is skipped unless the aspiration criterion allows it.
                    bool isTabu =
                        tabuList.IsTabu(
                            move,
                            iteration,
                            candidateCmax,
                            bestCmax,
                            currentCmax);

                    if (isTabu)
                    {
                        continue;
                    }

                    // Long-term memory:
                    // Moves that have been used frequently receive a penalty.
                    // The penalty becomes stronger if the search stagnates.
                    int frequencyPenalty =
                        tabuList.GetFrequencyPenalty(
                            move);

                    double penaltyRate =
                        GetPenaltyRate(
                            iterationsSinceImprovement);

                    double candidateEvaluationValue =
                        candidateCmax +
                        candidateCmax * penaltyRate * frequencyPenalty;

                    // Select the admissible candidate with the best penalized evaluation value.
                    if (candidateEvaluationValue < bestCandidateEvaluationValue)
                    {
                        bestCandidateEvaluationValue =
                            candidateEvaluationValue;

                        bestCandidateCmax =
                            candidateCmax;

                        bestMove =
                            move;

                        bestCandidateOrders =
                            candidateOrders;
                    }
                }

                if (VerboseOutput &&
                    ShouldPrintIterationDetails(iteration))
                {
                    Console.WriteLine(
                        "Best candidate Cmax: " +
                        bestCandidateCmax);
                }

                // If no admissible move was found, the search continues with the same solution.
                if (bestMove == null ||
                    bestCandidateOrders == null)
                {
                    if (VerboseOutput)
                    {
                        Console.WriteLine("No admissible move found.");
                    }

                    iterationsSinceImprovement++;
                    continue;
                }

                // Move to the selected neighboring solution.
                currentOrders =
                    bestCandidateOrders;

                currentCmax =
                    bestCandidateCmax;

                // Register the performed move in the tabu list.
                tabuList.RegisterMove(
                    bestMove,
                    iteration);

                // Update the global best solution if the current solution improved it.
                if (currentCmax < bestCmax)
                {
                    bestCmax =
                        currentCmax;

                    bestOrders =
                        ScheduleOrderHelper.CopyMachineOrders(
                            currentOrders);

                    iterationsSinceImprovement =
                        0;
                }
                else
                {
                    iterationsSinceImprovement++;
                }

                // Restart / diversification:
                // If the global best solution has not improved for a long time,
                // the current search trajectory is restarted from a perturbed version
                // of the best known solution.
                if (iterationsSinceImprovement >= RestartAfterNoImprovement)
                {
                    restartCount++;

                    currentOrders =
                        RestartFromBestSolution(
                            instance,
                            bestOrders,
                            out currentCmax);

                    // Clear only the short-term tabu restrictions.
                    // The long-term move frequencies remain stored in the tabu list.
                    tabuList.ClearShortTermMemory();

                    iterationsSinceImprovement =
                        0;

                    if (VerboseOutput)
                    {
                        Console.WriteLine();
                        Console.WriteLine(
                            "Restart triggered at iteration " +
                            iteration +
                            " | Restart Cmax: " +
                            currentCmax +
                            " | Global best remains: " +
                            bestCmax +
                            " | Restart count: " +
                            restartCount);
                        Console.WriteLine();
                    }
                }

                if (VerboseOutput &&
                    ShouldPrintIterationDetails(iteration))
                {
                    Console.WriteLine(
                        iteration.ToString().PadRight(8) + " | " +
                        currentCmax.ToString().PadRight(10) + " | " +
                        bestCmax.ToString().PadRight(10) + " | " +
                        tabuList.CurrentTenure.ToString().PadRight(8) + " | " +
                        bestMove);
                }
            }

            stopwatch.Stop();

            // Recalculate the best schedule once more so that all operation start and end times
            // are consistent with the stored best machine order.
            ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                instance,
                bestOrders,
                out bestCmax);

            PrintCompactSummary(
                useTimeLimit,
                initialCmax,
                bestCmax,
                iteration,
                restartCount,
                stopwatch.Elapsed);

            if (VerboseOutput)
            {
                PrintMachineOrder(
                    bestOrders);
            }

            return bestCmax;
        }

        /// <summary>
        /// Decides whether the search should continue.
        ///
        /// In the 90-second mode, the time limit controls the loop.
        /// In the extended mode, no fixed time limit is used.
        /// The search then stops only when the maximum number of iterations is reached.
        /// </summary>
        private bool ShouldContinueSearch(
            bool useTimeLimit,
            Stopwatch stopwatch,
            int iteration)
        {
            if (useTimeLimit)
            {
                return stopwatch.Elapsed.TotalSeconds < timeLimitSeconds;
            }

            return iteration < maxIterations;
        }

        /// <summary>
        /// Defines when detailed iteration information should be printed in verbose mode.
        /// </summary>
        private static bool ShouldPrintIterationDetails(
            int iteration)
        {
            return iteration <= 10 ||
                   iteration % VerbosePrintInterval == 0;
        }

        /// <summary>
        /// Returns the penalty rate for the long-term frequency memory.
        /// The longer the search has not improved the global best solution,
        /// the stronger the diversification pressure becomes.
        /// </summary>
        private static double GetPenaltyRate(
            int iterationsSinceImprovement)
        {
            if (iterationsSinceImprovement < 300)
            {
                return 0.005;
            }

            if (iterationsSinceImprovement < 1000)
            {
                return 0.015;
            }

            return 0.03;
        }

        /// <summary>
        /// Creates a restart solution from the best known machine order.
        ///
        /// The method copies the best known solution and applies a number of
        /// random swaps on machine sequences. This keeps the restart close to a
        /// good solution but moves the search to a different region of the search space.
        ///
        /// If the perturbed solution is infeasible, the method retries with a new
        /// perturbation. If no feasible perturbation is found, it falls back to the
        /// unchanged best known solution.
        /// </summary>
        private Dictionary<int, List<Operation>> RestartFromBestSolution(
            Instance instance,
            Dictionary<int, List<Operation>> bestOrders,
            out int restartCmax)
        {
            const int maxRestartAttempts = 20;

            for (int attempt = 1; attempt <= maxRestartAttempts; attempt++)
            {
                Dictionary<int, List<Operation>> restartOrders =
                    ScheduleOrderHelper.CopyMachineOrders(
                        bestOrders);

                ApplyRandomPerturbation(
                    restartOrders,
                    RestartPerturbationMoves);

                bool feasible =
                    ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                        instance,
                        restartOrders,
                        out restartCmax);

                if (feasible)
                {
                    return restartOrders;
                }
            }

            // Fallback:
            // If all perturbation attempts fail, continue from the best known solution.
            Dictionary<int, List<Operation>> fallbackOrders =
                ScheduleOrderHelper.CopyMachineOrders(
                    bestOrders);

            ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                instance,
                fallbackOrders,
                out restartCmax);

            return fallbackOrders;
        }

        /// <summary>
        /// Applies random swaps inside machine sequences.
        ///
        /// Only machines with at least two operations are considered.
        /// The swaps are deliberately simple because the schedule feasibility
        /// is checked afterwards by RecalculateScheduleFromMachineOrders.
        /// </summary>
        private void ApplyRandomPerturbation(
            Dictionary<int, List<Operation>> machineOrders,
            int numberOfSwaps)
        {
            List<int> usableMachines =
                machineOrders
                    .Where(pair => pair.Value.Count >= 2)
                    .Select(pair => pair.Key)
                    .ToList();

            if (usableMachines.Count == 0)
            {
                return;
            }

            for (int i = 0; i < numberOfSwaps; i++)
            {
                int machine =
                    usableMachines[
                        restartRandom.Next(usableMachines.Count)];

                List<Operation> operationsOnMachine =
                    machineOrders[machine];

                int firstIndex =
                    restartRandom.Next(operationsOnMachine.Count);

                int secondIndex =
                    restartRandom.Next(operationsOnMachine.Count);

                if (firstIndex == secondIndex)
                {
                    continue;
                }

                Operation temp =
                    operationsOnMachine[firstIndex];

                operationsOnMachine[firstIndex] =
                    operationsOnMachine[secondIndex];

                operationsOnMachine[secondIndex] =
                    temp;
            }
        }

        /// <summary>
        /// Applies a move to a machine order.
        ///
        /// For swap moves, two operations on the same machine are exchanged.
        /// For insert moves, one operation is removed and inserted at another position.
        /// </summary>
        private void ApplyMove(
            Dictionary<int, List<Operation>> machineOrders,
            Move move)
        {
            List<Operation> operationsOnMachine =
                machineOrders[move.Machine];

            if (move.IsInsertMove)
            {
                Operation movedOperation =
                    operationsOnMachine[move.MachineIndex1];

                operationsOnMachine.RemoveAt(
                    move.MachineIndex1);

                int targetIndex =
                    move.MachineIndex2;

                // After removing an operation before the target position,
                // the target index must be shifted by one.
                if (move.MachineIndex1 < move.MachineIndex2)
                {
                    targetIndex--;
                }

                if (targetIndex < 0)
                {
                    targetIndex =
                        0;
                }

                if (targetIndex > operationsOnMachine.Count)
                {
                    targetIndex =
                        operationsOnMachine.Count;
                }

                operationsOnMachine.Insert(
                    targetIndex,
                    movedOperation);

                return;
            }

            Operation temp =
                operationsOnMachine[move.MachineIndex1];

            operationsOnMachine[move.MachineIndex1] =
                operationsOnMachine[move.MachineIndex2];

            operationsOnMachine[move.MachineIndex2] =
                temp;
        }

        /// <summary>
        /// Prints a compact final summary.
        /// This is the normal output used for the final evaluation.
        /// </summary>
        private static void PrintCompactSummary(
            bool useTimeLimit,
            int initialCmax,
            int bestCmax,
            int iterations,
            int restartCount,
            TimeSpan runtime)
        {
            double improvementPercent =
                initialCmax > 0
                    ? (double)(initialCmax - bestCmax) / initialCmax * 100.0
                    : 0.0;

            Console.WriteLine();
            Console.WriteLine("Tabu Search finished.");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("Mode: " + (useTimeLimit ? "90s time limit" : "Extended without fixed time limit"));
            Console.WriteLine("Initial Cmax: " + initialCmax);
            Console.WriteLine("Best Cmax: " + bestCmax);
            Console.WriteLine("Improvement: " + improvementPercent.ToString("F2") + "%");
            Console.WriteLine("Iterations: " + iterations);
            Console.WriteLine("Restarts: " + restartCount);
            Console.WriteLine("Runtime: " + runtime.TotalSeconds.ToString("F2") + " s");
            Console.WriteLine("---------------------------------------");
        }

        /// <summary>
        /// Prints the final machine order of the best solution.
        /// This output is useful for debugging and for checking the final sequence manually.
        /// It is only printed when VerboseOutput is enabled.
        /// </summary>
        private void PrintMachineOrder(
            Dictionary<int, List<Operation>> machineOrders)
        {
            Console.WriteLine();
            Console.WriteLine("TABU MACHINE ORDER");
            Console.WriteLine("--------------------------------");

            foreach (var pair in machineOrders.OrderBy(pair => pair.Key))
            {
                Console.Write("Machine " + pair.Key + ": ");

                Console.WriteLine(
                    string.Join(
                        " -> ",
                        pair.Value.Select(operation =>
                            "J" +
                            operation.JobID +
                            "O" +
                            operation.OperationID)));
            }

            Console.WriteLine();
        }
    }
}