using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods
{
    public interface INeighborhoodDefinition
    {
        List<Move> GenerateMoves(
         Instance instance,
        Dictionary<int, List<Operation>> machineOrders,
        List<CriticalBlock> criticalBlocks);
    }
}