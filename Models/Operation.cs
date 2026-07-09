namespace JobShopSchedulingFramework.Models
{
    /*
    Repräsentiert eine einzelne Operation innerhalb eines Jobs.
    Eine Operation besitzt eine feste Maschine und Bearbeitungszeit; Start- und Endzeit
    werden erst durch den jeweiligen Ablaufplan berechnet.
    */
    public class Operation
    {
        public int JobID { get; }
        public int OperationID { get; }
        public int Machine { get; }
        public int ProcessingTime { get; }

        // Zeitliche Lage der Operation im aktuell berechneten Ablaufplan.
        public int StartTime { get; set; }
        public int EndTime { get; set; }

        // Hilfswert für Prioritätsregeln wie LRPT oder SRPT.
        public int RemainingProcessingTime { get; set; }

        public Operation(
            int jobID,
            int operationID,
            int machine,
            int processingTime)
        {
            this.JobID = jobID;
            this.OperationID = operationID;
            this.Machine = machine;
            this.ProcessingTime = processingTime;
        }
    }
}