using System.Collections.Generic;

namespace JobShopSchedulingFramework.Models
{
    /*
    Repräsentiert einen Job mit seiner festen Operationsreihenfolge.
    Diese Reihenfolge bildet die Vorrangbedingungen innerhalb des Jobs ab.
    */
    public class Job
    {
        public int JobID { get; }

        // Operationen des Jobs in der vorgegebenen Bearbeitungsreihenfolge.
        public List<Operation> Operations { get; set; }

        public Job(int jobID)
        {
            this.JobID = jobID;
            this.Operations = new List<Operation>();
        }
    }
}