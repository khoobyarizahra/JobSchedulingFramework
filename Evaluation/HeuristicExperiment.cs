using JobShopSchedulingFramework.Data;
using JobShopSchedulingFramework.Heuristics.Initial;
using JobShopSchedulingFramework.Models;
using JobShopSchedulingFramework.Utils;
using System;

namespace JobShopSchedulingFramework.Evaluation
{
    /*
     HEURISTIC EXPERIMENT

     This class compares all priority rules of the Giffler-Thompson initial heuristic.

     Goal:
     - run every priority rule
     - calculate Cmax
     - find the best rule
     - return the best scheduled instance

     The returned result can later be used as the starting solution for a metaheuristic.
    */
    public class HeuristicExperiment
    {
        /*
         Run

         Executes the comparison for one instance file.

         Returns:
         InitialHeuristicResult containing:
         - best scheduled instance
         - best priority rule
         - best Cmax
        */
        public static InitialHeuristicResult Run(
            string fileName)
        {
            int bestCmax =
                int.MaxValue;

            PriorityRule bestRule =
                PriorityRule.LRPT;

            Instance? bestInstance =
                null;

            Console.WriteLine("EXPERIMENTAL COMPARISON");
            Console.WriteLine();

            foreach (PriorityRule rule in Enum.GetValues(typeof(PriorityRule)))
            {
                Instance instance =
                    InstanceReader.ReadFromFile(fileName);

                GifflerThompsonHeuristic.SetRandomSeed(42);

                GifflerThompsonHeuristic
                    .CalculateRemainingProcessingTimes(instance);

                GifflerThompsonHeuristic
                    .CreateInitialSchedule(
                        instance,
                        rule);

                int cmax =
                    ScheduleEvaluator.CalculateCmax(instance);

                Console.WriteLine(rule + " -> Cmax = " + cmax);

                if (cmax < bestCmax)
                {
                    bestCmax =
                        cmax;

                    bestRule =
                        rule;

                    bestInstance =
                        instance;
                }
            }

            if (bestInstance == null)
            {
                throw new InvalidOperationException(
                    "No initial heuristic solution could be generated.");
            }

            Console.WriteLine();
            Console.WriteLine("Best rule: " + bestRule);
            Console.WriteLine("Best Cmax: " + bestCmax);

            return new InitialHeuristicResult(
                bestInstance,
                bestRule,
                bestCmax);
        }
    }
}