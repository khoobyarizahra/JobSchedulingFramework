using JobShopSchedulingFramework.Heuristics.Metaheuristics;
using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JobShopSchedulingFramework.Heuristics.Tabu
{
    public class TabuSearchSolver
    {
        private readonly int maxIterations;
        //tabuTenure gibt an, wie viele Iterationen ein Move tabu bleibt, nachdem er ausgeführt wurde.
        private readonly int tabuTenure;

        //in konstruktor werden die Parameter maxIterations und tabuTenure übergeben und in den entsprechenden Feldern gespeichert.
        public TabuSearchSolver(int maxIterations, int tabuTenure)
        {
            this.maxIterations = maxIterations;
            this.tabuTenure = tabuTenure;
        }
        //Die Run-Methode führt die Tabu Search aus. Sie nimmt eine Instanz des Job-Shop-Problems als Eingabe
        //und gibt die beste gefundene Cmax zurück.
        public int Run(Instance instance)

        {
            //stopwatch wird gestartet, um die Laufzeit der Tabu Search zu messen.
            Stopwatch stopwatch = Stopwatch.StartNew();
            //timeLimitSeconds gibt die maximale Laufzeit der Tabu Search in Sekunden an. In diesem Fall sind es 90 Sekunden.
            int timeLimitSeconds = 90;
            //iterationsWithoutImprovement zählt, wie viele Iterationen ohne Verbesserung des besten Cmax vergangen sind.
            //Wenn dieser Wert einen bestimmten Schwellenwert überschreitet, wird die Suche abgebrochen.
            int iterationsWithoutImprovement = 0;
            int maxIterationsWithoutImprovement = 20;
            //currentOrders speichert die aktuelle Reihenfolge der Operationen auf den Maschinen. Es ist ein Dictionary,
            //bei dem der Schlüssel die Maschinennummer und der Wert eine Liste von Operationen ist, die auf dieser Maschine ausgeführt werden.
            Dictionary<int, List<Operation>> currentOrders =
                ScheduleOrderHelper.BuildMachineOrders(instance);
            //currentCmax speichert den aktuellen Cmax-Wert der Lösung, die durch currentOrders repräsentiert wird.
            int currentCmax;
            // currentFeasible gibt an, ob die aktuelle Lösung (repräsentiert durch currentOrders) machbar ist.
            // Die Methode RecalculateScheduleFromMachineOrders berechnet den Cmax-Wert basierend auf der Reihenfolge der Operationen und überprüft gleichzeitig die Machbarkeit der Lösung.
            bool currentFeasible =
                ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                    instance,
                    currentOrders,
                    out currentCmax);

            if (!currentFeasible)
                throw new InvalidOperationException("Initial schedule is infeasible.");

            int bestCmax = currentCmax;

            Dictionary<int, List<Operation>> bestOrders =
                ScheduleOrderHelper.CopyMachineOrders(currentOrders);

            Dictionary<string, int> tabuList =
                new Dictionary<string, int>();

            Console.WriteLine();
            Console.WriteLine("TABU SEARCH");
            Console.WriteLine("Initial Cmax: " + currentCmax);

            for (int iteration = 1; iteration <= maxIterations; iteration++)
            {
                if (stopwatch.Elapsed.TotalSeconds >= timeLimitSeconds)
                {
                    Console.WriteLine("Time limit of 90 seconds reached. Stop.");
                    break;
                }

                RemoveExpiredTabuMoves(tabuList, iteration);

                List<Operation> criticalPath =
                    CriticalPathFinder.FindCriticalPath(instance, currentOrders);

                List<CriticalBlock> criticalBlocks =
                    CriticalPathFinder.ExtractCriticalBlocks(criticalPath);

                List<Move> moves =
                    NeighborhoodGenerator.GenerateAdjacentSwapMoves(criticalBlocks);

                if (moves.Count == 0)
                {
                    Console.WriteLine("No critical-block moves found. Stop.");
                    break;
                }

                Move? bestMoveThisIteration = null;
                int bestCandidateCmax = int.MaxValue;

                Dictionary<int, List<Operation>>? bestCandidateOrders = null;

                foreach (Move move in moves)
                {
                    Dictionary<int, List<Operation>> candidateOrders =
                        ScheduleOrderHelper.CopyMachineOrders(currentOrders);

                    ScheduleOrderHelper.ApplyMove(candidateOrders, move);

                    int candidateCmax;

                    bool feasible =
                        ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                            instance,
                            candidateOrders,
                            out candidateCmax);

                    if (!feasible)
                        continue;

                    bool isTabu =
                        tabuList.ContainsKey(move.GetKey());

                    bool aspiration =
                        candidateCmax < bestCmax;

                    if (isTabu && !aspiration)
                        continue;

                    if (candidateCmax < bestCandidateCmax)
                    {
                        bestCandidateCmax = candidateCmax;
                        bestMoveThisIteration = move;
                        bestCandidateOrders = candidateOrders;
                    }
                }

                if (bestMoveThisIteration == null || bestCandidateOrders == null)
                {
                    Console.WriteLine("No admissible move found. Stop.");
                    break;
                }

                currentOrders = bestCandidateOrders;

                ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                    instance,
                    currentOrders,
                    out currentCmax);

                tabuList[bestMoveThisIteration.GetReverseKey()] =
                    iteration + tabuTenure;

                if (currentCmax < bestCmax)
                {
                    bestCmax = currentCmax;

                    bestOrders =
                        ScheduleOrderHelper.CopyMachineOrders(currentOrders);

                    iterationsWithoutImprovement = 0;
                }
                else
                {
                    iterationsWithoutImprovement++;
                }

                Console.WriteLine(
                    "Iteration " + iteration +
                    " | Move: " + bestMoveThisIteration +
                    " | Current Cmax: " + currentCmax +
                    " | Best Cmax: " + bestCmax);

                if (iterationsWithoutImprovement >= maxIterationsWithoutImprovement)
                {
                    Console.WriteLine(
                        "No improvement for " +
                        maxIterationsWithoutImprovement +
                        " iterations. Stop.");

                    break;
                }
            }

            ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                instance,
                bestOrders,
                out bestCmax);

            Console.WriteLine("Final best Cmax after Tabu Search: " + bestCmax);

            return bestCmax;
        }

        private void RemoveExpiredTabuMoves(
            Dictionary<string, int> tabuList,
            int currentIteration)
        {
            List<string> expiredKeys = tabuList
                .Where(pair => pair.Value <= currentIteration)
                .Select(pair => pair.Key)
                .ToList();

            foreach (string key in expiredKeys)
            {
                tabuList.Remove(key);
            }
        }
    }
}