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
        Anzahl der Moves, die pro Iteration exakt bewertet werden.

        Die Nachbarschaft kann sehr viele Moves erzeugen. Deshalb werden zuerst
        alle Moves schnell abgeschätzt. Anschließend werden nur die besten Kandidaten
        exakt geprüft, indem der Schedule wirklich neu berechnet wird.

        Ein kleiner Wert spart Laufzeit, kann aber gute Moves übersehen.
        Ein großer Wert verbessert die Move-Auswahl, erhöht aber die Laufzeit.
        */
        public int MaxExactEvaluationsPerIteration { get; set; } = 20;

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
    }
}