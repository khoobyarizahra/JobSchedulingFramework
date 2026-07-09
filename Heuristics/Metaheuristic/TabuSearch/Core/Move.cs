namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /*
    Repräsentiert einen Move innerhalb der Tabu Search.

    Ein Move beschreibt eine Änderung der Reihenfolge auf einer Maschine.
    Standardmäßig handelt es sich um einen Swap-Move zwischen zwei Operationen.
    Über IsInsertMove kann derselbe Datentyp auch für Insert-Moves verwendet werden.
    */
    public class Move
    {
        // Maschine, auf der der Move ausgeführt wird.
        public int Machine { get; set; }

        // Positionen der betroffenen Operationen in der Maschinenreihenfolge.
        public int MachineIndex1 { get; set; }
        public int MachineIndex2 { get; set; }

        // Erste beteiligte Operation.
        public int FirstJob { get; set; }
        public int FirstOperation { get; set; }

        // Zweite beteiligte Operation beziehungsweise Zieloperation bei Insert-Moves.
        public int SecondJob { get; set; }
        public int SecondOperation { get; set; }

        // Kennzeichnet, ob dieser Move als Insert statt als Swap interpretiert wird.
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

        /*
        Erzeugt einen Schlüssel für diesen Move.

        Der Schlüssel wird unter anderem für die Tabu-Liste und zur Erkennung
        doppelter Moves verwendet.
        */
        public string GetKey()
        {
            return
                "M" + Machine +
                "_J" + FirstJob + "O" + FirstOperation +
                "_J" + SecondJob + "O" + SecondOperation;
        }

        /*
        Erzeugt den Schlüssel für die umgekehrte Swap-Richtung.

        Dadurch kann ein Swap zwischen A und B auch dann erkannt werden,
        wenn er als B und A erzeugt wurde.
        */
        public string GetReverseKey()
        {
            return
                "M" + Machine +
                "_J" + SecondJob + "O" + SecondOperation +
                "_J" + FirstJob + "O" + FirstOperation;
        }

        public override string ToString()
        {
            if (IsInsertMove)
            {
                return
                    "Machine " + Machine +
                    " | Insert J" + FirstJob + "O" + FirstOperation +
                    " from " + MachineIndex1 +
                    " to " + MachineIndex2;
            }

            return
                "Machine " + Machine +
                " | Swap J" + FirstJob + "O" + FirstOperation +
                " <-> J" + SecondJob + "O" + SecondOperation;
        }
    }
}