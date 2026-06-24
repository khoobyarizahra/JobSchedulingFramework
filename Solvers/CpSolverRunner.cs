using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JobShopSchedulingFramework.ExactSolvers
{
    public static class CpSolverRunner
    {
        public static CpSolverResult Run(
            Instance instance,
            int timeLimitSeconds)
        {
            CpSatJobShopSolver solver =
                new CpSatJobShopSolver();

            int cpCmax =
                solver.Solve(
                    instance,
                    timeLimitSeconds);

            if (cpCmax != int.MaxValue)
            {
                PrintMachineOrder(instance);
            }
            Console.WriteLine("DEBUG CP status in CpSolverRunner: " + solver.LastStatus);

            return new CpSolverResult
            {
                BestInstance = instance,
                Cmax = cpCmax,
                Status = solver.LastStatus
            };
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

        private static void PrintMachineOrder(
            Instance instance)
        {
            Console.WriteLine();
            Console.WriteLine("CP MACHINE ORDER");
            Console.WriteLine("--------------------------------");

            for (int machine = 1; machine <= instance.NumMachines; machine++)
            {
                List<Operation> operations =
                    instance.Jobs
                        .SelectMany(job => job.Operations)
                        .Where(operation => operation.Machine == machine)
                        .OrderBy(operation => operation.StartTime)
                        .ToList();

                Console.Write("Machine " + machine + ": ");

                Console.WriteLine(
                    string.Join(
                        " -> ",
                        operations.Select(operation =>
                            "J" +
                            operation.JobID +
                            "O" +
                            operation.OperationID)));
            }

            Console.WriteLine();
        }
    }
}