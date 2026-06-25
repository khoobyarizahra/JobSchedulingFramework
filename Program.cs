using JobShopSchedulingFramework.Application;
using JobShopSchedulingFramework.Evaluation;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Select mode:");
        Console.WriteLine("1 - Interactive application");
        Console.WriteLine("2 - Full benchmark evaluation");
        Console.Write("Choice: ");

        string? choice =
            Console.ReadLine();

        if (choice == "2")
        {
            BenchmarkEvaluationRunner.Run();
        }
        else
        {
            SchedulingApplication.Run(args);
        }
    }
}