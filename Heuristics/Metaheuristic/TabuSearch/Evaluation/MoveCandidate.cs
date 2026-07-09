using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Evaluation
{
    /*
    Speichert Bewertungsinformationen zu einem einzelnen Move.

    Diese Klasse ist vor allem für die Analyse hilfreich:
    Zuerst wird ein Move schnell abgeschätzt. Nur ausgewählte Moves werden danach
    exakt bewertet. Dadurch kann später verglichen werden, ob die schnelle
    Abschätzung gute Kandidaten wirklich zuverlässig auswählt.
    */
    public class MoveCandidate
    {
        public MoveCandidate(
            Move move)
        {
            Move = move;
        }

        // Move, der bewertet wird.
        public Move Move { get; }

        /*
        Bewertungswert aus der schnellen Abschätzung.

        Dieser Wert kann bereits eine Frequency Penalty enthalten und dient dazu,
        vielversprechende Moves für die exakte Bewertung vorzuselektieren.
        */
        public double EstimatedEvaluationValue { get; set; }

        // Gibt an, ob für diesen Move eine exakte Schedule-Neuberechnung durchgeführt wurde.
        public bool ExactEvaluationPerformed { get; set; }

        // Gibt an, ob der Move nach exakter Bewertung zu einem zulässigen Schedule führte.
        public bool IsFeasible { get; set; }

        // Gibt an, ob der Move durch die Tabu-Liste blockiert war.
        public bool IsTabu { get; set; }

        // Tatsächlicher Cmax nach exakter Anwendung des Moves.
        public int ExactCmax { get; set; }

        /*
        Exakter Bewertungswert nach Anwendung des Moves.

        Dieser Wert kann vom ExactCmax abweichen, wenn zusätzliche Strafkomponenten
        wie die Frequency Penalty verwendet werden.
        */
        public double ExactEvaluationValue { get; set; }
    }
}