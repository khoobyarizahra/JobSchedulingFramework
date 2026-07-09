using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods
{
    /*
    Erzeugt eine eingeschränkte Swap-Nachbarschaft für kritische Blöcke.

    Pro kritischem Block werden nur die Randbereiche betrachtet:
    die ersten beiden Operationen und, falls vorhanden, die letzten beiden
    Operationen. Dadurch bleibt die Nachbarschaft sehr klein und schnell.
    */
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
                if (block.Operations.Count < 2)
                {
                    continue;
                }

                /*
                Erster Rand-Move:
                Die ersten beiden Operationen des kritischen Blocks werden getauscht.
                */
                Operation first =
                    block.Operations[0];

                Operation second =
                    block.Operations[1];

                moves.Add(
                    new Move(
                        block.Machine,
                        block.StartIndexInMachine,
                        block.StartIndexInMachine + 1,
                        first.JobID,
                        first.OperationID,
                        second.JobID,
                        second.OperationID));

                /*
                Zweiter Rand-Move:
                Wenn der Block mehr als zwei Operationen enthält, werden zusätzlich
                die letzten beiden Operationen des Blocks getauscht.

                Bei genau zwei Operationen wäre dieser Move identisch mit dem ersten
                und wird deshalb nicht doppelt erzeugt.
                */
                if (block.Operations.Count > 2)
                {
                    int lastIndex =
                        block.Operations.Count - 1;

                    Operation beforeLast =
                        block.Operations[lastIndex - 1];

                    Operation last =
                        block.Operations[lastIndex];

                    moves.Add(
                        new Move(
                            block.Machine,
                            block.StartIndexInMachine + lastIndex - 1,
                            block.StartIndexInMachine + lastIndex,
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