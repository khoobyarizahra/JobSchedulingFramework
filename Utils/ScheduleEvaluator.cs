using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


/*
 Berechnet den Makespan (Cmax) des gesamten Schedules.

 Cmax = maximale Endzeit aller Operationen über alle Jobs hinweg.

 Je kleiner der Cmax, desto besser ist der Schedule,
 da alle Aufträge früher abgeschlossen werden.
*/

namespace JobShopSchedulingFramework.Utils
{
    public static class ScheduleEvaluator
    {
        public static int CalculateCmax(Instance instance)
        {
            return instance.Jobs
                .SelectMany(j => j.Operations)
                .Max(op => op.EndTime);
        }
    }
}
