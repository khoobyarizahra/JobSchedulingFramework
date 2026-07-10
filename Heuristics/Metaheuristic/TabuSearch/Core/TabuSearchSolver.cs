using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Evaluation;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Restart;
using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /*
    Führt die Tabu Search für das Job-Shop-Scheduling-Problem aus.

    Die Suche arbeitet auf Maschinenreihenfolgen:
    Start- und Endzeiten werden nicht direkt verändert. Stattdessen wird die
    Reihenfolge der Operationen auf Maschinen angepasst. Nach jedem ausgewählten
    Move wird aus dieser Reihenfolge wieder ein vollständiger Schedule berechnet.

    Ablauf pro Iteration:
    - kritische Operationen bestimmen,
    - kritische Blöcke bilden,
    - Nachbarschaftsmoves erzeugen,
    - Moves für die exakte Bewertung auswählen,
    - ausgewählte Moves exakt bewerten,
    - Tabu-Regeln und Aspiration anwenden,
    - besten zulässigen Move übernehmen,
    - globale Bestlösung speichern,
    - bei Stagnation optional Restart ausführen.

    Über MoveSelectionMode kann experimentell untersucht werden, ob die schnelle
    Move-Abschätzung hilfreich ist oder ob bessere Ergebnisse entstehen, wenn sie
    vollständig deaktiviert wird.
    */
    public class TabuSearchSolver
    {
        private readonly TabuSearchSettings settings;
        private readonly INeighborhoodDefinition neighborhood;
        private readonly Random randomMoveSelector;

        public TabuSearchSolver(
        int maxIterations,
        int timeLimitSeconds,
        INeighborhoodDefinition neighborhoodDefinition)
        : this(
          TabuSearchSettings.CreateAdaptiveDefault(
              maxIterations,
              timeLimitSeconds),
          neighborhoodDefinition)
        {
            /*
            Dieser Konstruktor bleibt erhalten, damit bestehende Aufrufe im Projekt
            nicht angepasst werden müssen.

            Nach der Evaluation verwendet der normale Solver jetzt die adaptive exakte
            Move-Bewertung als verbesserte Standardstrategie. Die alte Baseline bleibt
            weiterhin über TabuSearchSettings.CreateDefault verfügbar.
            */
        }
 

        public TabuSearchSolver(
            TabuSearchSettings settings,
            INeighborhoodDefinition neighborhoodDefinition)
        {
            this.settings = settings;
            neighborhood = neighborhoodDefinition;

            randomMoveSelector =
                new Random(
                    settings.RandomMoveSelectionSeed);
        }

        public int Run(
            Instance instance)
        {
            /*
            Kompatible Methode für bestehenden Code.

            Bisher hat Run nur den besten Cmax zurückgegeben. Diese Schnittstelle
            bleibt erhalten, damit andere Klassen wie SchedulingApplication oder
            BenchmarkEvaluationRunner weiter funktionieren.
            */
            TabuSearchResult result =
                RunDetailed(
                    instance);

            return result.BestCmax;
        }

        public TabuSearchResult RunDetailed(
            Instance instance)
        {
            /*
            Diese Methode liefert zusätzlich zum besten Cmax auch Statistiken,
            Restart-Informationen und den Abbruchgrund. Für spätere Experimente
            ist diese Methode aussagekräftiger als Run().
            */
            Stopwatch stopwatch =
                Stopwatch.StartNew();

            TabuSearchStatistics statistics =
                new TabuSearchStatistics();

            RestartStatistics restartStatistics =
                new RestartStatistics();

            bool useTimeLimit =
                settings.TimeLimitSeconds > 0;

            int operationCount =
                instance.Jobs.Sum(job => job.Operations.Count);

            RestartManager restartManager =
                new RestartManager(
                    settings);

            int restartAfterNoImprovement =
                restartManager.GetRestartThreshold(
                    operationCount);

            int restartPerturbationMoves =
                restartManager.GetPerturbationMoveCount(
                    operationCount);

            Console.WriteLine(
                "Restart threshold: " +
                restartAfterNoImprovement);

            Console.WriteLine(
                "Restart perturbation moves: " +
                restartPerturbationMoves);

            Dictionary<int, List<Operation>> currentOrders =
                ScheduleOrderHelper.BuildMachineOrders(
                    instance);

            bool initialFeasible =
                ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                    instance,
                    currentOrders,
                    out int currentCmax);

            if (!initialFeasible)
            {
                statistics.StopReason =
                    StopReason.InitialSolutionInfeasible;

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
                    settings.MaxIterations,
                    settings);

            int iteration =
                0;

            int iterationsSinceImprovement =
                0;

            if (settings.VerboseOutput)
            {
                PrintSearchHeader(
                    currentCmax);
            }

            while (ShouldContinueSearch(
                useTimeLimit,
                stopwatch,
                iteration,
                statistics))
            {
                iteration++;
                statistics.Iterations = iteration;

                tabuList.UpdateTenureIfNeeded(
                    iteration);

                /*
                Kritische Operationen bestimmen.

                Diese Analyse liefert r_i, q_i und die Menge kritischer Operationen.
                Aus diesen Informationen werden anschließend kritische Blöcke auf den
                Maschinenreihenfolgen gebildet.
                */
                CriticalOperationAnalysisResult analysisResult =
                    CriticalOperationAnalyzer.Analyze(
                        instance,
                        currentOrders);

                if (CheckTimeLimitAndSetStopReason(
                    useTimeLimit,
                    stopwatch,
                    statistics))
                {
                    break;
                }

                List<CriticalBlock> criticalBlocks =
                    CriticalBlockBuilder.BuildCriticalBlocks(
                        currentOrders,
                        analysisResult.CriticalOperations);

                if (CheckTimeLimitAndSetStopReason(
                    useTimeLimit,
                    stopwatch,
                    statistics))
                {
                    break;
                }

                if (settings.VerboseOutput &&
                    iteration <= 10)
                {
                    Console.WriteLine(
                        "Critical operations: " +
                        analysisResult.CriticalOperations.Count);
                }

                /*
                Die Nachbarschaft erzeugt mögliche Moves.
                Welche Moves entstehen, hängt von der gewählten Nachbarschaft ab,
                z.B. N1, N2, N3 oder Insert-Moves.
                */
                List<Move> moves =
                    neighborhood.GenerateMoves(
                        instance,
                        currentOrders,
                        criticalBlocks);

                statistics.GeneratedMoves +=
                    moves.Count;

                if (CheckTimeLimitAndSetStopReason(
                    useTimeLimit,
                    stopwatch,
                    statistics))
                {
                    break;
                }

                if (settings.VerboseOutput &&
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
                    statistics.StopReason =
                        StopReason.NoMovesGenerated;

                    if (settings.VerboseOutput)
                    {
                        Console.WriteLine("No neighborhood moves found.");
                    }

                    break;
                }

                /*
                Auswahl der Moves für die exakte Bewertung.

                Im Baseline-Modus werden Moves zuerst schnell abgeschätzt.
                Für die neue Evaluation kann diese Abschätzung vollständig
                deaktiviert werden.
                */
                List<Move> promisingMoves =
                    SelectMovesForExactEvaluation(
                        instance,
                        currentOrders,
                        analysisResult,
                        moves,
                        tabuList,
                        iterationsSinceImprovement,
                        useTimeLimit,
                        stopwatch,
                        statistics);

                if (CheckTimeLimitAndSetStopReason(
                    useTimeLimit,
                    stopwatch,
                    statistics))
                {
                    break;
                }

                if (promisingMoves.Count == 0)
                {
                    statistics.StopReason =
                        StopReason.NoEstimatedMoves;

                    break;
                }

                if (settings.VerboseOutput &&
                    ShouldPrintIterationDetails(iteration))
                {
                    Console.WriteLine(
                        "Exactly evaluated moves: " +
                        promisingMoves.Count);
                }

                /*
                Die ausgewählten Moves werden exakt bewertet.
                Dabei wird der Move auf eine Kopie der Maschinenreihenfolge angewendet
                und der Schedule vollständig neu berechnet.
                */
                MoveSelectionResult selectionResult =
                    SelectBestMove(
                        instance,
                        currentOrders,
                        promisingMoves,
                        tabuList,
                        iteration,
                        iterationsSinceImprovement,
                        bestCmax,
                        currentCmax,
                        useTimeLimit,
                        stopwatch);

                statistics.ExactEvaluations +=
                    selectionResult.ExactEvaluations;

                statistics.FeasibleCandidates +=
                    selectionResult.FeasibleCandidates;

                statistics.InfeasibleCandidates +=
                    selectionResult.InfeasibleCandidates;

                statistics.TabuRejectedMoves +=
                    selectionResult.TabuRejectedMoves;

                if (CheckTimeLimitAndSetStopReason(
                    useTimeLimit,
                    stopwatch,
                    statistics))
                {
                    break;
                }

                if (settings.VerboseOutput &&
                    ShouldPrintIterationDetails(iteration))
                {
                    Console.WriteLine(
                        "Best candidate Cmax: " +
                        selectionResult.BestCandidateCmax);
                }

                /*
                Wie im ursprünglichen Solver wird bei fehlendem zulässigen Move nicht
                sofort abgebrochen. Die Suche zählt dies als Stagnation und läuft
                weiter, bis ein anderes Abbruchkriterium greift.
                */
                if (!selectionResult.HasAdmissibleMove)
                {
                    if (settings.VerboseOutput)
                    {
                        Console.WriteLine("No admissible move found.");
                    }

                    iterationsSinceImprovement++;
                    continue;
                }

                currentOrders =
                    selectionResult.BestCandidateOrders!;

                currentCmax =
                    selectionResult.BestCandidateCmax;

                tabuList.RegisterMove(
                    selectionResult.BestMove!,
                    iteration);

                statistics.AppliedMoves++;

                if (currentCmax < bestCmax)
                {
                    bestCmax =
                        currentCmax;

                    bestOrders =
                        ScheduleOrderHelper.CopyMachineOrders(
                            currentOrders);

                    iterationsSinceImprovement =
                        0;

                    statistics.Improvements++;

                    restartStatistics.MarkLatestRestartAsSuccessful(
                        iteration,
                        bestCmax);
                }
                else
                {
                    iterationsSinceImprovement++;
                }

                /*
                Restart wird nach längerer Stagnation ausgeführt.
                Die Restart-Logik liegt im RestartManager, damit sie separat
                analysiert und später parametrisiert werden kann.
                */
                if (restartManager.ShouldRestart(
                    iterationsSinceImprovement,
                    operationCount))
                {
                    int cmaxBeforeRestart =
                        currentCmax;

                    RestartResult restartResult =
                        restartManager.RestartFromBestSolution(
                            instance,
                            bestOrders,
                            restartPerturbationMoves);

                    currentOrders =
                        restartResult.MachineOrders;

                    currentCmax =
                        restartResult.Cmax;

                    tabuList.ClearShortTermMemory();

                    iterationsSinceImprovement =
                        0;

                    statistics.Restarts++;

                    restartStatistics.AddRestart(
                        iteration,
                        cmaxBeforeRestart,
                        currentCmax,
                        bestCmax,
                        restartPerturbationMoves,
                        restartResult.UsedFallbackSolution);

                    if (settings.VerboseOutput)
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
                            statistics.Restarts);
                        Console.WriteLine();
                    }
                }

                if (settings.VerboseOutput &&
                    ShouldPrintIterationDetails(iteration))
                {
                    Console.WriteLine(
                        iteration.ToString().PadRight(8) + " | " +
                        currentCmax.ToString().PadRight(10) + " | " +
                        bestCmax.ToString().PadRight(10) + " | " +
                        tabuList.CurrentTenure.ToString().PadRight(8) + " | " +
                        selectionResult.BestMove);
                }
            }

            stopwatch.Stop();

            statistics.Runtime =
                stopwatch.Elapsed;

            /*
            Am Ende wird die gespeicherte beste Maschinenreihenfolge noch einmal
            neu berechnet. Dadurch wird geprüft, ob der gespeicherte bestCmax
            wirklich zur gespeicherten Lösung passt.
            */
            bool finalFeasible =
                ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                    instance,
                    bestOrders,
                    out int finalBestCmax);

            if (!finalFeasible)
            {
                throw new InvalidOperationException(
                    "Stored best solution became infeasible.");
            }

            if (finalBestCmax != bestCmax)
            {
                Console.WriteLine(
                    "Warning: Stored best Cmax changed after final recalculation. " +
                    "Stored bestCmax: " +
                    bestCmax +
                    " | Recalculated bestOrders: " +
                    finalBestCmax);

                bestCmax =
                    finalBestCmax;
            }

            TabuSearchResult result =
                new TabuSearchResult
                {
                    InitialCmax = initialCmax,
                    BestCmax = bestCmax,
                    Runtime = stopwatch.Elapsed,
                    BestMachineOrders = bestOrders,
                    Statistics = statistics,
                    RestartStatistics = restartStatistics
                };

            PrintCompactSummary(
                useTimeLimit,
                result);

            if (settings.VerboseOutput)
            {
                PrintMachineOrder(
                    bestOrders);
            }

            return result;
        }

        private List<MoveCandidate> EstimateMoves(
            Instance instance,
            Dictionary<int, List<Operation>> currentOrders,
            CriticalOperationAnalysisResult analysisResult,
            List<Move> moves,
            MoveTabuList tabuList,
            int iterationsSinceImprovement,
            bool useTimeLimit,
            Stopwatch stopwatch,
            TabuSearchStatistics statistics)
        {
            List<MoveCandidate> estimatedMoves =
                new List<MoveCandidate>();

            foreach (Move move in moves)
            {
                if (IsTimeLimitReached(
                    useTimeLimit,
                    stopwatch))
                {
                    break;
                }

                int estimatedCmax =
                    MoveFastEvaluator.EstimateSwapCmax(
                        instance,
                        currentOrders,
                        analysisResult,
                        move);

                int moveFrequency =
                    tabuList.GetFrequencyPenalty(
                        move);

                double estimatedValue =
                    FrequencyPenaltyCalculator.CalculateEvaluationValue(
                        estimatedCmax,
                        moveFrequency,
                        iterationsSinceImprovement,
                        settings);

                MoveCandidate candidate =
                    new MoveCandidate(
                        move)
                    {
                        EstimatedEvaluationValue = estimatedValue
                    };

                estimatedMoves.Add(
                    candidate);

                statistics.EstimatedMoves++;
            }

            return estimatedMoves;
        }

        private List<Move> SelectMovesForExactEvaluation(
        Instance instance,
        Dictionary<int, List<Operation>> currentOrders,
        CriticalOperationAnalysisResult analysisResult,
        List<Move> moves,
        MoveTabuList tabuList,
        int iterationsSinceImprovement,
        bool useTimeLimit,
        Stopwatch stopwatch,
        TabuSearchStatistics statistics)
            {
            /*
            Diese Methode entscheidet, welche Moves exakt bewertet werden.

            Im Standardmodus wird zuerst die schnelle Move-Abschätzung verwendet.
            Für die neue adaptive Variante bleibt diese Abschätzung aktiv, aber die Anzahl
            exakt bewerteter Kandidaten wird nicht mehr fest gewählt. Stattdessen hängt sie
            von der Anzahl der generierten Moves und von der Stagnation der Suche ab.
            */
            if (settings.MoveSelectionMode == MoveSelectionMode.EstimatedTopCandidates)
            {
                List<MoveCandidate> estimatedMoves =
                    EstimateMoves(
                        instance,
                        currentOrders,
                        analysisResult,
                        moves,
                        tabuList,
                        iterationsSinceImprovement,
                        useTimeLimit,
                        stopwatch,
                        statistics);

                int exactEvaluationLimit =
                    CalculateExactEvaluationLimit(
                        estimatedMoves.Count,
                        iterationsSinceImprovement);

                return estimatedMoves
                    .OrderBy(candidate => candidate.EstimatedEvaluationValue)
                    .Take(exactEvaluationLimit)
                    .Select(candidate => candidate.Move)
                    .ToList();
            }

            if (settings.MoveSelectionMode == MoveSelectionMode.NoEstimationAllExact)
            {
                /*
                In dieser Variante wird keine schnelle Abschätzung verwendet.

                Alle generierten Moves werden an die exakte Bewertung weitergegeben.
                Die Laufzeit kann dadurch deutlich steigen. Das Zeitlimit wird jedoch
                weiterhin in SelectBestMove geprüft.
                */
                return moves.ToList();
            }

            if (settings.MoveSelectionMode == MoveSelectionMode.NoEstimationRandomCandidates)
            {
                /*
                In dieser Variante wird ebenfalls keine schnelle Abschätzung verwendet.

                Stattdessen wird eine zufällige Teilmenge der Moves exakt bewertet.
                Damit kann geprüft werden, ob die schnelle Abschätzung besser ist als
                eine einfache zufällige Kandidatenauswahl.
                */
                int selectionCount =
                    Math.Min(
                        settings.MaxExactEvaluationsPerIteration,
                        moves.Count);

                if (selectionCount <= 0)
                {
                    return new List<Move>();
                }

                List<Move> shuffledMoves =
                    moves.ToList();

                for (int i = 0; i < selectionCount; i++)
                {
                    int randomIndex =
                        randomMoveSelector.Next(
                            i,
                            shuffledMoves.Count);

                    Move temp =
                        shuffledMoves[i];

                    shuffledMoves[i] =
                        shuffledMoves[randomIndex];

                    shuffledMoves[randomIndex] =
                        temp;
                }

                return shuffledMoves
                    .Take(selectionCount)
                    .ToList();
            }

            throw new InvalidOperationException(
                "Unknown move selection mode: " +
                settings.MoveSelectionMode);
        }
        private int CalculateExactEvaluationLimit(
    int generatedMoveCount,
    int iterationsSinceImprovement)
        {
            /*
            Berechnet, wie viele Moves in dieser Iteration exakt bewertet werden.

            Ohne adaptive Bewertung bleibt das alte Verhalten erhalten:
            Es werden maximal MaxExactEvaluationsPerIteration Kandidaten exakt geprüft.

            Mit adaptiver Bewertung wird die Anzahl abhängig von der Nachbarschaftsgröße
            und von der aktuellen Stagnation erhöht. Dadurch bleibt die schnelle
            Abschätzung als Vorfilter erhalten, aber bei größeren Nachbarschaften oder
            längerer Stagnation werden mehr Kandidaten exakt nachbewertet.
            */
            if (generatedMoveCount <= 0)
            {
                return 0;
            }

            if (!settings.UseAdaptiveExactEvaluationLimit)
            {
                return Math.Min(
                    settings.MaxExactEvaluationsPerIteration,
                    generatedMoveCount);
            }

            if (generatedMoveCount <= settings.AdaptiveExactEvaluateAllMoveThreshold)
            {
                return generatedMoveCount;
            }

            int fractionBasedLimit =
                (int)Math.Ceiling(
                    generatedMoveCount *
                    settings.AdaptiveExactMoveFraction);

            int exactEvaluationLimit =
                Math.Max(
                    settings.AdaptiveExactMinEvaluations,
                    fractionBasedLimit);

            if (iterationsSinceImprovement >= settings.HighStagnationThreshold)
            {
                exactEvaluationLimit =
                    Math.Max(
                        exactEvaluationLimit,
                        settings.AdaptiveExactHighStagnationMinEvaluations);
            }
            else if (iterationsSinceImprovement >= settings.MediumStagnationThreshold)
            {
                exactEvaluationLimit =
                    Math.Max(
                        exactEvaluationLimit,
                        settings.AdaptiveExactMediumStagnationMinEvaluations);
            }

            exactEvaluationLimit =
                Math.Min(
                    exactEvaluationLimit,
                    settings.AdaptiveExactMaxEvaluations);

            exactEvaluationLimit =
                Math.Min(
                    exactEvaluationLimit,
                    generatedMoveCount);

            return Math.Max(
                1,
                exactEvaluationLimit);
        }

        private MoveSelectionResult SelectBestMove(
            Instance instance,
            Dictionary<int, List<Operation>> currentOrders,
            List<Move> promisingMoves,
            MoveTabuList tabuList,
            int iteration,
            int iterationsSinceImprovement,
            int bestCmax,
            int currentCmax,
            bool useTimeLimit,
            Stopwatch stopwatch)
        {
            MoveSelectionResult result =
                new MoveSelectionResult();

            foreach (Move move in promisingMoves)
            {
                if (IsTimeLimitReached(
                    useTimeLimit,
                    stopwatch))
                {
                    break;
                }

                MoveExactEvaluationResult exactResult =
                    MoveExactEvaluator.Evaluate(
                        instance,
                        currentOrders,
                        move);

                result.ExactEvaluations++;

                if (!exactResult.IsFeasible)
                {
                    result.InfeasibleCandidates++;
                    continue;
                }

                result.FeasibleCandidates++;

                bool isTabu =
                    tabuList.IsTabu(
                        move,
                        iteration,
                        exactResult.Cmax,
                        bestCmax,
                        currentCmax);

                if (isTabu)
                {
                    result.TabuRejectedMoves++;
                    continue;
                }

                int moveFrequency =
                    tabuList.GetFrequencyPenalty(
                        move);

                double candidateEvaluationValue =
                    FrequencyPenaltyCalculator.CalculateEvaluationValue(
                        exactResult.Cmax,
                        moveFrequency,
                        iterationsSinceImprovement,
                        settings);

                if (candidateEvaluationValue < result.BestCandidateEvaluationValue)
                {
                    result.BestCandidateEvaluationValue =
                        candidateEvaluationValue;

                    result.BestCandidateCmax =
                        exactResult.Cmax;

                    result.BestMove =
                        move;

                    result.BestCandidateOrders =
                        exactResult.MachineOrders;
                }
            }

            return result;
        }

        private bool ShouldContinueSearch(
            bool useTimeLimit,
            Stopwatch stopwatch,
            int iteration,
            TabuSearchStatistics statistics)
        {
            /*
            Die maximale Iterationsanzahl gilt in beiden Modi.

            Im 90-Sekunden-Modus ist sie ein zusätzliches Abbruchkriterium.
            Im Extended-Modus ist sie das Hauptkriterium, ergänzt durch ein
            technisches Sicherheitslimit.
            */
            if (iteration >= settings.MaxIterations)
            {
                statistics.StopReason =
                    StopReason.MaxIterationsReached;

                return false;
            }

            if (IsTimeLimitReached(
                useTimeLimit,
                stopwatch))
            {
                statistics.StopReason =
                    useTimeLimit
                        ? StopReason.TimeLimitReached
                        : StopReason.ExtendedSafetyLimitReached;

                return false;
            }

            return true;
        }

        private bool CheckTimeLimitAndSetStopReason(
            bool useTimeLimit,
            Stopwatch stopwatch,
            TabuSearchStatistics statistics)
        {
            if (!IsTimeLimitReached(
                useTimeLimit,
                stopwatch))
            {
                return false;
            }

            statistics.StopReason =
                useTimeLimit
                    ? StopReason.TimeLimitReached
                    : StopReason.ExtendedSafetyLimitReached;

            return true;
        }

        private bool IsTimeLimitReached(
            bool useTimeLimit,
            Stopwatch stopwatch)
        {
            if (useTimeLimit)
            {
                return stopwatch.Elapsed.TotalSeconds >= settings.TimeLimitSeconds;
            }

            return stopwatch.Elapsed.TotalSeconds >=
                   settings.ExtendedModeSafetyTimeLimitSeconds;
        }

        private bool ShouldPrintIterationDetails(
            int iteration)
        {
            return iteration <= 10 ||
                   iteration % settings.VerbosePrintInterval == 0;
        }

        private void PrintSearchHeader(
            int currentCmax)
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

        private static void PrintCompactSummary(
            bool useTimeLimit,
            TabuSearchResult result)
        {
            Console.WriteLine();
            Console.WriteLine("Tabu Search finished.");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine(
                "Mode: " +
                (useTimeLimit
                    ? "90s time limit"
                    : "Extended without fixed time limit"));
            Console.WriteLine("Initial Cmax: " + result.InitialCmax);
            Console.WriteLine("Best Cmax: " + result.BestCmax);
            Console.WriteLine("Improvement: " + result.ImprovementPercent.ToString("F2") + "%");
            Console.WriteLine("Iterations: " + result.Statistics.Iterations);
            Console.WriteLine("Restarts: " + result.Statistics.Restarts);
            Console.WriteLine("Stop reason: " + result.Statistics.StopReason);
            Console.WriteLine("Runtime: " + result.Runtime.TotalSeconds.ToString("F2") + " s");
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