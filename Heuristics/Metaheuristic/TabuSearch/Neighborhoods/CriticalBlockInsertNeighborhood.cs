using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods
{
    public class CriticalBlockInsertNeighborhood : INeighborhoodDefinition
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

                for (int fromIndex = 0; fromIndex < count; fromIndex++)
                {
                    for (int toIndex = 0; toIndex < count; toIndex++)
                    {
                        if (fromIndex == toIndex)
                        {
                            continue;
                        }

                        Operation movedOperation =
                            block.operations[fromIndex];

                        Operation targetOperation =
                            block.operations[toIndex];

                        Move move =
                            new Move(
                                block.machine,
                                block.startIndexInMachine + fromIndex,
                                block.startIndexInMachine + toIndex,
                                movedOperation.JobID,
                                movedOperation.OperationID,
                                targetOperation.JobID,
                                targetOperation.OperationID);
                        
                        move.IsInsertMove = true;

                        moves.Add(move);
                    }
                }
            }

            return moves;
        }
    }
}