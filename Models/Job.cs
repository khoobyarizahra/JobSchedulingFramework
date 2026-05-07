using System;
using System.Collections.Generic;
using System.Text;

//Eine Operation gehört zu genau einem Job und wird auf genau einer Maschine ausgeführt.
//Ein Job besteht aus einer festen Reihenfolge von Operationen.


namespace Project_Scheduling_ZahraAndCarolin.Models
{
    public class Job
    {
        public int jobID;
        public List<Operation> operations;

        public Job(int jobID)
        {
            this.jobID = jobID;
            this.operations = new List<Operation>();
        }
    }

}
