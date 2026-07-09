namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /*
    Legt fest, wie Moves vor der exakten Bewertung ausgewählt werden.

    Damit kann experimentell untersucht werden, ob die schnelle Move-Abschätzung
    tatsächlich hilfreich ist oder ob bessere Ergebnisse entstehen, wenn die
    Abschätzung vollständig deaktiviert wird.
    */
    public enum MoveSelectionMode
    {
        /*
        Standardverhalten der bisherigen Tabu Search.

        Alle generierten Moves werden zuerst schnell abgeschätzt. Danach werden
        nur die besten Kandidaten exakt bewertet.
        */
        EstimatedTopCandidates,

        /*
        Die schnelle Abschätzung wird vollständig übersprungen.

        Alle generierten Moves werden exakt bewertet. Diese Variante ist deutlich
        rechenintensiver, liefert aber den direktesten Vergleich zur Abschätzung.
        */
        NoEstimationAllExact,

        /*
        Die schnelle Abschätzung wird vollständig übersprungen.

        Stattdessen werden zufällig einige Moves ausgewählt und exakt bewertet.
        Dadurch kann geprüft werden, ob die Abschätzung besser ist als eine
        zufällige Kandidatenauswahl.
        */
        NoEstimationRandomCandidates
    }
}