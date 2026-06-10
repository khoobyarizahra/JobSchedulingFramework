using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.ExactSolvers
{
    public class CpSolverResult
    {
        public Instance BestInstance { get; set; }

        public int Cmax { get; set; }

        public bool HasFeasibleSolution
        {
            get
            {
                return Cmax != int.MaxValue;
            }
        }
    }
}