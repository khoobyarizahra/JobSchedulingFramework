using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Evaluation
{
    /*
    Ergebnis der Move-Auswahl innerhalb einer Tabu-Search-Iteration.

    Die Klasse speichert den besten zulässigen Move, die dazugehörige
    Maschinenreihenfolge und Kennzahlen zur Bewertung der Iteration.
    Dadurch kann später analysiert werden, wie viele Kandidaten exakt geprüft,
    verworfen oder durch Tabu-Regeln blockiert wurden.
    */
    public class MoveSelectionResult
    {
        // Bester zulässiger Move, der in dieser Iteration ausgewählt wurde.
        public Move? BestMove { get; set; }

        // Cmax der Lösung, die durch den besten Move entsteht.
        public int BestCandidateCmax { get; set; } = int.MaxValue;

        /*
        Bewertungswert des besten Kandidaten.

        Dieser Wert kann vom reinen Cmax abweichen, wenn z.B. eine Frequency Penalty
        verwendet wird. Dadurch kann die Suche häufig genutzte Moves bestrafen.
        */
        public double BestCandidateEvaluationValue { get; set; } = double.MaxValue;

        // Maschinenreihenfolge nach Anwendung des besten Moves.
        public Dictionary<int, List<Operation>>? BestCandidateOrders { get; set; }

        // Anzahl der Moves, die in dieser Iteration exakt bewertet wurden.
        public int ExactEvaluations { get; set; }

        // Anzahl der exakt bewerteten Kandidaten, die zulässig waren.
        public int FeasibleCandidates { get; set; }

        // Anzahl der exakt bewerteten Kandidaten, die zu keinem zulässigen Schedule führten.
        public int InfeasibleCandidates { get; set; }

        // Anzahl der Moves, die wegen Tabu-Regeln nicht verwendet wurden.
        public int TabuRejectedMoves { get; set; }

        public bool HasAdmissibleMove
        {
            get
            {
                return BestMove != null &&
                       BestCandidateOrders != null;
            }
        }
    }
}