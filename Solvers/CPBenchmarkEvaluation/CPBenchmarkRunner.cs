using JobShopSchedulingFramework.Data;
using JobShopSchedulingFramework.ExactSolvers;
using JobShopSchedulingFramework.Models;
using JobShopSchedulingFramework.Results;
using JobShopSchedulingFramework.Solvers.CPBenchmarkEvaluation;
using System.Diagnostics;


public static class CpBenchmarkRunner
{
    public static List<CpBenchmarkRow> RunBatch(
    List<string> instanceFiles,
    int timeLimitSeconds,
    string outputPath)
    {
        var results = new List<CpBenchmarkRow>();

        var solver = new CpSatJobShopSolver();

        foreach (var file in instanceFiles)
        {
            Console.WriteLine($"CP solving: {Path.GetFileName(file)}");

        
            Instance instance =
                InstanceReader.ReadFromFile(file);

            Stopwatch stopwatch = Stopwatch.StartNew();

            int cmax =
                solver.Solve(instance, timeLimitSeconds);

            stopwatch.Stop();

            CpBenchmarkRow row =
                new CpBenchmarkRow
                {
                    InstanceName = Path.GetFileNameWithoutExtension(file),
                    Cmax = cmax,
                    RuntimeSeconds = stopwatch.Elapsed.TotalSeconds,
                    Status = solver.LastStatus
                };

            results.Add(row);

            CpBenchmarkCsvWriter.AppendRow(
                outputPath,
                row);
        }

        return results;
    }
}