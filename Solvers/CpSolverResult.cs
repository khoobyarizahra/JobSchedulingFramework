using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.ExactSolvers
{
    public class CpSolverResult
    {
        public Instance? BestInstance { get; set; }

        public int Cmax { get; set; } = int.MaxValue;

        public string Status { get; set; } = "UNKNOWN";

        public bool HasFeasibleSolution
        {
            get
            {
                return BestInstance != null &&
                       Cmax != int.MaxValue;
            }
        }
    }
}