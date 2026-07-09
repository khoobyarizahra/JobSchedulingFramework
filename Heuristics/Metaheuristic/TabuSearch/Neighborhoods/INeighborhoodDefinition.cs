using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods
{
    /*
    Gemeinsame Schnittstelle für alle Nachbarschaftsdefinitionen der Tabu Search.

    Jede Nachbarschaft erzeugt aus dem aktuellen Schedule und den kritischen
    Blöcken eine Liste möglicher Moves. Dadurch kann die Tabu Search mit
    unterschiedlichen Nachbarschaften arbeiten, ohne ihre eigene Logik zu ändern.
    */
    public interface INeighborhoodDefinition
    {
        List<Move> GenerateMoves(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders,
            List<CriticalBlock> criticalBlocks);
    }
}