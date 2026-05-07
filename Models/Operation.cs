using System;
using System.Collections.Generic;
using System.Text;

namespace Project_Scheduling_ZahraAndCarolin.Models
{
    public class Operation
    {
        public int jobID;
        public int operationID;
        public int machine;
        public int processingTime;

        // Ergebnisse des Schedulings
        public int startTime;
        public int endTime;

        // Für LRPT / SRPT
        public int remainingProcessingTime;

        public Operation(int jobID, int operationID, int machine, int processingTime)
        {
            this.jobID = jobID;
            this.operationID = operationID;
            this.machine = machine;
            this.processingTime = processingTime;
        }
    }
}
