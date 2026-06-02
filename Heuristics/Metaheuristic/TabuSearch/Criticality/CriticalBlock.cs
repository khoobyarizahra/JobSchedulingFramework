using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality
{
    /*
     CRITICAL BLOCK

     Ein kritischer Block ist eine zusammenhängende Sequenz
     von kritischen Operationen auf derselben Maschine.

     Beispiel:
     Maschine 2: A - B - C - D - E
     Kritisch:       B   C   D
     Block:          [B, C, D]

     startIndexInMachine speichert, an welcher Position der Block
     in der kompletten Maschinenreihenfolge beginnt.

     Das ist wichtig, weil ein Move später mit den Indizes der
     kompletten Maschinenliste arbeitet.
    */
    public class CriticalBlock
    {
        // Maschine, auf der der kritische Block liegt
        public int machine;

        // Startposition des Blocks in der kompletten Maschinenreihenfolge
        public int startIndexInMachine;

        // Kritische Operationen, die direkt nacheinander auf dieser Maschine liegen
        public List<Operation> operations;

        public CriticalBlock(
            int machine,
            int startIndexInMachine)
        {
            this.machine = machine;
            this.startIndexInMachine = startIndexInMachine;
            this.operations = new List<Operation>();
        }
    }
}