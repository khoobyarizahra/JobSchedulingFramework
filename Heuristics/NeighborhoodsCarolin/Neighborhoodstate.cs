using JobShopSchedulingFramework.Heuristics.NeighborhoodsCarolin;
using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

//Speichert alle erstellten Sequenzen von Neighborhoods/Maschine Sequence
// Übersicht über alle Operationen auf allen Maschinen

namespace JobShopSchedulingFramework.Heuristics.Neighborhoods
{
    public class NeighborhoodState
    {

       // Speichert Maschienen Sequenzen
        public Dictionary<int, MachineSequence> MachineSequences
        {
            get;
            set;
        }


        public NeighborhoodState()
        {

            // Erstellt ein leeres Dictionary
            // Am Anfang existieren noch keine Maschinen
            MachineSequences =
                new Dictionary<int, MachineSequence>();
        }


        // Fügt Operation der richtigen Maschine hinzu
        public void AddOperation(Operation operation)
        {

            
            // Existiert für diese Maschine schon eine Sequenz?
            if (!MachineSequences.ContainsKey(
                operation.machine))
            {

                // Falls nicht: Neue Sequenz erzeugen
                MachineSequences[operation.machine] =
                    new MachineSequence(
                        operation.machine);
            }


            // Operation an die passende Maschine anhängen
            MachineSequences[operation.machine]
                .AddOperation(operation);
        }

    }
}