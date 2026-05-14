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
   
    public class Instance
    {
        public int numJobs;
        public int numMachines;
        public List<Job> jobs;
        public int[,] setupTimes;

        public Instance()
        {
            this.jobs = new List<Job>();
        }
    }

}
