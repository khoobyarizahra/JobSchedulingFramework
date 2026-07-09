using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods
{
    /*
    Erzeugt Insert-Moves innerhalb kritischer Blöcke.

    Bei einem Insert-Move wird eine Operation aus ihrer aktuellen Position
    entfernt und an einer anderen Position im selben kritischen Block eingefügt.
    Dadurch können stärkere Positionsänderungen erzeugt werden als bei
    einfachen Swap-Moves.
    */
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
                    block.Operations.Count;

                /*
                Jede Operation im kritischen Block kann als zu verschiebende
                Operation gewählt werden.
                */
                for (int fromIndex = 0; fromIndex < count; fromIndex++)
                {
                    /*
                    Die gewählte Operation kann an jede andere Position im
                    selben Block eingefügt werden.
                    */
                    for (int toIndex = 0; toIndex < count; toIndex++)
                    {
                        if (fromIndex == toIndex)
                        {
                            continue;
                        }

                        Operation movedOperation =
                            block.Operations[fromIndex];

                        Operation targetOperation =
                            block.Operations[toIndex];

                        /*
                        Die Move-Indizes beziehen sich auf die komplette
                        Maschinenreihenfolge. Deshalb wird der lokale Index
                        im Block mit dem Startindex des Blocks verrechnet.
                        */
                        Move move =
                            new Move(
                                block.Machine,
                                block.StartIndexInMachine + fromIndex,
                                block.StartIndexInMachine + toIndex,
                                movedOperation.JobID,
                                movedOperation.OperationID,
                                targetOperation.JobID,
                                targetOperation.OperationID);

                        // Kennzeichnet diesen Move eindeutig als Insert-Move.
                        move.IsInsertMove = true;

                        moves.Add(move);
                    }
                }
            }

            return moves;
        }
    }
}