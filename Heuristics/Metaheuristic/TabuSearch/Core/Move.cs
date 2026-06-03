namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    public class Move
    {
        //Diese Klasse repräsentiert einen Move, der in einem Tabu Search Algorithmus verwendet wird.
        //Ein Move beschreibt eine Änderung an der aktuellen Lösung, die durch den Austausch von Operationen auf einer Maschine erreicht wird.
        //Die Klasse enthält Informationen über die betroffene Maschine, die Indizes der Operationen auf dieser Maschine sowie die beteiligten Jobs und Operationen.
        //Sie bietet auch Methoden zur Generierung von Schlüsseln für die Identifikation von Moves und eine ToString-Methode für die lesbare Darstellung des Moves.
        public int Machine { get; set; }
        //MachineIndex1 und MachineIndex2 geben die Positionen der Operationen auf der betroffenen Maschine an, die ausgetauscht werden sollen.
        public int MachineIndex1 { get; set; }
        public int MachineIndex2 { get; set; }
        //FirstJob, FirstOperation, SecondJob und SecondOperation geben die spezifischen Jobs und Operationen an, die in diesem Move involviert sind.
        public int FirstJob { get; set; }
        public int FirstOperation { get; set; }

        public int SecondJob { get; set; }
        public int SecondOperation { get; set; }
        public bool IsInsertMove { get; set; }

        public Move(
            int machine,
            int machineIndex1,
            int machineIndex2,
            int job1,
            int operation1,
            int job2,
            int operation2)
        {
            this.Machine = machine;
            this.MachineIndex1 = machineIndex1;
            this.MachineIndex2 = machineIndex2;

            this.FirstJob = job1;
            this.FirstOperation = operation1;

            this.SecondJob = job2;
            this.SecondOperation = operation2;
            this.IsInsertMove = false;
        }
        //Die GetKey-Methode generiert einen eindeutigen Schlüssel für diesen Move, der auf der Maschine und den beteiligten Jobs und Operationen basiert.
        public string GetKey()
        {
            return
                "M" + Machine +
                "_J" + FirstJob + "O" + FirstOperation +
                "_J" + SecondJob + "O" + SecondOperation;
        }
        //Die GetReverseKey-Methode generiert einen Schlüssel für den umgekehrten Move, bei dem die Positionen der Jobs und Operationen vertauscht sind.
        //Dies ist nützlich, um zu überprüfen, ob ein Move bereits tabu ist, wenn er in umgekehrter Form auftritt.
        public string GetReverseKey()
        {
            return
                "M" + Machine +
                "_J" + SecondJob + "O" + SecondOperation +
                "_J" + FirstJob + "O" + FirstOperation;
        }

        public override string ToString()
        {
            return
                "Machine " + Machine +
                " | Swap J" + FirstJob + "O" + FirstOperation +
                " <-> J" + SecondJob + "O" + SecondOperation;
        }
    }
}