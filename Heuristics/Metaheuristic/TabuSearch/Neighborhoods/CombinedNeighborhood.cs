using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Models;
using System.Collections.Generic;
using System.Linq;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods
{
    // Combines multiple neighborhood definitions into a single neighborhood.
    // The purpose is to generate candidate moves from all implemented
    // neighborhood structures (e.g. N1, N2, N3, N4) during one Tabu Search run.
    // All generated moves are collected in a common move list and returned
    // to the Tabu Search algorithm for evaluation.
    // This allows the search to select the best admissible move from a
    // larger and more diverse set of candidate solutions.
    public class CombinedNeighborhood : INeighborhoodDefinition
    {
        private readonly List<INeighborhoodDefinition> neighborhoods;

        public CombinedNeighborhood()
        {
            neighborhoods =
                new List<INeighborhoodDefinition>
                {
                    new AdjacentSwapNeighborhood(),
                    new RestrictedBlockSwapNeighborhood(),
                    new AllPairSwapNeighborhood(),
                    new CriticalBlockInsertNeighborhood()
                };
        }

        public List<Move> GenerateMoves(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders,
            List<CriticalBlock> criticalBlocks)
        {
            List<Move> combinedMoves =
                new List<Move>();

            foreach (INeighborhoodDefinition neighborhood in neighborhoods)
            {
                List<Move> moves =
                    neighborhood.GenerateMoves(
                        instance,
                        machineOrders,
                        criticalBlocks);

                combinedMoves.AddRange(moves);
            }

            return combinedMoves;
        }
    }
}