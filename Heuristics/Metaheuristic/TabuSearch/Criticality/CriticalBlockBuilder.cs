using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality
{
    /*
     CRITICAL BLOCK BUILDER

     Diese Klasse bildet kritische Blöcke direkt aus den
     kritischen Operationen.

     Idee:
     - Jede Maschine wird separat betrachtet.
     - Ein Block beginnt nur dann, wenn die aktuelle Operation
       und die nächste Operation kritisch sind.
     - Solange weitere kritische Operationen folgen,
       wird der Block erweitert.
     - Da ein Block nur bei zwei kritischen Nachbarn beginnt,
       enthält jeder gespeicherte Block automatisch mindestens
       zwei Operationen.
    */
    public static class CriticalBlockBuilder
    {
        //als Eingabe erhält die Methode die Maschinenreihenfolge und die Menge der kritischen Operationen
        public static List<CriticalBlock> BuildCriticalBlocks(
            Dictionary<int, List<Operation>> machineOrders,
            HashSet<Operation> criticalOperations)
        {
            //Liste für die kritischen Blöcke, die erstellt werden
            List<CriticalBlock> criticalBlocks =
                new List<CriticalBlock>();
            //wir iterieren über jede Maschine und ihre Operationen
            foreach (var pair in machineOrders)
            {
                int machine = pair.Key;
                List<Operation> operationsOnMachine = pair.Value;
                //Flag, um zu verfolgen, ob wir uns gerade in einem kritischen Block befinden
                bool insideBlock = false;
                //aktueller Block, der aufgebaut wird, initialisiert mit ungültigem Index
                CriticalBlock currentBlock =
                    new CriticalBlock(machine, -1);
                //Iterieren über die Operationen der aktuellen Maschine
                for (int index = 0;
                     index < operationsOnMachine.Count;
                     index++)
                {
                    //aktuelle Operation auf der Maschine
                    Operation currentOperation =
                        operationsOnMachine[index];
                    //prüfen, ob die aktuelle Operation kritisch ist
                    bool isCurrentCritical =
                        criticalOperations.Contains(currentOperation);
                    //Logik zum Starten, Fortsetzen oder Beenden eines kritischen Blocks
                    // Ein Block beginnt nur, wenn die aktuelle und die nächste Operation kritisch sind, 
                    //in diesem Fall setzen wir insideBlock auf true und fügen die aktuelle Operation zum Block hinzu
                    //Ein Block wird fortgesetzt, wenn die aktuelle Operation kritisch ist und wir uns bereits in einem Block befinden,
                    //Ein Block wird beendet, wenn die aktuelle Operation nicht kritisch ist, aber wir uns in einem Block befinden,
                    //in diesem Fall fügen wir den Block zur Liste der kritischen Blöcke hinzu und setzen insideBlock auf false
                    //false wir das Ende der Machinenreihenfolge erreichen,
                    //aber wir befinden uns immer noch in einem Block, fügen wir den Block zur Liste der kritischen Blöcke hinzu
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
                        //Erst wenn die aktuelle und die nächste Operation kritisch sind, beginnt ein Block
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
                    //Wenn wir uns bereits in einem Block befinden und die aktuelle Operation kritisch ist, fügen wir sie zum aktuellen Block hinzu
                    else if (insideBlock &&
                             isCurrentCritical)
                    {
                        currentBlock.operations.Add(
                            currentOperation);
                    }
                    //Wenn wir uns in einem Block befinden, aber die aktuelle Operation nicht kritisch ist,
                    //fügen wir den Block zur Liste der kritischen Blöcke hinzu und setzen insideBlock auf false
                    else if (insideBlock &&
                             !isCurrentCritical)
                    {
                        AddBlock(
                            criticalBlocks,
                            currentBlock);

                        insideBlock = false;
                    }
                }
                //Wenn wir am Ende der Machinenreihenfolge angekommen sind,
                //aber immer noch in einem Block sind, fügen wir diesen Block zur Liste der kritischen Blöcke hinzu
                if (insideBlock)
                {
                    AddBlock(
                        criticalBlocks,
                        currentBlock);
                }
            }

            return criticalBlocks;
        }

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