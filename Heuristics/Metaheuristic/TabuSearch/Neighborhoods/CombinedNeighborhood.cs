using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods
{
    /// <summary>
    /// Combined neighborhood for the final Tabu Search.
    /// 
    /// This neighborhood combines two move types:
    /// 1. All-pair swaps inside critical blocks
    /// 2. Insert moves for critical operations
    /// 
    /// The goal is to allow both direct reordering by swaps
    /// and larger positional changes by insert moves.
    /// </summary>
    public class CombinedNeighborhood : INeighborhoodDefinition
    {
        private readonly AllPairSwapNeighborhood allPairSwapNeighborhood;
        private readonly CriticalBlockInsertNeighborhood criticalBlockInsertNeighborhood;

        public CombinedNeighborhood()
        {
            allPairSwapNeighborhood =
                new AllPairSwapNeighborhood();

            criticalBlockInsertNeighborhood =
                new CriticalBlockInsertNeighborhood();
        }

        public List<Move> GenerateMoves(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders,
            List<CriticalBlock> criticalBlocks)
        {
            List<Move> combinedMoves =
                new List<Move>();

            HashSet<string> generatedMoveKeys =
                new HashSet<string>();

            List<Move> swapMoves =
                allPairSwapNeighborhood.GenerateMoves(
                    instance,
                    machineOrders,
                    criticalBlocks);

            AddUniqueMoves(
                combinedMoves,
                generatedMoveKeys,
                swapMoves);

            List<Move> insertMoves =
                criticalBlockInsertNeighborhood.GenerateMoves(
                    instance,
                    machineOrders,
                    criticalBlocks);

            AddUniqueMoves(
                combinedMoves,
                generatedMoveKeys,
                insertMoves);

            return combinedMoves;
        }

        private static void AddUniqueMoves(
            List<Move> combinedMoves,
            HashSet<string> generatedMoveKeys,
            List<Move> newMoves)
        {
            foreach (Move move in newMoves)
            {
                string moveKey =
                    CreateMoveKey(move);

                if (!generatedMoveKeys.Add(moveKey))
                {
                    continue;
                }

                combinedMoves.Add(move);
            }
        }

        private static string CreateMoveKey(
            Move move)
        {
            string moveType =
                move.IsInsertMove
                    ? "Insert"
                    : "Swap";

            return
                moveType +
                "|M" + move.Machine +
                "|I1:" + move.MachineIndex1 +
                "|I2:" + move.MachineIndex2 +
                "|J1:" + move.FirstJob +
                "|O1:" + move.FirstOperation +
                "|J2:" + move.SecondJob +
                "|O2:" + move.SecondOperation;
        }
    }
}