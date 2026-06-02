using System;
using System.Collections.Generic;
using System.Text;

//Eine Operation gehört zu genau einem Job und wird auf genau einer Maschine ausgeführt.
//Ein Job besteht aus einer festen Reihenfolge von Operationen.


namespace JobShopSchedulingFramework.Models
{
    public class Job
    {
        public int JobID { get; }
        public List<Operation> Operations { get; set; }

        public Job(int jobID)
        {
            this.JobID = jobID;
            this.Operations = new List<Operation>();
        }
    }

}
