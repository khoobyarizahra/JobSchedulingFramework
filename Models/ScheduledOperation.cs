using System;

namespace JobShopSchedulingFramework.Models
{
    /*
     SCHEDULED OPERATION

     Represents one operation inside a concrete schedule.

     Important distinction:

     Operation:
     = static problem definition

     ScheduledOperation:
     = operation with assigned timing information
    */
    public class ScheduledOperation
    {
        /*
         Reference to the original operation.

         This keeps access to:
         - jobID
         - operationID
         - machine
         - processingTime
        */
        public Operation operation;

        /*
         Start time of the operation in the schedule.
        */
        public int startTime;

        /*
         End time of the operation in the schedule.
        */
        public int endTime;

        /*
         Constructor.
        */
        public ScheduledOperation(
            Operation operation,
            int startTime,
            int endTime)
        {
            this.operation = operation;

            this.startTime = startTime;

            this.endTime = endTime;
        }
    }
}