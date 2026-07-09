using System.Collections.Generic;

namespace JobShopSchedulingFramework.Models
{
    /*
    Enthält die vollständigen Eingabedaten eines Job-Shop-Scheduling-Problems.
    Die Instanz dient als gemeinsame Datenbasis für Heuristiken, Tabu Search und CP-SAT.
    */
    public class Instance
    {
        public int NumJobs { get; set; }
        public int NumMachines { get; set; }

        // Jobs mit ihren Operationen in der vorgegebenen Bearbeitungsreihenfolge.
        public List<Job> Jobs { get; set; } = new List<Job>();

        // Reihenfolgeabhängige Setup-Zeiten zwischen zwei direkt nacheinander bearbeiteten Jobs.
        public int[,] SetupTimes { get; set; } = new int[0, 0];
    }
}