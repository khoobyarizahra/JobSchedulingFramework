using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods
{
    /*
    Kombiniert mehrere Nachbarschaften für die finale Tabu Search.

    Verwendet werden All-Pair-Swaps innerhalb kritischer Blöcke und Insert-Moves
    für kritische Operationen. Dadurch kann die Suche sowohl lokale Vertauschungen
    als auch größere Positionsänderungen berücksichtigen.
    */
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

            /*
            Speichert eindeutige Move-Schlüssel, damit identische Moves aus
            verschiedenen Nachbarschaften nicht mehrfach bewertet werden.
            */
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

        /*
        Fügt nur Moves hinzu, die bisher noch nicht in der kombinierten
        Nachbarschaft enthalten sind.
        */
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

        /*
        Erzeugt einen eindeutigen Schlüssel für einen Move.

        Der Move-Typ wird bewusst mit aufgenommen, weil ein Swap und ein Insert
        mit ähnlichen Indizes unterschiedliche Bewegungen darstellen können.
        */
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