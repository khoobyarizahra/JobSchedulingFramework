using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods
{
    public class AdjacentSwapNeighborhood : INeighborhoodDefinition
    {
        public List<Move> GenerateMoves(
        Instance instance,
        Dictionary<int, List<Operation>> machineOrders,
        List<CriticalBlock> criticalBlocks)
        {
            List<Move> moves =
                new List<Move>();

            foreach (CriticalBlock block in criticalBlocks)
            {
                for (int i = 0;
                     i < block.operations.Count - 1;
                     i++)
                {
                    Operation first =
                        block.operations[i];

                    Operation second =
                        block.operations[i + 1];

                    moves.Add(
                        new Move(
                            block.machine,
                            block.startIndexInMachine + i,
                            block.startIndexInMachine + i + 1,
                            first.JobID,
                            first.OperationID,
                            second.JobID,
                            second.OperationID));
                }
            }

            return moves;
        }
    }
}
