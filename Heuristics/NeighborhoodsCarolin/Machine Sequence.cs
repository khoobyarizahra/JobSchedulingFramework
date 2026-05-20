using System;
using System.Collections.Generic;
using System.Text;
using JobShopSchedulingFramework.Models;

// Klasse zum Erstellen von Sequenzen
//Funktionen: Hinzufügen von Operationen
//            Vertauschen von Operationen

//Welche Operationen werden auf einer bestimmten Maschine in welcher Reihenfolge ausgeführt?
// Lokale Suche braucht die Reihenfolge der Operationen, nicht die Zeiten


namespace JobShopSchedulingFramework.Heuristics.NeighborhoodsCarolin
{
        public class MachineSequence
        {
            // IDs der Maschinen einlesen
            public int MachineId { get; set; }

        // speichert die Reihenfolge, in der Operationen auf der Maschine ausgeführt werden.
        public List<Operation> Operations { get; set; }

            public MachineSequence(int machineId)
            {
                //Maschine ID speichern
                MachineId = machineId;
                //Erstellt eine leere Liste für Operationen.
                Operations = new List<Operation>();
            }

            // Methode zum Hinzufügen einer Operation.
            // Fügt die Operation hinten an die Liste an
            public void AddOperation(Operation operation)
            {
                Operations.Add(operation);
            }

            // Methode zum Vertauschen zweier Operationen.
            public void Swap(int indexA, int indexB)
            {
                // Speichert die erste Operation temporär, damit sie nicht überschrieben wird.
                Operation temp = Operations[indexA];

                // Überschreibt Position A mit der Operation von Position B.
                Operations[indexA] = Operations[indexB];

                // Schreibt die temporär gespeicherte Operation auf Position B.
                Operations[indexB] = temp;
            }
        }
    
}
