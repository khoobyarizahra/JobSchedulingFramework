using System;
using System.Collections.Generic;
using System.Text;


/*
  INSTANCE Repräsentiert das gesamte Scheduling-Problem:
   - Anzahl Jobs
   - Anzahl Maschinen
   - alle Jobs
   - Setup-Zeiten zwischen Jobs
  */


namespace JobShopSchedulingFramework.Models
{
    /*
    Repräsentiert das gesamte Scheduling-Problem:
    - Anzahl Jobs
    - Anzahl Maschinen
    - alle Jobs
    - Setup-Zeiten zwischen Jobs
  */

    public class Instance
    {
        public int NumJobs { get; set; }
        public int NumMachines { get; set; }
        public List<Job> Jobs {  get; set; } = new List<Job>();
        public int[,] SetupTimes { get; set; } = new int[0, 0];

    }

}
