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
        private readonly int maxIterations;
        private readonly int timeLimitSeconds;
        private readonly INeighborhoodDefinition neighborhood;

        private const int MaxExactEvaluationsPerIteration = 20;
        private const bool VerboseOutput = false;
        private const int VerbosePrintInterval = 1000;

        // Restart is triggered after operationCount * factor iterations without global improvement.
        private const int RestartAfterNoImprovementFactor = 25;

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

        public int Run(Instance instance)
        {
            Stopwatch stopwatch =
                Stopwatch.StartNew();

            bool useTimeLimit =
                timeLimitSeconds > 0;

            int operationCount =
                instance.Jobs.Sum(job => job.Operations.Count);

            int restartAfterNoImprovement =
                operationCount * RestartAfterNoImprovementFactor;

            int restartPerturbationMoves =
                Math.Max(
                    3,
                    operationCount / 20);

            Console.WriteLine(
                "Restart threshold: " +
                restartAfterNoImprovement);

            Console.WriteLine(
                "Restart perturbation moves: " +
                restartPerturbationMoves);

            Dictionary<int, List<Operation>> currentOrders =
                ScheduleOrderHelper.BuildMachineOrders(instance);

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

            int initialCmax =
                currentCmax;

            int bestCmax =
                currentCmax;

            Dictionary<int, List<Operation>> bestOrders =
                ScheduleOrderHelper.CopyMachineOrders(
                    currentOrders);

            MoveTabuList tabuList =
                new MoveTabuList(
                    instance.NumJobs,
                    instance.NumMachines,
                    maxIterations);

            int iteration =
                0;

            int iterationsSinceImprovement =
                0;

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

                tabuList.UpdateTenureIfNeeded(
                    iteration);

                CriticalOperationAnalysisResult analysisResult =
                    CriticalOperationAnalyzer.Analyze(
                        instance,
                        currentOrders);

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

                    int frequencyPenalty =
                        tabuList.GetFrequencyPenalty(
                            move);

                    double penaltyRate =
                        GetPenaltyRate(
                            iterationsSinceImprovement);

                    double candidateEvaluationValue =
                        candidateCmax +
                        candidateCmax * penaltyRate * frequencyPenalty;

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

                currentOrders =
                    bestCandidateOrders;

                currentCmax =
                    bestCandidateCmax;

                tabuList.RegisterMove(
                    bestMove,
                    iteration);

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

                if (iterationsSinceImprovement >= restartAfterNoImprovement)
                {
                    restartCount++;

                    currentOrders =
                        RestartFromBestSolution(
                            instance,
                            bestOrders,
                            restartPerturbationMoves,
                            out currentCmax);

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

        private static bool ShouldPrintIterationDetails(
            int iteration)
        {
            return iteration <= 10 ||
                   iteration % VerbosePrintInterval == 0;
        }

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

        private Dictionary<int, List<Operation>> RestartFromBestSolution(
            Instance instance,
            Dictionary<int, List<Operation>> bestOrders,
            int restartPerturbationMoves,
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
                    restartPerturbationMoves);

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

            Dictionary<int, List<Operation>> fallbackOrders =
                ScheduleOrderHelper.CopyMachineOrders(
                    bestOrders);

            ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                instance,
                fallbackOrders,
                out restartCmax);

            return fallbackOrders;
        }

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