using System;
using System.Collections.Generic;
using System.Text;
using JobShopSchedulingFramework.Models;
using System.Collections.Generic;


namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality
{
    /*
     CRITICAL OPERATION ANALYSIS RESULT

     Diese Klasse bündelt die Ergebnisse der kritischen Analyse.

     Sie enthält:
     - r_i Werte
     - q_i Werte
     - kritische Operationen
     - Cmax

     Dadurch muss nicht jede Methode mehrere einzelne Werte zurückgeben.
    */
    public class CriticalOperationAnalysisResult
    {
        public Dictionary<Operation, int> releaseTimes;
        public Dictionary<Operation, int> tails;
        public HashSet<Operation> criticalOperations;
        public int cmax;

        public CriticalOperationAnalysisResult()
        {
            releaseTimes = new Dictionary<Operation, int>();
            tails = new Dictionary<Operation, int>();
            criticalOperations = new HashSet<Operation>();
            cmax = 0;
        }
    }
}
