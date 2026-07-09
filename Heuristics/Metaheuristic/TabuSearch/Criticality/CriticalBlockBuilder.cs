using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality
{
    /*
    Bildet kritische Blöcke aus den zuvor bestimmten kritischen Operationen.

    Ein kritischer Block ist eine zusammenhängende Folge kritischer Operationen
    auf derselben Maschine. Diese Blöcke werden später genutzt, um gezielte
    Nachbarschaftsbewegungen in der Tabu Search zu erzeugen.
    */
    public static class CriticalBlockBuilder
    {
        public static List<CriticalBlock> BuildCriticalBlocks(
            Dictionary<int, List<Operation>> machineOrders,
            HashSet<Operation> criticalOperations)
        {
            List<CriticalBlock> criticalBlocks =
                new List<CriticalBlock>();

            // Jede Maschine wird separat betrachtet, da kritische Blöcke maschinenbezogen sind.
            foreach (var pair in machineOrders)
            {
                int machine = pair.Key;
                List<Operation> operationsOnMachine = pair.Value;

                bool insideBlock = false;

                CriticalBlock currentBlock =
                    new CriticalBlock(machine, -1);

                for (int index = 0;
                     index < operationsOnMachine.Count;
                     index++)
                {
                    Operation currentOperation =
                        operationsOnMachine[index];

                    bool isCurrentCritical =
                        criticalOperations.Contains(currentOperation);

                    /*
                    Ein neuer Block beginnt nur, wenn die aktuelle Operation
                    und ihre direkte Nachfolgeroperation auf derselben Maschine
                    kritisch sind. Dadurch enthält ein gespeicherter Block
                    mindestens zwei Operationen.
                    */
                    if (!insideBlock &&
                        isCurrentCritical)
                    {
                        bool isNextCritical = false;

                        if (index < operationsOnMachine.Count - 1)
                        {
                            Operation nextOperation =
                                operationsOnMachine[index + 1];

                            isNextCritical =
                                criticalOperations.Contains(nextOperation);
                        }

                        if (isNextCritical)
                        {
                            insideBlock = true;

                            currentBlock =
                                new CriticalBlock(
                                    machine,
                                    index);

                            currentBlock.operations.Add(
                                currentOperation);
                        }
                    }
                    /*
                    Solange weitere kritische Operationen direkt folgen,
                    wird der aktuelle Block erweitert.
                    */
                    else if (insideBlock &&
                             isCurrentCritical)
                    {
                        currentBlock.operations.Add(
                            currentOperation);
                    }
                    /*
                    Sobald eine nicht-kritische Operation erreicht wird,
                    endet der aktuelle kritische Block.
                    */
                    else if (insideBlock &&
                             !isCurrentCritical)
                    {
                        AddBlock(
                            criticalBlocks,
                            currentBlock);

                        insideBlock = false;
                    }
                }

                // Falls der letzte Block bis zum Maschinenende reicht, wird er hier abgeschlossen.
                if (insideBlock)
                {
                    AddBlock(
                        criticalBlocks,
                        currentBlock);
                }
            }

            return criticalBlocks;
        }

        // Speichert nur Blöcke, die mindestens zwei Operationen enthalten.
        private static void AddBlock(
            List<CriticalBlock> criticalBlocks,
            CriticalBlock block)
        {
            if (block.operations.Count >= 2)
            {
                criticalBlocks.Add(block);
            }
        }
    }
}