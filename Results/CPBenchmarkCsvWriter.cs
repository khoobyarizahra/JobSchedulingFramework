using JobShopSchedulingFramework.Solvers.CPBenchmarkEvaluation;
using System.Text;

namespace JobShopSchedulingFramework.Results
{
    public static class CpBenchmarkCsvWriter
    {
        public static void WriteHeader(string path)
        {
            File.WriteAllText(
                path,
                "InstanceName;Cmax;RuntimeSeconds;Status"
                + Environment.NewLine,
                Encoding.UTF8);
        }


        public static void AppendRow(
            string path,
            CpBenchmarkRow row)
        {
            string line =
                $"{row.InstanceName};" +
                $"{row.Cmax};" +
                $"{row.RuntimeSeconds:F3};" +
                $"{row.Status}"
                + Environment.NewLine;

            File.AppendAllText(
                path,
                line,
                Encoding.UTF8);
        }
    }
}