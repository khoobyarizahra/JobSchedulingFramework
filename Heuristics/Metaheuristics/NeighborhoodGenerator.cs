using JobShopSchedulingFramework.Heuristics.Metaheuristics;
using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Tabu
{
    public static class NeighborhoodGenerator
    {
        //Die Methode GenerateAdjacentSwapMoves generiert alle möglichen Moves,
        //die durch das Vertauschen von benachbarten Operationen innerhalb eines kritischen Blocks entstehen.
        public static List<Move> GenerateAdjacentSwapMoves(
            List<CriticalBlock> criticalBlocks)
        {
            // Erstellen einer Liste, um die generierten Moves zu speichern.
            List<Move> moves = new List<Move>();
            // Iterieren über alle kritischen Blöcke, um die benachbarten Operationen zu identifizieren.
            foreach (CriticalBlock block in criticalBlocks)
            {
                for (int i = 0; i < block.operations.Count - 1; i++)
                {
                    Operation first = block.operations[i];
                    Operation second = block.operations[i + 1];

                    Move move = new Move(
                        block.machine,i, i+1,
                        first.jobID,
                        first.operationID,
                        second.jobID,
                        second.operationID);

                    moves.Add(move);
                }
            }

            return moves;
        }
    }
}