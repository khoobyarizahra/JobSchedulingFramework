using System.Collections.Generic;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods;

namespace JobShopSchedulingFramework.Evaluation
{
    public static class TabuEvaluationVariantFactory
    {
        /*
        Diese Factory sammelt alle experimentellen Varianten der Evaluation.

        Die Baseline bildet die aktuelle Abgabe-Konfiguration ab.
        Alle weiteren Varianten verändern gezielt genau eine Komponente.

        Dadurch kann später wissenschaftlich argumentiert werden:
        Wenn sich eine Variante verbessert oder verschlechtert, liegt der
        Unterschied an genau dieser veränderten Komponente.
        */

        public static TabuEvaluationVariant CreateBaselineVariant()
        {
            /*
            Die Baseline ist der wichtigste Referenzpunkt der Evaluation.

            Sie entspricht der aktuellen Standardlogik:
            - N3 als Nachbarschaft
            - Restart aktiviert
            - Frequency Penalty aktiviert
            - Move-Abschätzung aktiviert
            - 20 exakte Move-Bewertungen pro Iteration
            - dynamische Tabu-Dauer
            */

            return new TabuEvaluationVariant(
                variantName: "Baseline_N3",
                category: "Baseline",
                description: "Aktuelle Abgabe-Konfiguration mit N3, Restart, Frequency Penalty, Move-Abschätzung und 20 exakten Bewertungen pro Iteration.",
                settingsFactory: CreateDefaultSettings,
                neighborhoodFactory: () => new AllPairSwapNeighborhood(),
                isBaseline: true);
        }

        public static List<TabuEvaluationVariant> CreateScreeningVariants()
        {
            /*
            Diese Liste ist für die erste systematische Screening-Evaluation gedacht.

            Das Ziel ist nicht, sofort die beste Kombination aller Parameter zu finden.
            Stattdessen wird jeweils nur eine Komponente gegenüber der Baseline geändert.

            Dadurch entstehen interpretierbare Experimente:
            - Restart-Einfluss
            - Frequency-Penalty-Einfluss
            - Einfluss der Anzahl exakt bewerteter Moves
            - Einfluss der Tabu-Dauer
            - Einfluss der Nachbarschaft
            */

            List<TabuEvaluationVariant> variants =
                new List<TabuEvaluationVariant>();

            variants.Add(
                CreateBaselineVariant());

            AddRestartVariants(
                variants);

            AddPerturbationVariants(
                variants);

            AddFrequencyPenaltyVariants(
                variants);

            AddExactEvaluationVariants(
                variants);

            AddTabuTenureVariants(
                variants);

            AddNeighborhoodVariants(
                variants);

            return variants;
        }

        public static List<TabuEvaluationVariant> CreateMoveSelectionComparisonVariants()
        {
            /*
            Diese Variantenliste untersucht gezielt die Rolle der schnellen
            Move-Abschätzung.

            In der bisherigen Baseline werden alle erzeugten Moves zunächst schnell
            abgeschätzt. Anschließend werden nur die besten Kandidaten exakt bewertet.

            Die neuen Varianten deaktivieren diese Abschätzung vollständig:
            - einmal mit exakter Bewertung aller generierten Moves,
            - einmal mit zufälliger Auswahl von 20 Moves.

            Dadurch kann geprüft werden, ob die Abschätzung gute Moves tatsächlich
            besser vorselektiert als eine zufällige Auswahl oder eine vollständige
            exakte Bewertung.
            */

            return new List<TabuEvaluationVariant>
            {
                new TabuEvaluationVariant(
                    variantName: "Estimate_Top20",
                    category: "MoveSelection",
                    description: "Baseline der Move-Auswahl: Alle Moves werden schnell abgeschätzt, danach werden die besten 20 Kandidaten exakt bewertet.",
                    settingsFactory: (maxIterations, timeLimitSeconds) =>
                    {
                        TabuSearchSettings settings =
                            CreateDefaultSettings(
                                maxIterations,
                                timeLimitSeconds);

                        settings.MoveSelectionMode =
                            MoveSelectionMode.EstimatedTopCandidates;

                        settings.MaxExactEvaluationsPerIteration =
                            20;

                        return settings;
                    },
                    neighborhoodFactory: () => new AllPairSwapNeighborhood(),
                    isBaseline: true),

                new TabuEvaluationVariant(
                    variantName: "NoEstimate_AllExact",
                    category: "MoveSelection",
                    description: "Keine schnelle Move-Abschätzung. Alle generierten Moves werden exakt bewertet, soweit das Zeitlimit dies erlaubt.",
                    settingsFactory: (maxIterations, timeLimitSeconds) =>
                    {
                        TabuSearchSettings settings =
                            CreateDefaultSettings(
                                maxIterations,
                                timeLimitSeconds);

                        settings.MoveSelectionMode =
                            MoveSelectionMode.NoEstimationAllExact;

                        return settings;
                    },
                    neighborhoodFactory: () => new AllPairSwapNeighborhood()),

                new TabuEvaluationVariant(
                    variantName: "NoEstimate_Random20",
                    category: "MoveSelection",
                    description: "Keine schnelle Move-Abschätzung. Pro Iteration werden zufällig 20 Moves ausgewählt und exakt bewertet.",
                    settingsFactory: (maxIterations, timeLimitSeconds) =>
                    {
                        TabuSearchSettings settings =
                            CreateDefaultSettings(
                                maxIterations,
                                timeLimitSeconds);

                        settings.MoveSelectionMode =
                            MoveSelectionMode.NoEstimationRandomCandidates;

                        settings.MaxExactEvaluationsPerIteration =
                            20;

                        settings.RandomMoveSelectionSeed =
                            321;

                        return settings;
                    },
                    neighborhoodFactory: () => new AllPairSwapNeighborhood())
            };
        }

        public static List<TabuEvaluationVariant> CreateAdaptiveExactEvaluationComparisonVariants()
        {
            /*
            Diese Variantenliste vergleicht die bisher besten festen Einstellungen
            für die exakte Move-Bewertung mit der neuen adaptiven Strategie.

            Ziel:
            Die Abschätzung bleibt aktiv, aber die Anzahl exakt bewerteter Kandidaten
            wird nicht mehr fest gewählt. Stattdessen hängt sie von der Anzahl der
            generierten Moves und von der Stagnation ab.

            Dadurch wird geprüft, ob die adaptive Strategie einen besseren Kompromiss
            zwischen Lösungsqualität und Laufzeit bietet als feste Werte wie 50, 100
            oder 200 exakte Bewertungen pro Iteration.
            */

            return new List<TabuEvaluationVariant>
    {
        new TabuEvaluationVariant(
            variantName: "Baseline_Top20",
            category: "AdaptiveExactEvaluation",
            description: "Baseline: Move-Abschätzung aktiv, danach maximal 20 exakt bewertete Kandidaten pro Iteration.",
            settingsFactory: (maxIterations, timeLimitSeconds) =>
            {
                TabuSearchSettings settings =
                    CreateDefaultSettings(
                        maxIterations,
                        timeLimitSeconds);

                settings.MoveSelectionMode =
                    MoveSelectionMode.EstimatedTopCandidates;

                settings.MaxExactEvaluationsPerIteration =
                    20;

                settings.UseAdaptiveExactEvaluationLimit =
                    false;

                return settings;
            },
            neighborhoodFactory: () => new AllPairSwapNeighborhood(),
            isBaseline: true),

        new TabuEvaluationVariant(
            variantName: "FixedExact_50",
            category: "AdaptiveExactEvaluation",
            description: "Move-Abschätzung aktiv, danach maximal 50 exakt bewertete Kandidaten pro Iteration.",
            settingsFactory: (maxIterations, timeLimitSeconds) =>
            {
                TabuSearchSettings settings =
                    CreateDefaultSettings(
                        maxIterations,
                        timeLimitSeconds);

                settings.MoveSelectionMode =
                    MoveSelectionMode.EstimatedTopCandidates;

                settings.MaxExactEvaluationsPerIteration =
                    50;

                settings.UseAdaptiveExactEvaluationLimit =
                    false;

                return settings;
            },
            neighborhoodFactory: () => new AllPairSwapNeighborhood()),

        new TabuEvaluationVariant(
            variantName: "FixedExact_100",
            category: "AdaptiveExactEvaluation",
            description: "Move-Abschätzung aktiv, danach maximal 100 exakt bewertete Kandidaten pro Iteration.",
            settingsFactory: (maxIterations, timeLimitSeconds) =>
            {
                TabuSearchSettings settings =
                    CreateDefaultSettings(
                        maxIterations,
                        timeLimitSeconds);

                settings.MoveSelectionMode =
                    MoveSelectionMode.EstimatedTopCandidates;

                settings.MaxExactEvaluationsPerIteration =
                    100;

                settings.UseAdaptiveExactEvaluationLimit =
                    false;

                return settings;
            },
            neighborhoodFactory: () => new AllPairSwapNeighborhood()),

        new TabuEvaluationVariant(
            variantName: "FixedExact_200",
            category: "AdaptiveExactEvaluation",
            description: "Move-Abschätzung aktiv, danach maximal 200 exakt bewertete Kandidaten pro Iteration.",
            settingsFactory: (maxIterations, timeLimitSeconds) =>
            {
                TabuSearchSettings settings =
                    CreateDefaultSettings(
                        maxIterations,
                        timeLimitSeconds);

                settings.MoveSelectionMode =
                    MoveSelectionMode.EstimatedTopCandidates;

                settings.MaxExactEvaluationsPerIteration =
                    200;

                settings.UseAdaptiveExactEvaluationLimit =
                    false;

                return settings;
            },
            neighborhoodFactory: () => new AllPairSwapNeighborhood()),

        new TabuEvaluationVariant(
            variantName: "AdaptiveExact_50_200",
            category: "AdaptiveExactEvaluation",
            description: "Move-Abschätzung aktiv, adaptive exakte Bewertung: alle Moves bei kleinen Nachbarschaften, sonst mindestens 50, bei Stagnation 100 bis 200, maximal 200 Kandidaten.",
            settingsFactory: (maxIterations, timeLimitSeconds) =>
            {
                TabuSearchSettings settings =
                    CreateDefaultSettings(
                        maxIterations,
                        timeLimitSeconds);

                settings.MoveSelectionMode =
                    MoveSelectionMode.EstimatedTopCandidates;

                settings.UseAdaptiveExactEvaluationLimit =
                    true;

                settings.AdaptiveExactEvaluateAllMoveThreshold =
                    50;

                settings.AdaptiveExactMinEvaluations =
                    50;

                settings.AdaptiveExactMaxEvaluations =
                    200;

                settings.AdaptiveExactMoveFraction =
                    0.25;

                settings.AdaptiveExactMediumStagnationMinEvaluations =
                    100;

                settings.AdaptiveExactHighStagnationMinEvaluations =
                    200;

                return settings;
            },
            neighborhoodFactory: () => new AllPairSwapNeighborhood())
    };
        }

        public static List<TabuEvaluationVariant> CreateNeighborhoodComparisonVariants()
        {
            /*
            Diese Variantenliste ist nur für den Nachbarschaftsvergleich gedacht.

            Es wird bewusst keine kombinierte Nachbarschaft verwendet.
            Dadurch bleibt der Einfluss jeder einzelnen Nachbarschaft klar sichtbar.
            */

            return new List<TabuEvaluationVariant>
            {
                new TabuEvaluationVariant(
                    variantName: "Neighborhood_N1_AdjacentSwap",
                    category: "Neighborhood",
                    description: "N1: Vertauscht benachbarte Operationen innerhalb kritischer Blöcke.",
                    settingsFactory: CreateDefaultSettings,
                    neighborhoodFactory: () => new AdjacentSwapNeighborhood()),

                new TabuEvaluationVariant(
                    variantName: "Neighborhood_N2_RestrictedBlockSwap",
                    category: "Neighborhood",
                    description: "N2: Beschränkte Swaps am Anfang und Ende kritischer Blöcke.",
                    settingsFactory: CreateDefaultSettings,
                    neighborhoodFactory: () => new RestrictedBlockSwapNeighborhood()),

                new TabuEvaluationVariant(
                    variantName: "Neighborhood_N3_AllPairSwap_Baseline",
                    category: "Neighborhood",
                    description: "N3: All-Pair-Swaps innerhalb kritischer Blöcke. Diese Variante entspricht der Baseline-Nachbarschaft.",
                    settingsFactory: CreateDefaultSettings,
                    neighborhoodFactory: () => new AllPairSwapNeighborhood(),
                    isBaseline: true),

                new TabuEvaluationVariant(
                    variantName: "Neighborhood_N4_CriticalBlockInsert",
                    category: "Neighborhood",
                    description: "N4: Insert-Moves innerhalb kritischer Blöcke.",
                    settingsFactory: CreateDefaultSettings,
                    neighborhoodFactory: () => new CriticalBlockInsertNeighborhood()),

                new TabuEvaluationVariant(
                    variantName: "Neighborhood_SetupHeavy",
                    category: "Neighborhood",
                    description: "Setup-heavy Nachbarschaft: erzeugt Moves an Positionen mit hohen Rüstzeiten.",
                    settingsFactory: CreateDefaultSettings,
                    neighborhoodFactory: () => new SetupHeavyMachineNeighborhood())
            };
        }

        private static void AddRestartVariants(
            List<TabuEvaluationVariant> variants)
        {
            /*
            Restart-Varianten prüfen, ob die aktuelle Restart-Strategie
            hilfreich ist und ob die aktuelle Restart-Schwelle sinnvoll gewählt ist.

            Besonders wichtig:
            Die Baseline verwendet operationCount * 25.
            Aus der bisherigen Analyse wissen wir, dass diese Schwelle bei kleinen
            Instanzen sehr niedrig und bei großen Instanzen teilweise zu hoch ist.
            */

            variants.Add(
                new TabuEvaluationVariant(
                    variantName: "Restart_Off",
                    category: "Restart",
                    description: "Restart deaktiviert. Dient als Vergleich, ob die aktuelle Restart-Strategie tatsächlich zur Lösungsqualität beiträgt.",
                    settingsFactory: (maxIterations, timeLimitSeconds) =>
                    {
                        TabuSearchSettings settings =
                            CreateDefaultSettings(
                                maxIterations,
                                timeLimitSeconds);

                        settings.UseRestart =
                            false;

                        return settings;
                    },
                    neighborhoodFactory: () => new AllPairSwapNeighborhood()));

            variants.Add(
                CreateRestartThresholdVariant(
                    "Restart_x10",
                    10,
                    "Aggressive Restart-Schwelle: Restart nach operationCount * 10 Iterationen ohne globale Verbesserung."));

            variants.Add(
                CreateRestartThresholdVariant(
                    "Restart_x50",
                    50,
                    "Konservativere Restart-Schwelle: Restart nach operationCount * 50 Iterationen ohne globale Verbesserung."));

            variants.Add(
                CreateRestartThresholdVariant(
                    "Restart_x100",
                    100,
                    "Sehr konservative Restart-Schwelle: Restart nach operationCount * 100 Iterationen ohne globale Verbesserung."));
        }

        private static TabuEvaluationVariant CreateRestartThresholdVariant(
            string variantName,
            int restartFactor,
            string description)
        {
            /*
            Erzeugt eine Restart-Variante mit anderer Schwelle.

            Alle anderen Parameter bleiben auf Baseline-Niveau.
            Dadurch wird ausschließlich der Einfluss der Restart-Schwelle getestet.
            */

            return new TabuEvaluationVariant(
                variantName: variantName,
                category: "Restart",
                description: description,
                settingsFactory: (maxIterations, timeLimitSeconds) =>
                {
                    TabuSearchSettings settings =
                        CreateDefaultSettings(
                            maxIterations,
                            timeLimitSeconds);

                    settings.UseRestart =
                        true;

                    settings.RestartAfterNoImprovementFactor =
                        restartFactor;

                    return settings;
                },
                neighborhoodFactory: () => new AllPairSwapNeighborhood());
        }

        private static void AddPerturbationVariants(
            List<TabuEvaluationVariant> variants)
        {
            /*
            Diese Varianten verändern nicht die Restart-Schwelle,
            sondern die Stärke der Veränderung nach einem Restart.

            Dadurch wird geprüft, ob die aktuelle Perturbation zu schwach,
            zu stark oder angemessen ist.
            */

            variants.Add(
                new TabuEvaluationVariant(
                    variantName: "Perturb_Weak",
                    category: "Restart",
                    description: "Schwächere Perturbation: max(3, operationCount / 50). Die Startstruktur nach einem Restart bleibt stärker erhalten.",
                    settingsFactory: (maxIterations, timeLimitSeconds) =>
                    {
                        TabuSearchSettings settings =
                            CreateDefaultSettings(
                                maxIterations,
                                timeLimitSeconds);

                        settings.RestartMinPerturbationMoves =
                            3;

                        settings.RestartOperationDivisor =
                            50;

                        return settings;
                    },
                    neighborhoodFactory: () => new AllPairSwapNeighborhood()));

            variants.Add(
                new TabuEvaluationVariant(
                    variantName: "Perturb_Strong",
                    category: "Restart",
                    description: "Stärkere Perturbation: max(5, operationCount / 10). Der Restart verändert die Maschinenreihenfolgen deutlich stärker.",
                    settingsFactory: (maxIterations, timeLimitSeconds) =>
                    {
                        TabuSearchSettings settings =
                            CreateDefaultSettings(
                                maxIterations,
                                timeLimitSeconds);

                        settings.RestartMinPerturbationMoves =
                            5;

                        settings.RestartOperationDivisor =
                            10;

                        return settings;
                    },
                    neighborhoodFactory: () => new AllPairSwapNeighborhood()));
        }

        private static void AddFrequencyPenaltyVariants(
            List<TabuEvaluationVariant> variants)
        {
            /*
            Die Frequency Penalty soll häufig verwendete Moves bestrafen
            und dadurch die Diversifikation der Suche erhöhen.

            Diese Varianten prüfen:
            - ob die Penalty überhaupt hilfreich ist,
            - ob sie zu schwach ist,
            - oder ob sie gute Moves zu stark benachteiligt.
            */

            variants.Add(
                new TabuEvaluationVariant(
                    variantName: "Penalty_Off",
                    category: "FrequencyPenalty",
                    description: "Frequency Penalty deaktiviert. Prüft, ob die Strafkomponente die Suche unterstützt oder gute Moves zu stark benachteiligt.",
                    settingsFactory: (maxIterations, timeLimitSeconds) =>
                    {
                        TabuSearchSettings settings =
                            CreateDefaultSettings(
                                maxIterations,
                                timeLimitSeconds);

                        settings.UseFrequencyPenalty =
                            false;

                        return settings;
                    },
                    neighborhoodFactory: () => new AllPairSwapNeighborhood()));

            variants.Add(
                new TabuEvaluationVariant(
                    variantName: "Penalty_Weak",
                    category: "FrequencyPenalty",
                    description: "Schwächere Frequency Penalty mit geringeren Strafwerten.",
                    settingsFactory: (maxIterations, timeLimitSeconds) =>
                    {
                        TabuSearchSettings settings =
                            CreateDefaultSettings(
                                maxIterations,
                                timeLimitSeconds);

                        settings.UseFrequencyPenalty =
                            true;

                        settings.LowStagnationPenaltyRate =
                            0.002;

                        settings.MediumStagnationPenaltyRate =
                            0.005;

                        settings.HighStagnationPenaltyRate =
                            0.010;

                        return settings;
                    },
                    neighborhoodFactory: () => new AllPairSwapNeighborhood()));

            variants.Add(
                new TabuEvaluationVariant(
                    variantName: "Penalty_Strong",
                    category: "FrequencyPenalty",
                    description: "Stärkere Frequency Penalty mit höheren Strafwerten.",
                    settingsFactory: (maxIterations, timeLimitSeconds) =>
                    {
                        TabuSearchSettings settings =
                            CreateDefaultSettings(
                                maxIterations,
                                timeLimitSeconds);

                        settings.UseFrequencyPenalty =
                            true;

                        settings.LowStagnationPenaltyRate =
                            0.010;

                        settings.MediumStagnationPenaltyRate =
                            0.030;

                        settings.HighStagnationPenaltyRate =
                            0.050;

                        return settings;
                    },
                    neighborhoodFactory: () => new AllPairSwapNeighborhood()));
        }

        private static void AddExactEvaluationVariants(
            List<TabuEvaluationVariant> variants)
        {
            /*
            Die Baseline bewertet pro Iteration nur eine begrenzte Anzahl
            von Kandidaten exakt.

            Diese Varianten testen, ob eine breitere exakte Bewertung zu besseren
            Entscheidungen führt oder ob der zusätzliche Rechenaufwand zu groß wird.
            */

            variants.Add(
                CreateExactEvaluationVariant(
                    "ExactEval_5",
                    5,
                    "Nur 5 Kandidaten werden pro Iteration exakt bewertet. Dies testet eine sehr schnelle, aber grobe Move-Auswahl."));

            variants.Add(
                CreateExactEvaluationVariant(
                    "ExactEval_50",
                    50,
                    "50 Kandidaten werden pro Iteration exakt bewertet. Dies erweitert die Move-Auswahl gegenüber der Baseline."));

            variants.Add(
                CreateExactEvaluationVariant(
                    "ExactEval_100",
                    100,
                    "100 Kandidaten werden pro Iteration exakt bewertet. Dies testet eine deutlich breitere exakte Bewertung."));

            variants.Add(
                CreateExactEvaluationVariant(
                    "ExactEval_200",
                    200,
                    "200 Kandidaten werden pro Iteration exakt bewertet. Dies ist eine sehr breite, aber laufzeitintensive Bewertung."));
        }

        private static TabuEvaluationVariant CreateExactEvaluationVariant(
            string variantName,
            int maxExactEvaluations,
            string description)
        {
            /*
            Erzeugt eine Variante mit anderer Anzahl exakt bewerteter Moves.

            Die schnelle Move-Abschätzung bleibt aktiv.
            Nur die Anzahl der Kandidaten, die danach exakt bewertet werden,
            wird verändert.
            */

            return new TabuEvaluationVariant(
                variantName: variantName,
                category: "ExactEvaluation",
                description: description,
                settingsFactory: (maxIterations, timeLimitSeconds) =>
                {
                    TabuSearchSettings settings =
                        CreateDefaultSettings(
                            maxIterations,
                            timeLimitSeconds);

                    settings.MaxExactEvaluationsPerIteration =
                        maxExactEvaluations;

                    return settings;
                },
                neighborhoodFactory: () => new AllPairSwapNeighborhood());
        }

        private static void AddTabuTenureVariants(
            List<TabuEvaluationVariant> variants)
        {
            /*
            Die Tabu-Dauer beeinflusst, wie lange Rückbewegungen verboten bleiben.

            Eine zu kurze Tabu-Dauer kann zu Zyklen führen.
            Eine zu lange Tabu-Dauer kann gute Moves unnötig blockieren.
            Deshalb werden kurze, lange und stärker dynamische Varianten getestet.
            */

            variants.Add(
                new TabuEvaluationVariant(
                    variantName: "Tabu_Short",
                    category: "TabuTenure",
                    description: "Kurze dynamische Tabu-Dauer mit Faktoren 0.5 bis 0.8.",
                    settingsFactory: (maxIterations, timeLimitSeconds) =>
                    {
                        TabuSearchSettings settings =
                            CreateDefaultSettings(
                                maxIterations,
                                timeLimitSeconds);

                        settings.MinTenureFactor =
                            0.5;

                        settings.MaxTenureFactor =
                            0.8;

                        return settings;
                    },
                    neighborhoodFactory: () => new AllPairSwapNeighborhood()));

            variants.Add(
                new TabuEvaluationVariant(
                    variantName: "Tabu_Long",
                    category: "TabuTenure",
                    description: "Längere dynamische Tabu-Dauer mit Faktoren 1.2 bis 2.0.",
                    settingsFactory: (maxIterations, timeLimitSeconds) =>
                    {
                        TabuSearchSettings settings =
                            CreateDefaultSettings(
                                maxIterations,
                                timeLimitSeconds);

                        settings.MinTenureFactor =
                            1.2;

                        settings.MaxTenureFactor =
                            2.0;

                        return settings;
                    },
                    neighborhoodFactory: () => new AllPairSwapNeighborhood()));

            variants.Add(
                new TabuEvaluationVariant(
                    variantName: "Tabu_WideDynamic",
                    category: "TabuTenure",
                    description: "Stärker schwankende dynamische Tabu-Dauer mit Faktoren 0.5 bis 2.0.",
                    settingsFactory: (maxIterations, timeLimitSeconds) =>
                    {
                        TabuSearchSettings settings =
                            CreateDefaultSettings(
                                maxIterations,
                                timeLimitSeconds);

                        settings.MinTenureFactor =
                            0.5;

                        settings.MaxTenureFactor =
                            2.0;

                        return settings;
                    },
                    neighborhoodFactory: () => new AllPairSwapNeighborhood()));
        }

        private static void AddNeighborhoodVariants(
            List<TabuEvaluationVariant> variants)
        {
            /*
            Fügt die einzelnen Nachbarschaftsvarianten zur Screening-Liste hinzu.

            Die Baseline-Nachbarschaft N3 wird hier nicht erneut ergänzt,
            weil die Baseline bereits am Anfang der Liste enthalten ist.
            */

            variants.AddRange(
                CreateNeighborhoodComparisonVariants()
                    .FindAll(variant => !variant.IsBaseline));
        }

        private static TabuSearchSettings CreateDefaultSettings(
            int maxIterations,
            int timeLimitSeconds)
        {
            /*
            Diese Methode bildet die aktuelle Standardkonfiguration der Tabu Search ab.

            Alle experimentellen Varianten starten von diesen Settings
            und verändern danach gezielt nur eine Komponente.

            Dadurch bleibt die Evaluation kontrolliert:
            Eine Variante unterscheidet sich immer nur in dem Parameter,
            der gerade untersucht werden soll.
            */

            TabuSearchSettings settings =
                TabuSearchSettings.CreateDefault(
                    maxIterations,
                    timeLimitSeconds);

            settings.VerboseOutput =
                false;

            return settings;
        }
    }
}