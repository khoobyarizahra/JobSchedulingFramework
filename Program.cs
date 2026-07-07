using JobShopSchedulingFramework.Application;
using JobShopSchedulingFramework.Evaluation;
using JobShopSchedulingFramework.Solvers.CPBenchmarkEvaluation;

public class Program
{
    public static void Main(string[] args)
    {
        
        Console.WriteLine("Select mode:");
        Console.WriteLine("1 - Interactive application");
        Console.WriteLine("2 - Full benchmark evaluation");
        Console.WriteLine("3 - CP benchmark only");
        Console.Write("Choice: ");

        string? choice =
            Console.ReadLine();

        if (choice == "2")
        {
            BenchmarkEvaluationRunner.Run();
        }
        else if (choice == "3")
        {
            CpBenchmarkApplication.Run(
                Path.GetFullPath(
                    Path.Combine(
                        AppContext.BaseDirectory,
                        @"..\..\..\Instances\Benchmark")));
        }
        else
        {
            SchedulingApplication.Run(args);
        }
    }
}