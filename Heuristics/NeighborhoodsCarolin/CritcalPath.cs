using JobShopSchedulingFramework.Heuristics.NeighborhoodsCarolin;
using JobShopSchedulingFramework.Models;
using JobShopSchedulingFramework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

//Klasse zum bestimmend es Kritischen Pfades

namespace JobShopSchedulingFramework.Heuristics.Neighborhoods
{
    public class CriticalPathFinder
    {
        //Gibt den Pfad als Liste von Operationen zurück
        public static List<Operation> GetCriticalPath(
            //Instanzen aufrufen
            Instance instance,
            //Aktuelle Maschinenreihenfolge
            NeighborhoodState state)
        {
   

           
            int cmax = ScheduleEvaluator.CalculateCmax(instance);

            Operation lastOp = instance.jobs
            .SelectMany(j => j.operations)
            .First(op => op.endTime == cmax);

            if (lastOp == null) // falls keine operation gefunden wurde
                return new List<Operation>(); // gibt leere liste zurück

            List<Operation> criticalPath = new List<Operation>(); // liste für kritischen pfad
            Operation current = lastOp; // startet rückwärts bei der letzten operation

            while (current != null) // solange eine vorgänger operation existiert
            {
                criticalPath.Add(current); // fügt aktuelle operation zum pfad hinzu

                current = FindPredecessor(current, instance.jobs, state); // sucht die vorherige operation
            }

            criticalPath.Reverse(); // dreht die liste um, damit sie von start bis ende geht
            return criticalPath; // gibt den kritischen pfad zurück
        }

        private static Operation FindPredecessor( // findet die vorgänger operation einer operation
            Operation op, // aktuelle operation
            List<Job> jobs, // alle jobs
            NeighborhoodState state) // maschinenreihenfolgen
        {
            var job = jobs.First(j => j.jobID == op.jobID); // findet den job zu dieser operation
            int indexInJob = job.operations.IndexOf(op); // position der operation im job

            if (indexInJob > 0) // wenn es eine vorherige operation im job gibt
            {
                var jobPred = job.operations[indexInJob - 1]; // nimmt die vorherige job operation

                if (jobPred.endTime <= op.startTime) // prüft zeitliche konsistenz
                    return jobPred; // gibt job vorgänger zurück
            }

            if (state.MachineSequences.TryGetValue(op.machine, out var seq)) // holt maschinensequenz der operation
            {
                int indexInMachine = seq.Operations.IndexOf(op); // position in maschinenreihenfolge

                if (indexInMachine > 0) // wenn es eine vorherige operation auf der maschine gibt
                {
                    var machinePred = seq.Operations[indexInMachine - 1]; // vorherige operation auf maschine

                    if (machinePred.endTime <= op.startTime) // prüft zeitliche konsistenz
                        return machinePred; // gibt maschinen vorgänger zurück
                }
            }

            return null; // wenn kein vorgänger existiert
        }
    }
}
