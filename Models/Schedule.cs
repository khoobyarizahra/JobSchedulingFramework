using System.Collections.Generic;

namespace JobShopSchedulingFramework.Models
{
    /*
     SCHEDULE

     Represents one complete scheduling solution.

     A schedule contains:
     - all scheduled operations
     - objective value (Cmax)

     Later this class becomes very important for:
     - tabu search
     - neighborhood search
     - schedule comparison
     - copying solutions
    */
    public class Schedule
    {
        /*
         All scheduled operations
         belonging to this schedule.
        */
        public List<ScheduledOperation> scheduledOperations;

        /*
         Makespan of the schedule.

         Cmax = completion time
         of the last finished operation.
        */
        public int cmax;

        /*
         Constructor.
        */
        public Schedule()
        {
            scheduledOperations =
                new List<ScheduledOperation>();

            cmax = 0;
        }
    }
}