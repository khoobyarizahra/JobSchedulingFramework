using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods
{
    public class AllPairSwapNeighborhood : INeighborhoodDefinition
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
                int count =
                    block.operations.Count;

                for (int i = 0;
                     i < count - 1;
                     i++)
                {
                    for (int j = i + 1;
                         j < count;
                         j++)
                    {
                        Operation first =
                            block.operations[i];

                        Operation second =
                            block.operations[j];

                        moves.Add(
                            new Move(
                                block.machine,
                                block.startIndexInMachine + i,
                                block.startIndexInMachine + j,
                                first.JobID,
                                first.OperationID,
                                second.JobID,
                                second.OperationID));
                    }
                }
            }

            return moves;
        }
    }
}