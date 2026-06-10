using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods
{

 
    public class RestrictedBlockSwapNeighborhood : INeighborhoodDefinition
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
                if (block.operations.Count < 2)
                {
                    continue;
                }

                Operation first =
                    block.operations[0];

                Operation second =
                    block.operations[1];

                moves.Add(
                    new Move(
                        block.machine,
                        block.startIndexInMachine,
                        block.startIndexInMachine + 1,
                        first.JobID,
                        first.OperationID,
                        second.JobID,
                        second.OperationID));

                if (block.operations.Count > 2)
                {
                    int lastIndex =
                        block.operations.Count - 1;

                    Operation beforeLast =
                        block.operations[lastIndex - 1];

                    Operation last =
                        block.operations[lastIndex];

                    moves.Add(
                        new Move(
                            block.machine,
                            block.startIndexInMachine + lastIndex - 1,
                            block.startIndexInMachine + lastIndex,
                            beforeLast.JobID,
                            beforeLast.OperationID,
                            last.JobID,
                            last.OperationID));
                }
            }

            return moves;
        }
    }
}