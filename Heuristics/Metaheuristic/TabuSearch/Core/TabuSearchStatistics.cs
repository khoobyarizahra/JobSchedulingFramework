using System;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /*
    Sammelt Kennzahlen während eines Tabu-Search-Laufs.

    Diese Klasse beeinflusst die Suche nicht direkt. Sie dient dazu, das Verhalten
    der Tabu Search später auswerten zu können. Dadurch kann untersucht werden,
    ob schlechte Ergebnisse eher durch das Zeitlimit, zu viele generierte Moves,
    zu wenige exakte Bewertungen, unzulässige Kandidaten oder zu strenge Tabu-Regeln
    verursacht wurden.
    */
    public class TabuSearchStatistics
    {
        // Anzahl der tatsächlich ausgeführten Iterationen.
        public int Iterations { get; set; }

        // Anzahl aller Moves, die durch die Nachbarschaftsgenerierung erzeugt wurden.
        public int GeneratedMoves { get; set; }

        // Anzahl der Moves, die mit der schnellen Abschätzung bewertet wurden.
        public int EstimatedMoves { get; set; }

        // Anzahl der Moves, die durch vollständige Schedule-Neuberechnung exakt bewertet wurden.
        public int ExactEvaluations { get; set; }

        // Anzahl der exakt bewerteten Moves, die zu einem zulässigen Schedule geführt haben.
        public int FeasibleCandidates { get; set; }

        // Anzahl der exakt bewerteten Moves, die zu keiner zulässigen Maschinenreihenfolge geführt haben.
        public int InfeasibleCandidates { get; set; }

        // Anzahl der Moves, die wegen Tabu-Regeln verworfen wurden.
        public int TabuRejectedMoves { get; set; }

        // Anzahl der Moves, die tatsächlich auf die aktuelle Lösung angewendet wurden.
        public int AppliedMoves { get; set; }

        // Anzahl der globalen Verbesserungen während der Suche.
        public int Improvements { get; set; }

        // Anzahl der durchgeführten Restarts.
        public int Restarts { get; set; }

        /*
        Grund, warum die Tabu Search beendet wurde.

        Diese Information ist für die Bewertung wichtig:
        Ein Lauf, der wegen Zeitlimit stoppt, ist anders zu interpretieren als ein
        Lauf, der wegen fehlender zulässiger Moves beendet wurde.
        */
        public StopReason StopReason { get; set; } = StopReason.NotStopped;

        // Gesamtlaufzeit des Tabu-Search-Laufs.
        public TimeSpan Runtime { get; set; } = TimeSpan.Zero;

        public double RuntimeSeconds
        {
            get { return Runtime.TotalSeconds; }
        }

        /*
        Durchschnittliche Anzahl erzeugter Moves pro Iteration.

        Ein hoher Wert kann erklären, warum die Laufzeit steigt, besonders wenn
        zusätzlich viele Kandidaten exakt bewertet werden.
        */
        public double AverageGeneratedMovesPerIteration
        {
            get
            {
                if (Iterations == 0)
                {
                    return 0.0;
                }

                return (double)GeneratedMoves / Iterations;
            }
        }

        /*
        Durchschnittliche Anzahl exakt bewerteter Moves pro Iteration.

        Dieser Wert zeigt, wie intensiv die Suche pro Iteration prüft.
        Mehr exakte Bewertungen können bessere Moves finden, erhöhen aber die Laufzeit.
        */
        public double AverageExactEvaluationsPerIteration
        {
            get
            {
                if (Iterations == 0)
                {
                    return 0.0;
                }

                return (double)ExactEvaluations / Iterations;
            }
        }

        /*
        Anteil der exakt bewerteten Kandidaten, die zulässig waren.

        Eine niedrige Rate kann darauf hinweisen, dass viele Moves zyklische oder
        anderweitig unzulässige Maschinenreihenfolgen erzeugen.
        */
        public double FeasibleCandidateRate
        {
            get
            {
                int evaluatedCandidates =
                    FeasibleCandidates + InfeasibleCandidates;

                if (evaluatedCandidates == 0)
                {
                    return 0.0;
                }

                return (double)FeasibleCandidates / evaluatedCandidates;
            }
        }
    }
}