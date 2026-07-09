using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods
{
    /*
    Erzeugt Moves durch Tauschen direkt benachbarter Operationen
    innerhalb kritischer Blöcke.

    Diese Nachbarschaft ist bewusst klein gehalten: Es werden nur Operationen
    betrachtet, die auf derselben Maschine direkt nebeneinander in einem
    kritischen Block liegen.
    */
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
                /*
                Innerhalb eines kritischen Blocks werden nur direkt benachbarte
                Operationen getauscht. Bei einem Block [A, B, C] entstehen also
                die Moves A<->B und B<->C.
                */
                for (int i = 0;
                     i < block.Operations.Count - 1;
                     i++)
                {
                    Operation first =
                        block.Operations[i];

                    Operation second =
                        block.Operations[i + 1];

                    /*
                    Die Indizes im Move beziehen sich auf die komplette
                    Maschinenreihenfolge, nicht nur auf die lokale Blockliste.
                    Deshalb wird der Startindex des Blocks addiert.
                    */
                    moves.Add(
                        new Move(
                            block.Machine,
                            block.StartIndexInMachine + i,
                            block.StartIndexInMachine + i + 1,
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