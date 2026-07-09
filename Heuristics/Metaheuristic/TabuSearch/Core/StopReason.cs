namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /*
    Beschreibt, warum ein Tabu-Search-Lauf beendet wurde.

    Der Abbruchgrund ist für die spätere Auswertung wichtig. Ein schlechter Cmax
    ist anders zu interpretieren, wenn die Suche wegen Zeitlimit stoppt, als wenn
    keine zulässigen Moves mehr gefunden wurden.
    */
    public enum StopReason
    {
        // Die Suche läuft noch oder es wurde noch kein Abbruchgrund gesetzt.
        NotStopped,

        // Die maximale Anzahl an Iterationen wurde erreicht.
        MaxIterationsReached,

        // Das festgelegte Zeitlimit wurde erreicht.
        TimeLimitReached,

        // Das technische Sicherheitslimit des Extended-Modus wurde erreicht.
        ExtendedSafetyLimitReached,

        // Die Nachbarschaft hat keine Moves erzeugt.
        NoMovesGenerated,

        // Nach der schnellen Bewertung waren keine Kandidaten verfügbar.
        NoEstimatedMoves,

        // Es wurde kein zulässiger und anwendbarer Move gefunden.
        NoAdmissibleMoveFound,

        // Die Startlösung konnte nicht in einen zulässigen Schedule überführt werden.
        InitialSolutionInfeasible
    }
}