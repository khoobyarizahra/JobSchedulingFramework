using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Evaluation
{
    /*
    Ergebnis einer exakten Move-Bewertung.

    Enthält, ob der Move zu einem zulässigen Schedule geführt hat, welchen Cmax
    dieser Schedule besitzt und welche Maschinenreihenfolge daraus entstanden ist.
    */
    public class MoveExactEvaluationResult
    {
        // Gibt an, ob aus der neuen Maschinenreihenfolge ein gültiger Schedule berechnet werden konnte.
        public bool IsFeasible { get; set; }

        // Tatsächlicher Cmax nach Anwendung und Neuberechnung des Moves.
        public int Cmax { get; set; }

        // Maschinenreihenfolge nach Anwendung des Moves.
        public Dictionary<int, List<Operation>> MachineOrders { get; set; } =
            new Dictionary<int, List<Operation>>();
    }
}