namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Restart
{
    /*
    Speichert die Kennzahlen eines einzelnen Restarts.

    Ein Restart gilt erst dann als erfolgreich, wenn danach eine neue globale
    Bestlösung gefunden wird. Deshalb werden Zeitpunkt und Cmax des Restarts
    sofort gespeichert, während der spätere Erfolg nachträglich markiert wird.
    */
    public class RestartEvent
    {
        // Iteration, in der der Restart ausgelöst wurde.
        public int Iteration { get; set; }

        // Cmax der aktuellen Lösung direkt vor dem Restart.
        public int CmaxBeforeRestart { get; set; }

        // Cmax der neu berechneten Lösung direkt nach dem Restart.
        public int CmaxAfterRestart { get; set; }

        // Beste globale Lösung, die bis zum Restart bekannt war.
        public int BestCmaxBeforeRestart { get; set; }

        // Anzahl der zufälligen Swaps, die für die Perturbation verwendet wurden.
        public int PerturbationMoves { get; set; }

        /*
        true bedeutet:
        Es konnte keine zulässige perturbierte Lösung erzeugt werden.
        Deshalb wurde auf die beste bekannte Lösung zurückgegriffen.
        */
        public bool UsedFallbackSolution { get; set; }

        // Wird true, wenn nach diesem Restart eine neue globale Bestlösung gefunden wurde.
        public bool LedToNewBestSolution { get; set; }

        // Iteration, in der nach dem Restart die nächste globale Verbesserung gefunden wurde.
        public int? IterationOfNextImprovement { get; set; }

        // Neuer bester Cmax, der nach diesem Restart erreicht wurde.
        public int? BestCmaxAfterImprovement { get; set; }
    }
}