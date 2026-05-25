/*
 MOVE

 Diese Klasse beschreibt eine Nachbarschaftsbewegung (Move)
 innerhalb der Tabu Search.

 Die Grundidee:
 Zwei Operationen auf derselben Maschine sollen
 in ihrer Reihenfolge vertauscht werden.

 WICHTIG:
 Diese Klasse beschreibt NUR die Veränderung.
 Sie führt den Tausch selbst NICHT aus.
 Das macht später die Methode ApplyMove().
*/

namespace JobShopSchedulingFramework.Models
{
    public class Move
    {
        public int machine;

        public int firstIndex;
        public int secondIndex;

        public int firstJobID;
        public int firstOperationID;

        public int secondJobID;
        public int secondOperationID;

        public Move(
            int machine,
            int firstIndex,
            int secondIndex,
            int firstJobID,
            int firstOperationID,
            int secondJobID,
            int secondOperationID)
        {
            this.machine = machine;

            this.firstIndex = firstIndex;
            this.secondIndex = secondIndex;

            this.firstJobID = firstJobID;
            this.firstOperationID = firstOperationID;

            this.secondJobID = secondJobID;
            this.secondOperationID = secondOperationID;
        }

        /*
         Schlüssel für die Tabu-Liste.

         WICHTIG:
         Wir benutzen hier nicht nur die Indizes,
         sondern die Identität der Operationen.

         Grund:
         Indizes können sich nach einem Move ändern.
         JobID und OperationID bleiben stabiler.
        */
        public string GetKey()
        {
            return
                $"M{machine}_" +
                $"J{firstJobID}O{firstOperationID}_" +
                $"J{secondJobID}O{secondOperationID}";
        }

        /*
         Schlüssel für den umgekehrten Move.

         Wenn wir A und B tauschen,
         soll die Tabu Search nicht sofort wieder
         B und A zurücktauschen.
        */
        public string GetReverseKey()
        {
            return
                $"M{machine}_" +
                $"J{secondJobID}O{secondOperationID}_" +
                $"J{firstJobID}O{firstOperationID}";
        }

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