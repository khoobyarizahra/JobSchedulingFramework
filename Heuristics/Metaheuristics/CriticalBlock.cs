using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristics
{
    /*
     CRITICAL BLOCK

     Diese Klasse beschreibt einen kritischen Block
     innerhalb eines kritischen Pfades.

     Ein kritischer Block besteht aus:
     - kritischen Operationen
     - auf derselben Maschine
     - die direkt hintereinander ausgeführt werden

     Kritische Blöcke sind wichtig für die Tabu Search,
     weil aus ihnen die möglichen Nachbarschafts-Moves
     erzeugt werden.
    */
    public class CriticalBlock
    {
        // Maschine, auf der sich der kritische Block befindet
        public int machine;

        // Liste aller Operationen,
        // die zu diesem kritischen Block gehören
        public List<Operation> operations;

        /*
         Konstruktor

         Erstellt einen neuen kritischen Block
         für eine bestimmte Maschine.

         Die Liste der Operationen ist am Anfang leer
         und wird später gefüllt.
        */
        public CriticalBlock(int machine)
        {
            // Speichert die Maschine des Blocks
            this.machine = machine;

            // Erstellt eine leere Liste für die Operationen
            this.operations = new List<Operation>();
        }
    }
}