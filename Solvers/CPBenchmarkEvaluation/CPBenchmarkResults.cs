namespace JobShopSchedulingFramework.Solvers.CPBenchmarkEvaluation
{
    public class CpBenchmarkRow
    {
        public string InstanceName { get; set; } = "";

        public int Cmax { get; set; }

        public double RuntimeSeconds { get; set; }

        public string Status { get; set; } = "";
    }
}