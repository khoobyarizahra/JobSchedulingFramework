using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods
{
    /*
    Erzeugt Swap-Moves für alle Operationspaare innerhalb kritischer Blöcke.

    Im Gegensatz zu AdjacentSwapNeighborhood werden nicht nur direkte Nachbarn
    betrachtet, sondern jedes mögliche Paar innerhalb eines kritischen Blocks.
    Dadurch entsteht eine größere, aber auch teurere Nachbarschaft.
    */
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
                    block.Operations.Count;

                /*
                Für einen Block mit n Operationen werden alle Paare (i, j)
                mit i < j erzeugt. Dadurch entstehen n * (n - 1) / 2 Moves.
                */
                for (int i = 0;
                     i < count - 1;
                     i++)
                {
                    for (int j = i + 1;
                         j < count;
                         j++)
                    {
                        Operation first =
                            block.Operations[i];

                        Operation second =
                            block.Operations[j];

                        /*
                        Die lokalen Blockindizes werden auf Indizes der
                        kompletten Maschinenreihenfolge abgebildet.
                        */
                        moves.Add(
                            new Move(
                                block.Machine,
                                block.StartIndexInMachine + i,
                                block.StartIndexInMachine + j,
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