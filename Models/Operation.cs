using System;
using System.Collections.Generic;
using System.Text;

namespace JobShopSchedulingFramework.Models
{
    public class Operation
    {
        public int JobID { get; }
        public int OperationID {get; }
        public int Machine { get; }
        public int ProcessingTime { get; }

        // Ergebnisse des Schedulings
        public int StartTime { get; set; }
        public int EndTime { get; set; }

        // Für LRPT / SRPT
        public int remainingProcessingTime { get; set; }

        public Operation(int jobID, int operationID, int machine, int processingTime)
        {
            this.JobID = jobID;
            this.OperationID = operationID;
            this.Machine = machine;
            this.ProcessingTime = processingTime;
        }
    }
}
