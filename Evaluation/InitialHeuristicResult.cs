using JobShopSchedulingFramework.Heuristics.Initial;
using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Evaluation
{
    /*
     INITIAL HEURISTIC RESULT

     Stores the best result found by comparing all priority rules.

     This object is useful later because the metaheuristic
     can start from the best initial solution.
    */
    public class InitialHeuristicResult
    {
        // Best scheduled instance found by the initial heuristic comparison.
        public Instance bestInstance;

        // Priority rule that produced the best Cmax.
        public PriorityRule bestRule;

        // Best makespan value.
        public int bestCmax;

        /*
         Constructor.
        */
        public InitialHeuristicResult(
            Instance bestInstance,
            PriorityRule bestRule,
            int bestCmax)
        {
            this.bestInstance = bestInstance;
            this.bestRule = bestRule;
            this.bestCmax = bestCmax;
        }
    }
}