using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods;
using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Evaluation
{
    public static class TabuEvaluationRunner
    {
        /*
        Dieser Runner führt die experimentelle Evaluation der Tabu Search aus.

        Wichtig:
        Diese Klasse ersetzt nicht den ursprünglichen Full Benchmark Runner.
        Der ursprüngliche Runner bleibt für die Abgabe-Baseline erhalten.

        Diese Klasse ist nur für die wissenschaftliche Analyse gedacht:
        - Restart-Varianten
        - Frequency-Penalty-Varianten
        - Anzahl exakt bewerteter Moves
        - Tabu-Dauer
        - Nachbarschaftsvergleich
        - Einfluss der schnellen Move-Abschätzung

        Für den ersten Schritt wird bewusst eine Screening-Evaluation verwendet.
        Das bedeutet: viele Varianten, aber nur eine repräsentative Auswahl von
        Instanzen. Dadurch kann man schnell erkennen, welche Varianten überhaupt
        vielversprechend sind, ohne sofort sehr lange Full Runs auszuführen.
        */

        private const int TimeLimit90Seconds =
            90;

        private const string ScreeningOutputFileName =
            "Tabu_Evaluation_Screening.csv";

        private const string MoveSelectionOutputFileName =
            "Tabu_Evaluation_MoveSelection.csv";
        
        private const string AdaptiveExactOutputFileName =
        "Tabu_Evaluation_AdaptiveExact.csv";

        public static void RunScreeningEvaluation()
        {
            /*
            Diese Methode ist der erste zentrale Einstiegspunkt für die Evaluation.

            Sie führt alle Screening-Varianten im 90s-Modus aus.
            Der Extended-Modus wird hier bewusst nicht verwendet, weil das Screening
            sonst sehr lange dauern würde und die erste Analyse vor allem zeigen soll,
            welche Parameter grundsätzlich interessant sind.
            */

            Console.WriteLine();
            Console.WriteLine("=======================================");
            Console.WriteLine(" TABU SEARCH EXPERIMENTAL SCREENING");
            Console.WriteLine("=======================================");
            Console.WriteLine();

            string benchmarkFolder =
                Path.Combine(
                    GetProjectRootFolder(),
                    "Instances",
                    "Benchmark");

            if (!Directory.Exists(benchmarkFolder))
            {
                Console.WriteLine("Benchmark folder not found:");
                Console.WriteLine(benchmarkFolder);
                return;
            }

            List<string> selectedInstanceFiles =
                GetScreeningInstanceFiles(
                    benchmarkFolder);

            List<TabuEvaluationVariant> variants =
                TabuEvaluationVariantFactory.CreateScreeningVariants();

            RunEvaluation(
                selectedInstanceFiles,
                variants,
                ScreeningOutputFileName,
                "SCREENING EVALUATION");
        }

        public static void RunMoveSelectionEvaluation()
        {
            /*
            Diese Evaluation untersucht gezielt, ob die schnelle Move-Abschätzung
            hilfreich ist.

            Verglichen werden drei Varianten:
            - Estimate_Top20:
              Baseline-Verhalten. Alle Moves werden schnell abgeschätzt und danach
              werden die besten 20 Moves exakt bewertet.

            - NoEstimate_AllExact:
              Die schnelle Abschätzung wird vollständig deaktiviert. Alle erzeugten
              Moves werden exakt bewertet, soweit das Zeitlimit dies erlaubt.

            - NoEstimate_Random20:
              Die schnelle Abschätzung wird ebenfalls deaktiviert. Stattdessen werden
              zufällig 20 Moves pro Iteration exakt bewertet.

            Dadurch kann geprüft werden, ob die Abschätzung gute Moves besser
            auswählt als eine zufällige Auswahl und ob eine vollständige exakte
            Bewertung trotz höherer Kosten bessere Ergebnisse liefert.
            */

            Console.WriteLine();
            Console.WriteLine("=======================================");
            Console.WriteLine(" TABU SEARCH MOVE SELECTION EVALUATION");
            Console.WriteLine("=======================================");
            Console.WriteLine();

            string benchmarkFolder =
                Path.Combine(
                    GetProjectRootFolder(),
                    "Instances",
                    "Benchmark");

            if (!Directory.Exists(benchmarkFolder))
            {
                Console.WriteLine("Benchmark folder not found:");
                Console.WriteLine(benchmarkFolder);
                return;
            }

            List<string> selectedInstanceFiles =
                GetMoveSelectionInstanceFiles(
                    benchmarkFolder);

            List<TabuEvaluationVariant> variants =
                TabuEvaluationVariantFactory.CreateMoveSelectionComparisonVariants();

            RunEvaluation(
                selectedInstanceFiles,
                variants,
                MoveSelectionOutputFileName,
                "MOVE SELECTION EVALUATION");
        }

        public static void RunAdaptiveExactEvaluation()
        {
            /*
            Diese Evaluation vergleicht feste Grenzen für die exakte Move-Bewertung
            mit der neuen adaptiven Strategie.

            Die adaptive Strategie basiert auf der vorherigen Evaluation:
            Die Move-Abschätzung bleibt aktiv, aber die Anzahl exakt bewerteter Kandidaten
            wird abhängig von der Anzahl generierter Moves und von der Stagnation gewählt.

            Verglichen werden:
            - Baseline mit 20 exakt bewerteten Kandidaten
            - feste Grenzen mit 50, 100 und 200 Kandidaten
            - adaptive Grenze zwischen 50 und 200 Kandidaten
            */

            Console.WriteLine();
            Console.WriteLine("=======================================");
            Console.WriteLine(" TABU SEARCH ADAPTIVE EXACT EVALUATION");
            Console.WriteLine("=======================================");
            Console.WriteLine();

            string benchmarkFolder =
                Path.Combine(
                    GetProjectRootFolder(),
                    "Instances",
                    "Benchmark");

            if (!Directory.Exists(benchmarkFolder))
            {
                Console.WriteLine("Benchmark folder not found:");
                Console.WriteLine(benchmarkFolder);
                return;
            }

            /*
            Es werden bewusst dieselben Instanzen wie in der Move-Selection-Evaluation
            verwendet. Dadurch können die Ergebnisse direkt mit der vorherigen Analyse
            verglichen werden.
            */
            List<string> selectedInstanceFiles =
                GetMoveSelectionInstanceFiles(
                    benchmarkFolder);

            List<TabuEvaluationVariant> variants =
                TabuEvaluationVariantFactory.CreateAdaptiveExactEvaluationComparisonVariants();

            RunEvaluation(
                selectedInstanceFiles,
                variants,
                AdaptiveExactOutputFileName,
                "ADAPTIVE EXACT EVALUATION");
        }

        private static void RunEvaluation(
            List<string> selectedInstanceFiles,
            List<TabuEvaluationVariant> variants,
            string outputFileName,
            string evaluationTitle)
        {
            /*
            Gemeinsame Ausführungslogik für verschiedene Evaluationsarten.

            Dadurch verwenden Screening-Evaluation und Move-Selection-Evaluation
            denselben Ablauf:
            - Initialheuristik einmal pro Instanz ausführen,
            - alle Varianten auf derselben Startlösung testen,
            - Ergebnisse direkt nach jedem Lauf in eine CSV-Datei schreiben.
            */

            Console.WriteLine("Selected instances: " + selectedInstanceFiles.Count);
            Console.WriteLine("Evaluation variants: " + variants.Count);
            Console.WriteLine("Run mode: 90s");
            Console.WriteLine();

            Console.WriteLine("Output CSV:");
            Console.WriteLine(
                Path.Combine(
                    GetProjectRootFolder(),
                    "Results",
                    "Csv",
                    outputFileName));
            Console.WriteLine();

            bool firstRow =
                true;

            int totalRuns =
                selectedInstanceFiles.Count * variants.Count;

            int completedRuns =
                0;

            foreach (string instanceFile in selectedInstanceFiles)
            {
                string instanceName =
                    Path.GetFileNameWithoutExtension(
                        instanceFile);

                Console.WriteLine();
                Console.WriteLine("=======================================");
                Console.WriteLine("Instance: " + instanceName);
                Console.WriteLine("=======================================");
                Console.WriteLine();

                /*
                Die Initialheuristik wird pro Instanz einmal ausgeführt.
                Alle Varianten starten anschließend von derselben besten
                Initiallösung.

                Dadurch ist der Vergleich fair:
                Unterschiede zwischen Varianten entstehen durch die Tabu-Search-
                Parameter oder die Nachbarschaft, nicht durch andere Startlösungen.
                */

                InitialHeuristicResult initialResult =
                    HeuristicExperiment.Run(
                        instanceFile);

                int maxIterations =
                    CalculateInstanceDependentMaxIterations(
                        initialResult.bestInstance);

                foreach (TabuEvaluationVariant variant in variants)
                {
                    completedRuns++;

                    Console.WriteLine();
                    Console.WriteLine("---------------------------------------");
                    Console.WriteLine(
                        "Run " +
                        completedRuns +
                        " / " +
                        totalRuns);
                    Console.WriteLine("Variant: " + variant.VariantName);
                    Console.WriteLine("Category: " + variant.Category);
                    Console.WriteLine("Instance: " + instanceName);
                    Console.WriteLine("---------------------------------------");

                    try
                    {
                        TabuEvaluationResultRow row =
                            RunSingleVariant(
                                instanceFile,
                                initialResult,
                                variant,
                                maxIterations,
                                TimeLimit90Seconds);

                        /*
                        Die Ergebniszeile wird sofort gespeichert.
                        Dadurch gehen Ergebnisse nicht verloren, falls ein späterer
                        Lauf sehr lange dauert oder die Evaluation abgebrochen wird.
                        */

                        TabuEvaluationCsvWriter.WriteResultsToFile(
                            new List<TabuEvaluationResultRow> { row },
                            outputFileName,
                            append: !firstRow);

                        firstRow =
                            false;

                        Console.WriteLine(
                            "Result: Cmax = " +
                            row.BestCmax +
                            ", Runtime = " +
                            row.RuntimeSeconds.ToString("F2") +
                            " s, StopReason = " +
                            row.StopReason);
                    }
                    catch (Exception exception)
                    {
                        /*
                        Ein einzelner fehlerhafter Lauf soll nicht die gesamte
                        Evaluation abbrechen. Der Fehler wird ausgegeben und die
                        nächste Variante wird fortgesetzt.

                        Für die finale Auswertung sollte man solche Fälle später
                        separat prüfen.
                        */

                        Console.WriteLine();
                        Console.WriteLine("ERROR during variant run:");
                        Console.WriteLine("Variant: " + variant.VariantName);
                        Console.WriteLine("Instance: " + instanceName);
                        Console.WriteLine(exception.Message);
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("=======================================");
            Console.WriteLine(" " + evaluationTitle + " FINISHED");
            Console.WriteLine("=======================================");
            Console.WriteLine("Completed runs: " + completedRuns + " / " + totalRuns);
            Console.WriteLine("CSV file:");
            Console.WriteLine(
                Path.Combine(
                    GetProjectRootFolder(),
                    "Results",
                    "Csv",
                    outputFileName));
        }

        private static TabuEvaluationResultRow RunSingleVariant(
            string instanceFile,
            InitialHeuristicResult initialResult,
            TabuEvaluationVariant variant,
            int maxIterations,
            int timeLimitSeconds)
        {
            /*
            Führt genau eine Variante auf genau einer Instanz aus.

            Alle relevanten Kennzahlen werden anschließend in eine
            TabuEvaluationResultRow übertragen. Diese Zeile ist die Grundlage
            für spätere Tabellen und Diagramme in der Hausarbeit.
            */

            Instance instanceCopy =
                CloneInstance(
                    initialResult.bestInstance);

            TabuSearchSettings settings =
                variant.CreateSettings(
                    maxIterations,
                    timeLimitSeconds);

            INeighborhoodDefinition neighborhood =
                variant.CreateNeighborhood();

            TabuSearchSolver solver =
                new TabuSearchSolver(
                    settings,
                    neighborhood);

            Stopwatch stopwatch =
                Stopwatch.StartNew();

            TabuSearchResult result =
                solver.RunDetailed(
                    instanceCopy);

            stopwatch.Stop();

            /*
            Die Runtime wird bevorzugt aus dem TabuSearchResult gelesen.
            Falls der Solver dort aus irgendeinem Grund keine Laufzeit gesetzt hat,
            wird die lokal gemessene Stopwatch-Laufzeit verwendet.
            */

            double runtimeSeconds =
                result.Runtime.TotalSeconds > 0.0
                    ? result.Runtime.TotalSeconds
                    : stopwatch.Elapsed.TotalSeconds;

            int operationCount =
                CountOperations(
                    initialResult.bestInstance);

            bool restartEnabled =
                settings.UseRestart;

            int restartThreshold =
                restartEnabled
                    ? operationCount * settings.RestartAfterNoImprovementFactor
                    : 0;

            int restartPerturbationMoves =
                restartEnabled
                    ? Math.Max(
                        settings.RestartMinPerturbationMoves,
                        operationCount / settings.RestartOperationDivisor)
                    : 0;

            int fallbackRestarts =
                result.RestartStatistics.Events
                    .Count(restartEvent => restartEvent.UsedFallbackSolution);

            return new TabuEvaluationResultRow
            {
                InstanceName =
                    Path.GetFileNameWithoutExtension(
                        instanceFile),

                VariantName =
                    variant.VariantName,

                Category =
                    variant.Category,

                RunMode =
                    timeLimitSeconds > 0
                        ? "90s"
                        : "Extended",

                NeighborhoodName =
                    neighborhood.GetType().Name,

                IsBaseline =
                    variant.IsBaseline,

                MultiStartCount =
                    variant.MultiStartCount,

                MaxIterations =
                    maxIterations,

                TimeLimitSeconds =
                    timeLimitSeconds,

                InitialRule =
                    initialResult.bestRule.ToString(),

                InitialCmax =
                    initialResult.bestCmax,

                BestCmax =
                    result.BestCmax,

                ImprovementPercent =
                    result.ImprovementPercent,

                RuntimeSeconds =
                    runtimeSeconds,

                StopReason =
                    result.Statistics.StopReason.ToString(),

                Iterations =
                    result.Statistics.Iterations,

                GeneratedMoves =
                    result.Statistics.GeneratedMoves,

                EstimatedMoves =
                    result.Statistics.EstimatedMoves,

                ExactEvaluations =
                    result.Statistics.ExactEvaluations,

                FeasibleCandidates =
                    result.Statistics.FeasibleCandidates,

                InfeasibleCandidates =
                    result.Statistics.InfeasibleCandidates,

                TabuRejectedMoves =
                    result.Statistics.TabuRejectedMoves,

                AppliedMoves =
                    result.Statistics.AppliedMoves,

                Improvements =
                    result.Statistics.Improvements,

                RestartEnabled =
                    restartEnabled,

                RestartThreshold =
                    restartThreshold,

                RestartPerturbationMoves =
                    restartPerturbationMoves,

                Restarts =
                    result.RestartStatistics.TotalRestarts,

                SuccessfulRestarts =
                    result.RestartStatistics.SuccessfulRestarts,

                FallbackRestarts =
                    fallbackRestarts,

                RestartSuccessRate =
                    result.RestartStatistics.SuccessRate,

                FrequencyPenaltyEnabled =
                    settings.UseFrequencyPenalty,

                LowPenaltyRate =
                    settings.LowStagnationPenaltyRate,

                MediumPenaltyRate =
                    settings.MediumStagnationPenaltyRate,

                HighPenaltyRate =
                    settings.HighStagnationPenaltyRate,

                MaxExactEvaluationsPerIteration =
                    settings.MaxExactEvaluationsPerIteration,

                TabuMinTenureFactor =
                    settings.MinTenureFactor,

                TabuMaxTenureFactor =
                    settings.MaxTenureFactor
            };
        }

        private static List<string> GetScreeningInstanceFiles(
            string benchmarkFolder)
        {
            /*
            Die Screening-Auswahl enthält bewusst 12 repräsentative Instanzen.

            Die Auswahl deckt unterschiedliche Problemgrößen und Suchverhalten ab:
            - kleine Instanzen mit sehr vielen Restarts
            - kleine Instanzen mit und ohne Verbesserung
            - mittlere Instanzen
            - große und sehr große Instanzen
            - Instanzen ohne Restart-Aktivität
            - Instanzen, bei denen Extended besser als 90s war
            - einen auffälligen Sonderfall mit sehr frühem Abbruch

            Dadurch ist das Screening methodisch belastbarer als eine zufällige
            Auswahl einzelner Instanzen.
            */

            string[] selectedInstanceNames =
            {
                /*
                Kleine Instanzen:
                Diese Fälle zeigen, wie sich Varianten verhalten, wenn sehr viele
                Iterationen in kurzer Zeit möglich sind und Restart häufig ausgelöst wird.
                */

                "ClassroomInstanceSet1_1",
                "TeamC_1",
                "TeamD_1",

                /*
                Übergang von kleinen zu mittleren Instanzen:
                Diese Instanzen sind wichtig, weil hier sowohl Iterationslimit als auch
                Zeitlimit eine Rolle spielen können.
                */

                "ClassroomInstanceSet3_1",
                "ClassroomInstanceSet6_1",

                /*
                Auffälliger Sonderfall:
                TeamB_3 endete in der Baseline extrem früh. Diese Instanz ist wichtig,
                um zu prüfen, ob bestimmte Nachbarschaften oder Parameter mehr Moves
                erzeugen und die Suche stabiler machen.
                */

                "TeamB_3",

                /*
                Eigene Team-F-Instanz:
                Diese Instanz ist wichtig, weil sie zur eigenen Instanzgruppe gehört
                und daher für die projektbezogene Bewertung besonders relevant ist.
                */

                "TeamF_Instance5",

                /*
                Größere Classroom-Instanzen:
                Diese Instanzen zeigten in der Baseline teilweise Unterschiede zwischen
                90s- und Extended-Lauf. Sie sind daher wichtig für Laufzeit- und
                Restart-Analysen.
                */

                "ClassroomInstanceSet7_1",
                "ClassroomInstanceSet10_3",

                /*
                Große und sehr große Teaminstanzen:
                Bei diesen Instanzen war die Restart-Schwelle teilweise so hoch, dass
                Restart im 90s-Modus gar nicht aktiv wurde.
                */

                "TeamA_3",
                "TeamB_1",
                "TeamB_2"
            };

            return BuildExistingInstanceFileList(
                benchmarkFolder,
                selectedInstanceNames);
        }

        private static List<string> GetMoveSelectionInstanceFiles(
        string benchmarkFolder)
        {
            /*
            Für die Move-Selection-Evaluation wird eine gezielte Instanzauswahl
            verwendet.

            Die Auswahl basiert auf der vorherigen Screening-Analyse:
            Es werden vor allem Instanzen gewählt, bei denen die Anzahl exakt bewerteter
            Moves oder die Qualität der Move-Auswahl bereits einen sichtbaren Einfluss
            auf das Ergebnis hatte.

            Kleine Instanzen ohne Variantenunterschiede werden bewusst nicht verwendet,
            weil sie für diese Fragestellung wenig zusätzliche Erkenntnis liefern.
            TeamB_3 wird ebenfalls ausgeschlossen, da diese Instanz in der Baseline sehr
            früh wegen fehlender Moves abbrach und die Move-Auswahl dort kaum bewertet
            werden kann.
            */

            string[] selectedInstanceNames =
            {
            /*
            Mittlere Kontrollinstanz:
            Viele Varianten lagen hier nah beieinander. Dadurch kann geprüft werden,
            ob die Move-Auswahl nur auf schwierigen Instanzen wirkt oder auch auf
            stabileren Fällen.
            */
            "ClassroomInstanceSet3_1",

            /*
            Mittelgroße Instanz mit sichtbaren Variantenunterschieden.
            Diese Instanz eignet sich, um zu prüfen, ob die schnelle Abschätzung
            gute Kandidaten zuverlässig auswählt.
            */
            "ClassroomInstanceSet6_1",

            /*
            Größere Classroom-Instanz mit deutlichen Verbesserungsmöglichkeiten.
            Bei größeren Nachbarschaften ist die Kandidatenauswahl besonders wichtig.
            */
            "ClassroomInstanceSet10_3",

            /*
            Eigene Team-F-Instanz.
            Diese Instanz ist für die projektbezogene Bewertung besonders relevant.
            */
            "TeamF_Instance5",

            /*
            Große Teaminstanz.
            In der vorherigen Analyse war eine höhere Anzahl exakt bewerteter Moves
            besonders relevant. Dadurch ist diese Instanz gut geeignet, um die
            Qualität der Move-Abschätzung zu prüfen.
            */
            "TeamA_3",

            /*
            Große Teaminstanz.
            Auch hier zeigte die vorherige Analyse, dass eine breitere exakte
            Bewertung bessere Ergebnisse liefern kann.
            */
            "TeamB_1"
        };

            return BuildExistingInstanceFileList(
                benchmarkFolder,
                selectedInstanceNames);
        }

        private static List<string> BuildExistingInstanceFileList(
            string benchmarkFolder,
            string[] selectedInstanceNames)
        {
            /*
            Erstellt aus den Instanznamen die Dateipfade und überspringt fehlende
            Dateien mit einer Warnung.
            */

            List<string> files =
                new List<string>();

            foreach (string instanceName in selectedInstanceNames)
            {
                string file =
                    Path.Combine(
                        benchmarkFolder,
                        instanceName + ".txt");

                if (File.Exists(file))
                {
                    files.Add(
                        file);
                }
                else
                {
                    Console.WriteLine(
                        "Warning: evaluation instance not found: " +
                        file);
                }
            }

            return files;
        }

        private static int CalculateInstanceDependentMaxIterations(
            Instance instance)
        {
            /*
            Diese Methode verwendet dieselbe Logik wie der bisherige
            BenchmarkEvaluationRunner.

            Dadurch bleibt die Screening-Evaluation mit der bisherigen Baseline
            vergleichbar.
            */

            int operationCount =
                CountOperations(
                    instance);

            if (operationCount <= 0)
            {
                throw new InvalidOperationException(
                    "Instance contains no operations.");
            }

            if (operationCount <= 60)
            {
                return 250_000;
            }

            if (operationCount <= 100)
            {
                return 200_000;
            }

            if (operationCount <= 200)
            {
                return 120_000;
            }

            if (operationCount <= 300)
            {
                return 80_000;
            }

            if (operationCount <= 450)
            {
                return 40_000;
            }

            if (operationCount <= 650)
            {
                return 20_000;
            }

            if (operationCount <= 850)
            {
                return 10_000;
            }

            return 8_000;
        }

        private static int CountOperations(
            Instance instance)
        {
            /*
            Die Anzahl der Operationen dient an mehreren Stellen als Maß
            für die Instanzgröße:
            - Iterationsbudget
            - Restart-Schwelle
            - Perturbationsstärke
            */

            return instance.Jobs
                .Sum(job => job.Operations.Count);
        }

        private static Instance CloneInstance(
            Instance original)
        {
            /*
            Jede Variante erhält eine eigene Kopie der initialen Lösung.

            Das ist wichtig, weil die Tabu Search Start- und Endzeiten sowie
            Maschinenreihenfolgen verändert. Ohne Kopie könnte ein Lauf den
            Startzustand für spätere Varianten verfälschen.
            */

            Instance clone =
                new Instance();

            clone.NumJobs =
                original.NumJobs;

            clone.NumMachines =
                original.NumMachines;

            foreach (Job originalJob in original.Jobs)
            {
                Job clonedJob =
                    new Job(
                        originalJob.JobID);

                foreach (Operation originalOperation in originalJob.Operations)
                {
                    Operation clonedOperation =
                        new Operation(
                            originalOperation.JobID,
                            originalOperation.OperationID,
                            originalOperation.Machine,
                            originalOperation.ProcessingTime);

                    clonedOperation.StartTime =
                        originalOperation.StartTime;

                    clonedOperation.EndTime =
                        originalOperation.EndTime;

                    clonedJob.Operations.Add(
                        clonedOperation);
                }

                clone.Jobs.Add(
                    clonedJob);
            }

            if (original.SetupTimes != null)
            {
                int rows =
                    original.SetupTimes.GetLength(0);

                int columns =
                    original.SetupTimes.GetLength(1);

                clone.SetupTimes =
                    new int[rows, columns];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        clone.SetupTimes[i, j] =
                            original.SetupTimes[i, j];
                    }
                }
            }

            return clone;
        }

        private static string GetProjectRootFolder()
        {
            /*
            Das Programm läuft normalerweise aus bin/Debug/netX.
            Drei Ebenen nach oben führen zurück in den Projektordner.
            */

            return Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    @"..\..\.."));
        }
    }
}