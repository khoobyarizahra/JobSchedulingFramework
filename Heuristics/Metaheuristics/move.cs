/*
 MOVE

 Diese Klasse beschreibt eine Nachbarschaftsbewegung (Move)
 innerhalb der Tabu Search.

 Die Grundidee:
 Zwei Operationen auf derselben Maschine sollen
 in ihrer Reihenfolge vertauscht werden.

 Beispiel:

 Vorher:
 M2: J1O2 -> J3O1

 Nachher:
 M2: J3O1 -> J1O2

 Genau diese Veränderung wird durch ein Move-Objekt beschrieben.

 WICHTIG:
 Diese Klasse beschreibt NUR die Veränderung.
 Sie führt den Tausch selbst NICHT aus.
 Das macht später die Methode ApplyMove().
*/

namespace JobShopSchedulingFramework.Models
{
    public class Move
    {
        /*
         Die Maschine, auf der der Move stattfindet.

         Beispiel:
         machine = 2

         Bedeutet:
         Der Tausch passiert auf Maschine M2.
        */
        public int machine;

        /*
         Position der ersten Operation innerhalb
         der Maschinenreihenfolge. Das bedeutet, an welcher Stelle die erste Operation in der Liste der Operationen auf dieser Maschine steht. 
        das ist in Klasse ScheduleOrderHelper wichtig, um die Operationen zu identifizieren, die vertauscht werden sollen.

         Beispiel:
         M2: [A, B, C, D]

         firstIndex = 1
         -> entspricht Operation B
        */
        public int firstIndex;

        /*
         Position der zweiten Operation innerhalb der Maschinenreihenfolge.

         Beispiel:
         M2: [A, B, C, D]

         secondIndex = 2
         -> entspricht Operation C
        */
        public int secondIndex;

        /*
         Job-ID der ersten Operation.

         Diese Information ist für die eigentliche
         Vertauschung nicht zwingend notwendig,
       

         Beispiel:
         J1O2
         -> firstJobID = 1
        */
        public int firstJobID;

        /*
         Operations-ID der ersten Operation.

         Beispiel:
         J1O2
         -> firstOperationID = 2
        */
        public int firstOperationID;

        /*
         Job-ID der zweiten Operation.

         Beispiel:
         J3O1
         -> secondJobID = 3
        */
        public int secondJobID;

        /*
         Operations-ID der zweiten Operation.

         Beispiel:
         J3O1
         -> secondOperationID = 1
        */
        public int secondOperationID;

        /*
         Konstruktor zum Erzeugen eines Moves.

         Hier werden alle wichtigen Informationen
         gespeichert, die den Tausch beschreiben.
        */
        public Move(
            int machine,
            //firstIndex und secondIndex sind wichtig, um die Positionen der Operationen auf der Maschine zu identifizieren,
            //die vertauscht werden sollen.firstIndex bekommen wir aus der ScheduleOrderHelper-Klasse,
            //wenn wir die Operationen auf der Maschine durchgehen. Die sind Indexen der Liste namens "machineOperations"
            //in der ScheduleOrderHelper-Klasse.
            int firstIndex,
            int secondIndex,

            int firstJobID,
            int firstOperationID,

            int secondJobID,
            int secondOperationID)
        {
            /*
             Speichert die Maschine,
             auf der der Move durchgeführt wird.
            */
            this.machine = machine;

            /*
             Speichert die Position der ersten Operation
             innerhalb der Maschinenliste.
            */
            this.firstIndex = firstIndex;

            /*
             Speichert die Position der zweiten Operation
             innerhalb der Maschinenliste.
            */
            this.secondIndex = secondIndex;

            /*
             Speichert die Job-ID
             der ersten Operation.
            */
            this.firstJobID = firstJobID;

            /*
             Speichert die Operations-ID
             der ersten Operation.
            */
            this.firstOperationID = firstOperationID;

            /*
             Speichert die Job-ID
             der zweiten Operation.
            */
            this.secondJobID = secondJobID;

            /*
             Speichert die Operations-ID
             der zweiten Operation.
            */
            this.secondOperationID = secondOperationID;
        }

        /*
         Diese Methode erzeugt eine lesbare Darstellung
         des Moves als String.


         Beispielausgabe:

         M2: J1O2 <-> J3O1
        */
        public override string ToString()
        {
            return
                $"M{machine}: " +
                $"J{firstJobID}O{firstOperationID} " +
                $"<-> " +
                $"J{secondJobID}O{secondOperationID}";
        }
    }
}