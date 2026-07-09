using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Evaluation
{
    /*
    Bewertet einen Move exakt.

    Die schnelle Move-Abschätzung dient nur dazu, viele mögliche Moves günstig
    vorzusortieren. Für die endgültige Auswahl reicht diese Schätzung aber nicht aus,
    weil ein Move die gesamte zeitliche Struktur des Schedules verändern kann.

    Deshalb wird hier der Move wirklich angewendet:
    - aktuelle Maschinenreihenfolge kopieren,
    - Move auf der Kopie ausführen,
    - vollständigen Schedule neu berechnen,
    - prüfen, ob die neue Maschinenreihenfolge zulässig ist,
    - tatsächlichen Cmax zurückgeben.
    */
    public static class MoveExactEvaluator
    {
        public static MoveExactEvaluationResult Evaluate(
            Instance instance,
            Dictionary<int, List<Operation>> currentOrders,
            Move move)
        {
            /*
            Die aktuelle Maschinenreihenfolge darf nicht direkt verändert werden.
            Jeder Kandidat wird deshalb auf einer Kopie getestet.
            */
            Dictionary<int, List<Operation>> candidateOrders =
                ScheduleOrderHelper.CopyMachineOrders(
                    currentOrders);

            /*
            Der Move verändert nur die Reihenfolge der Operationen auf einer Maschine.
            Start- und Endzeiten werden danach nicht übernommen, sondern neu berechnet.
            */
            MoveApplier.Apply(
                candidateOrders,
                move);

            /*
            Aus der neuen Maschinenreihenfolge wird ein kompletter Schedule berechnet.
            Dabei werden Job-Reihenfolgen, Maschinenreihenfolgen und Setup-Zeiten
            berücksichtigt. Falls durch den Move ein Zyklus entsteht, ist der Kandidat
            unzulässig.
            */
            bool isFeasible =
                ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                    instance,
                    candidateOrders,
                    out int candidateCmax);

            return new MoveExactEvaluationResult
            {
                IsFeasible = isFeasible,
                Cmax = candidateCmax,
                MachineOrders = candidateOrders
            };
        }
    }
}