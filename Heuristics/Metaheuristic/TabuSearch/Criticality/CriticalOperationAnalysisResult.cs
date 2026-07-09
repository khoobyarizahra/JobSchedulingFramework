using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality
{
    /*
    Ergebnis der Analyse kritischer Operationen.

    Die Klasse speichert die berechneten r_i-Werte, q_i-Werte, kritischen
    Operationen und den aktuellen Cmax. Diese Informationen werden später für
    die Bildung kritischer Blöcke und für die Move-Abschätzung verwendet.

    Hinweis zum Refactoring:
    Die PascalCase-Properties sind die saubere neue Schreibweise.
    Die kleingeschriebenen Properties bleiben vorübergehend erhalten, damit
    bestehende Klassen während des Refactorings weiter kompilieren.
    */
    public class CriticalOperationAnalysisResult
    {
        // r_i: frühestmöglicher Startzeitpunkt jeder Operation.
        public Dictionary<Operation, int> ReleaseTimes { get; set; }

        // q_i: längster Restpfad ab der Operation inklusive eigener Bearbeitungszeit.
        public Dictionary<Operation, int> Tails { get; set; }

        // Menge aller Operationen, für die r_i + q_i == Cmax gilt.
        public HashSet<Operation> CriticalOperations { get; set; }

        // Makespan des aktuell betrachteten Schedules.
        public int Cmax { get; set; }

        public CriticalOperationAnalysisResult()
        {
            ReleaseTimes = new Dictionary<Operation, int>();
            Tails = new Dictionary<Operation, int>();
            CriticalOperations = new HashSet<Operation>();
            Cmax = 0;
        }

        /*
        Kompatibilitäts-Properties für ältere Code-Stellen.

        Diese Properties können später entfernt werden, wenn alle Klassen auf
        ReleaseTimes, Tails, CriticalOperations und Cmax umgestellt wurden.
        */
        public Dictionary<Operation, int> releaseTimes
        {
            get { return ReleaseTimes; }
            set { ReleaseTimes = value; }
        }

        public Dictionary<Operation, int> tails
        {
            get { return Tails; }
            set { Tails = value; }
        }

        public HashSet<Operation> criticalOperations
        {
            get { return CriticalOperations; }
            set { CriticalOperations = value; }
        }

        public int cmax
        {
            get { return Cmax; }
            set { Cmax = value; }
        }
    }
}