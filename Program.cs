using JobShopSchedulingFramework.Application;

/*
 Program.cs

 This is the only entry point of the project.

 It should stay very small.
 All real workflow logic is moved to SchedulingApplication.
*/

public class Program
{
    public static void Main(string[] args)
    {
        SchedulingApplication.Run(args);
    }
}