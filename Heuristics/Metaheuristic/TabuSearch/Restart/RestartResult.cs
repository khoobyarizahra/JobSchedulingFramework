using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Restart
{
    /*
    Ergebnis eines Restart-Versuchs.

    Ein Restart liefert eine neue Maschinenreihenfolge und den daraus berechneten
    Cmax. Falls keine zulässige perturbierte Lösung gefunden wurde, wird markiert,
    dass auf die beste bekannte Lösung zurückgegriffen wurde.
    */
    public class RestartResult
    {
        // Maschinenreihenfolge, mit der die Tabu Search nach dem Restart weiterläuft.
        public Dictionary<int, List<Operation>> MachineOrders { get; set; } =
            new Dictionary<int, List<Operation>>();

        // Cmax der nach dem Restart neu berechneten Lösung.
        public int Cmax { get; set; }

        /*
        true bedeutet:
        Es konnte keine zulässige perturbierte Lösung erzeugt werden.
        Deshalb wurde die beste bekannte Lösung als Fallback verwendet.
        */
        public bool UsedFallbackSolution { get; set; }
    }
}