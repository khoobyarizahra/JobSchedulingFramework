using JobShopSchedulingFramework.Models;
using System;

namespace JobShopSchedulingFramework.ExactSolvers
{
    public static class CpSolverRunner
    {
        public static int Run(
            Instance instance,
            int timeLimitSeconds)
        {
            CpSatJobShopSolver solver =
                new CpSatJobShopSolver();

            int cpCmax =
                solver.Solve(
                    instance,
                    timeLimitSeconds);

            return cpCmax;
        }

        public static void PrintComparison(
            int initialCmax,
            int tabuCmax,
            int cpCmax)
        {
            Console.WriteLine();
            Console.WriteLine("CP BENCHMARK COMPARISON");
            Console.WriteLine("--------------------------------");

            Console.WriteLine("Initial Cmax: " + initialCmax);
            Console.WriteLine("Tabu Cmax:    " + tabuCmax);
            Console.WriteLine("CP Cmax:      " + cpCmax);

            if (cpCmax == int.MaxValue)
            {
                Console.WriteLine("CP did not find a feasible solution.");
                return;
            }

            double initialGap =
                ((double)(initialCmax - cpCmax) / cpCmax) * 100.0;

            double tabuGap =
                ((double)(tabuCmax - cpCmax) / cpCmax) * 100.0;

            Console.WriteLine("Initial gap:  " + initialGap.ToString("F2") + "%");
            Console.WriteLine("Tabu gap:     " + tabuGap.ToString("F2") + "%");
        }
    }
}