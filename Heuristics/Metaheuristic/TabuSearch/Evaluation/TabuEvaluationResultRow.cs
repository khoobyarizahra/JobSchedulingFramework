namespace JobShopSchedulingFramework.Evaluation
{
    public sealed class TabuEvaluationResultRow
    {
        /*
        Diese Klasse beschreibt eine einzelne Ergebniszeile der neuen
        experimentellen Evaluation.

        Im Unterschied zur ursprünglichen Abgabe-CSV werden hier nicht nur
        Cmax und Laufzeit gespeichert, sondern auch interne Suchkennzahlen.
        Dadurch können die experimentellen Varianten später wissenschaftlich
        miteinander verglichen werden.

        Beispiele:
        - Hat eine Variante mehr Restarts ausgelöst?
        - Wurde das Zeitlimit oder das Iterationslimit erreicht?
        - Hat eine höhere Anzahl exakt bewerteter Moves bessere Lösungen erzeugt?
        - Hat eine andere Nachbarschaft mehr Verbesserungen gefunden?
        */

        public string InstanceName { get; set; } =
            "";

        public string VariantName { get; set; } =
            "";

        public string Category { get; set; } =
            "";

        public string RunMode { get; set; } =
            "";

        public string NeighborhoodName { get; set; } =
            "";

        public bool IsBaseline { get; set; }

        public int MultiStartCount { get; set; } =
            1;

        /*
        Diese Werte beschreiben die äußeren Laufbedingungen.
        Sie sind wichtig, damit später klar ist, ob ein Ergebnis aus dem
        90s-Modus oder aus dem Extended-Modus stammt.
        */

        public int MaxIterations { get; set; }

        public int TimeLimitSeconds { get; set; }

        /*
        Diese Werte beschreiben die Startlösung.
        So kann später getrennt bewertet werden, wie gut die Initialheuristik
        war und wie stark die Tabu Search diese Lösung verbessern konnte.
        */

        public string InitialRule { get; set; } =
            "";

        public int InitialCmax { get; set; }

        /*
        Diese Werte beschreiben das finale Ergebnis der Tabu Search.
        BestCmax ist die wichtigste Zielgröße der Evaluation.
        */

        public int BestCmax { get; set; }

        public double ImprovementPercent { get; set; }

        public double RuntimeSeconds { get; set; }

        public string StopReason { get; set; } =
            "";

        /*
        Diese Werte beschreiben den Suchverlauf.
        Sie helfen zu verstehen, ob eine Variante nur länger gerechnet hat
        oder tatsächlich ein anderes Suchverhalten erzeugt.
        */

        public int Iterations { get; set; }

        public int GeneratedMoves { get; set; }

        public int EstimatedMoves { get; set; }

        public int ExactEvaluations { get; set; }

        public int FeasibleCandidates { get; set; }

        public int InfeasibleCandidates { get; set; }

        public int TabuRejectedMoves { get; set; }

        public int AppliedMoves { get; set; }

        public int Improvements { get; set; }

        /*
        Diese Werte beschreiben die Restart-Strategie.
        Gerade für eure Analyse ist das wichtig, weil die aktuelle Restart-Logik
        bei kleinen und großen Instanzen sehr unterschiedlich wirkt.
        */

        public bool RestartEnabled { get; set; }

        public int RestartThreshold { get; set; }

        public int RestartPerturbationMoves { get; set; }

        public int Restarts { get; set; }

        public int SuccessfulRestarts { get; set; }

        public int FallbackRestarts { get; set; }

        public double RestartSuccessRate { get; set; }

        /*
        Diese Werte beschreiben weitere Parameter der Tabu Search.
        Dadurch kann später jede CSV-Zeile eindeutig einer Parameterkonfiguration
        zugeordnet werden.
        */

        public bool FrequencyPenaltyEnabled { get; set; }

        public double LowPenaltyRate { get; set; }

        public double MediumPenaltyRate { get; set; }

        public double HighPenaltyRate { get; set; }

        public int MaxExactEvaluationsPerIteration { get; set; }

        public double TabuMinTenureFactor { get; set; }

        public double TabuMaxTenureFactor { get; set; }
    }
}