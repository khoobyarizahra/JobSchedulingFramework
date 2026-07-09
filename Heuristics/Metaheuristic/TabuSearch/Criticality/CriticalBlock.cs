using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality
{
    /*
    Beschreibt einen kritischen Block.

    Ein kritischer Block ist eine zusammenhängende Folge kritischer Operationen
    auf derselben Maschine. Innerhalb solcher Blöcke werden Nachbarschaftsmoves
    erzeugt, weil Änderungen dort den aktuellen Cmax beeinflussen können.

    Beispiel:
    Maschine 2: A - B - C - D - E
    kritisch:       B   C   D
    Block:          [B, C, D]

    StartIndexInMachine speichert, an welcher Position der Block in der vollständigen
    Maschinenreihenfolge beginnt. Das ist wichtig, weil Moves mit den Indizes der
    kompletten Maschinenliste arbeiten.

    Hinweis zum Refactoring:
    Die PascalCase-Properties sind die saubere neue Schreibweise.
    Die kleingeschriebenen Properties bleiben vorübergehend erhalten, damit
    bestehende Neighborhood-Klassen während des Refactorings weiter kompilieren.
    */
    public class CriticalBlock
    {
        // Maschine, auf der der kritische Block liegt.
        public int Machine { get; set; }

        // Startposition des Blocks in der vollständigen Maschinenreihenfolge.
        public int StartIndexInMachine { get; set; }

        // Kritische Operationen, die direkt nacheinander auf dieser Maschine liegen.
        public List<Operation> Operations { get; set; }

        public CriticalBlock(
            int machine,
            int startIndexInMachine)
        {
            Machine = machine;
            StartIndexInMachine = startIndexInMachine;
            Operations = new List<Operation>();
        }

        /*
        Kompatibilitäts-Properties für ältere Code-Stellen.

        Diese Properties können später entfernt werden, wenn alle Neighborhoods
        vollständig auf Machine, StartIndexInMachine und Operations umgestellt wurden.
        */
        public int machine
        {
            get { return Machine; }
            set { Machine = value; }
        }

        public int startIndexInMachine
        {
            get { return StartIndexInMachine; }
            set { StartIndexInMachine = value; }
        }

        public List<Operation> operations
        {
            get { return Operations; }
            set { Operations = value; }
        }
    }
}