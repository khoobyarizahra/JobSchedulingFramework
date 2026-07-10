namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /*
    Zentrale Konfiguration der Tabu Search.

    In dieser Klasse werden alle wichtigen Parameter gesammelt, die Qualität und
    Laufzeit der Suche beeinflussen. Dadurch müssen Parameter nicht direkt im
    Solver geändert werden und verschiedene Varianten können einfacher verglichen
    werden, z.B. mit oder ohne Restart, mit anderer Tabu-Dauer oder mit mehr
    exakten Move-Bewertungen pro Iteration.
    */
    public class TabuSearchSettings
    {
        /*
        Abbruchkriterien der Suche.

        MaxIterations begrenzt die Anzahl der Iterationen.
        TimeLimitSeconds wird für den 90-Sekunden-Modus verwendet.
        ExtendedModeSafetyTimeLimitSeconds verhindert, dass ein Lauf ohne festes
        Zeitlimit unbegrenzt lange läuft.
        */
        public int MaxIterations { get; set; }
        public int TimeLimitSeconds { get; set; }
        public int ExtendedModeSafetyTimeLimitSeconds { get; set; } = 300;

        /*
        Steuerung der Move-Auswahl.

        Die bisherige Baseline verwendet EstimatedTopCandidates:
        Alle Moves werden schnell abgeschätzt und anschließend werden nur die
        besten Kandidaten exakt bewertet.

        Für die Evaluation kann die Abschätzung vollständig deaktiviert werden.
        Dadurch kann untersucht werden, ob die Schätzung gute Moves zuverlässig
        erkennt oder ob sie gute Kandidaten zu früh aussortiert.
        */
        public MoveSelectionMode MoveSelectionMode { get; set; } =
            MoveSelectionMode.EstimatedTopCandidates;

        /*
        Anzahl der Moves, die pro Iteration exakt bewertet werden.

        Bei EstimatedTopCandidates werden nach der schnellen Abschätzung nur die
        besten Kandidaten exakt geprüft.

        Bei NoEstimationRandomCandidates wird diese Anzahl für die zufällige
        Kandidatenauswahl verwendet.

        Bei NoEstimationAllExact wird dieser Wert ignoriert, weil alle generierten
        Moves exakt bewertet werden.
        */
        public int MaxExactEvaluationsPerIteration { get; set; } = 20;

        /*
        Adaptive exakte Move-Bewertung.

        Wenn diese Option aktiviert ist, wird die Anzahl exakt bewerteter Kandidaten
        nicht mehr als fixer Wert verwendet. Stattdessen hängt sie von der Anzahl der
        generierten Moves und von der aktuellen Stagnation der Suche ab.

        Die Idee basiert auf der Evaluation:
        Die schnelle Abschätzung ist nützlich, aber 20 exakte Bewertungen sind auf
        größeren Instanzen teilweise zu restriktiv. Deshalb wird bei größeren
        Nachbarschaften und längerer Stagnation eine breitere exakte Nachbewertung
        verwendet.
        */
        public bool UseAdaptiveExactEvaluationLimit { get; set; } = false;

        /*
        Wenn nur wenige Moves erzeugt werden, sollen alle exakt bewertet werden.
        Dadurch geht kein Kandidat durch eine unnötige Vorauswahl verloren.
        */
        public int AdaptiveExactEvaluateAllMoveThreshold { get; set; } = 50;

        /*
        Mindestanzahl exakt bewerteter Kandidaten im adaptiven Modus.

        Dieser Wert verhindert, dass bei größeren Nachbarschaften zu wenige Moves
        exakt geprüft werden.
        */
        public int AdaptiveExactMinEvaluations { get; set; } = 50;

        /*
        Obergrenze für exakte Bewertungen im adaptiven Modus.

        Diese Grenze verhindert, dass einzelne Iterationen zu teuer werden.
        */
        public int AdaptiveExactMaxEvaluations { get; set; } = 200;

        /*
        Anteil der generierten Moves, der im normalen Suchzustand exakt bewertet
        werden soll. Ein Wert von 0.25 bedeutet, dass ungefähr 25 Prozent der
        generierten Moves exakt geprüft werden.
        */
        public double AdaptiveExactMoveFraction { get; set; } = 0.25;

        /*
        Mindestanzahl exakt bewerteter Kandidaten bei mittlerer und hoher Stagnation.

        Wenn längere Zeit keine neue globale Bestlösung gefunden wird, wird die
        Move-Auswahl intensiver. Dadurch kann die Suche bessere Kandidaten finden,
        ohne die Abschätzung vollständig zu deaktivieren.
        */
        public int AdaptiveExactMediumStagnationMinEvaluations { get; set; } = 100;
        public int AdaptiveExactHighStagnationMinEvaluations { get; set; } = 200;

        /*
        Seed für die zufällige Move-Auswahl ohne Abschätzung.

        Der feste Seed macht die Evaluation reproduzierbar. Dadurch kann ein Lauf
        mit gleicher Konfiguration erneut ausgeführt und besser verglichen werden.
        */
        public int RandomMoveSelectionSeed { get; set; } = 321;

        /*
        Parameter der Restart-Strategie.

        Ein Restart wird ausgelöst, wenn über viele Iterationen keine neue globale
        Bestlösung gefunden wurde. Die Schwelle wird abhängig von der Instanzgröße
        berechnet:

        RestartThreshold = OperationCount * RestartAfterNoImprovementFactor

        Die Perturbationsstärke wird über RestartOperationDivisor bestimmt.
        */
        public bool UseRestart { get; set; } = true;
        public int RestartAfterNoImprovementFactor { get; set; } = 25;
        public int RestartMinPerturbationMoves { get; set; } = 3;
        public int RestartOperationDivisor { get; set; } = 20;
        public int MaxRestartAttempts { get; set; } = 20;
        public int RestartRandomSeed { get; set; } = 123;

        /*
        Parameter der Frequency Penalty.

        Die Frequency Penalty bestraft Moves, die bereits häufig verwendet wurden.
        Dadurch wird verhindert, dass die Suche immer wieder dieselben Bewegungen
        bevorzugt. Bei längerer Stagnation wird die Strafe erhöht, um die Suche
        stärker zu diversifizieren.

        Für Experimente kann diese Komponente vollständig deaktiviert werden.
        */
        public bool UseFrequencyPenalty { get; set; } = true;
        public int MediumStagnationThreshold { get; set; } = 300;
        public int HighStagnationThreshold { get; set; } = 1000;
        public double LowStagnationPenaltyRate { get; set; } = 0.005;
        public double MediumStagnationPenaltyRate { get; set; } = 0.015;
        public double HighStagnationPenaltyRate { get; set; } = 0.03;

        /*
        Aspiration erlaubt einen tabu Move trotzdem, wenn dadurch eine neue globale
        Bestlösung erreicht wird. Dadurch verhindert die Tabu-Liste nicht, dass
        klare Verbesserungen angenommen werden.
        */
        public bool UseAspiration { get; set; } = true;

        /*
        Parameter der Tabu-Dauer.

        Die Tabu-Dauer legt fest, wie lange ein Rückmove verboten bleibt.
        Eine kurze Dauer erlaubt mehr Flexibilität, kann aber Zyklen begünstigen.
        Eine lange Dauer diversifiziert stärker, kann aber gute Moves blockieren.

        Die Faktoren 0.8 und 1.2 steuern die zufällige Schwankung um die Basis-Tenure.
        */
        public int TabuRandomSeed { get; set; } = 42;
        public double MinTenureFactor { get; set; } = 0.8;
        public double MaxTenureFactor { get; set; } = 1.2;
        public int MinBaseTenure { get; set; } = 3;
        public int MinTenureUpdateInterval { get; set; } = 1000;
        public int TenureUpdateOperationFactor { get; set; } = 5;

        /*
        Konsolenausgabe während der Suche.

        Für finale Benchmarkläufe sollte VerboseOutput meist deaktiviert bleiben,
        damit die Laufzeit nicht unnötig durch Konsolenausgaben beeinflusst wird.
        */
        public bool VerboseOutput { get; set; } = false;
        public int VerbosePrintInterval { get; set; } = 1000;

        public static TabuSearchSettings CreateDefault(
            int maxIterations,
            int timeLimitSeconds)
        {
            /*
            Standardkonfiguration für einen normalen Tabu-Search-Lauf.
            Spezielle Experimente können danach einzelne Werte überschreiben.
            */
            return new TabuSearchSettings
            {
                MaxIterations = maxIterations,
                TimeLimitSeconds = timeLimitSeconds
            };
        }

        public static TabuSearchSettings CreateAdaptiveDefault(
    int maxIterations,
    int timeLimitSeconds)
        {
            /*
            Verbesserte Standardkonfiguration nach der experimentellen Evaluation.

            Die Move-Abschätzung bleibt aktiv, weil sie bessere Kandidaten auswählt
            als eine zufällige Auswahl. Die Anzahl exakt bewerteter Kandidaten wird aber
            nicht mehr starr auf 20 begrenzt, sondern adaptiv gewählt.

            Dadurch wird bei größeren Nachbarschaften und längerer Stagnation intensiver
            gesucht, während kleine Nachbarschaften weiterhin effizient behandelt werden.
            */
            return new TabuSearchSettings
            {
                MaxIterations =
                    maxIterations,

                TimeLimitSeconds =
                    timeLimitSeconds,

                MoveSelectionMode =
                    MoveSelectionMode.EstimatedTopCandidates,

                MaxExactEvaluationsPerIteration =
                    200,

                UseAdaptiveExactEvaluationLimit =
                    true,

                AdaptiveExactEvaluateAllMoveThreshold =
                    50,

                AdaptiveExactMinEvaluations =
                    50,

                AdaptiveExactMaxEvaluations =
                    200,

                AdaptiveExactMoveFraction =
                    0.25,

                AdaptiveExactMediumStagnationMinEvaluations =
                    100,

                AdaptiveExactHighStagnationMinEvaluations =
                    200,

                VerboseOutput =
                    false
            };
        }
    }
}