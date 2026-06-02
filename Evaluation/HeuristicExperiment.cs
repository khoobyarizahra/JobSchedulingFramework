using JobShopSchedulingFramework.Data;
using JobShopSchedulingFramework.Heuristics;
using JobShopSchedulingFramework.Heuristics.Initial;
using JobShopSchedulingFramework.Models;
using JobShopSchedulingFramework.Utils;
using System;

namespace JobShopSchedulingFramework.Evaluation
{
    /*
     HEURISTIC EXPERIMENT

     This class compares all priority rules of the
     Giffler-Thompson initial heuristic.

     Goal:
     - run every priority rule
     - calculate Cmax
     - find the best rule
     - return the best scheduled instance

     The returned result can later be used as the
     starting solution for a metaheuristic.
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
        public static InitialHeuristicResult Run(string fileName)
        {
            // Start with the largest possible value.
            int bestCmax = int.MaxValue;

            // Default value; will be replaced if another rule is better.
            PriorityRule bestRule = PriorityRule.LRPT;

            // Stores the best scheduled instance.
            Instance bestInstance = null;

            Console.WriteLine("EXPERIMENTAL COMPARISON");
            Console.WriteLine();

            /*
             Run all priority rules defined in PriorityRule enum.
            */
            foreach (PriorityRule rule in Enum.GetValues(typeof(PriorityRule)))
            {
                /*
                 Reload the instance for every rule.

                 Important:
                 Scheduling changes Operation start/end times.
                 Therefore each priority rule needs a fresh instance.
                */
                Instance instance =
                    InstanceReader.ReadFromFile(fileName);

                /*
                 Set seed for reproducible random priority rule.
                */
                GifflerThompsonHeuristic.SetRandomSeed(42);

                /*
                 Calculate remaining processing times.

                 Needed for:
                 - LRPT
                 - SRPT
                 - SetupAwareLRPT
                */
                GifflerThompsonHeuristic
                    .CalculateRemainingProcessingTimes(instance);

                /*
                 Create schedule using the current priority rule.
                */
                GifflerThompsonHeuristic
                    .CreateInitialSchedule(instance, rule);

                /*
                 Calculate makespan.
                */
                int cmax =
                    ScheduleEvaluator.CalculateCmax(instance);

                Console.WriteLine(rule + " -> Cmax = " + cmax);

                /*
                 Check whether this rule is currently the best.
                */
                if (cmax < bestCmax)
                {
                    bestCmax = cmax;
                    bestRule = rule;
                    bestInstance = instance;
                }
            }

            Console.WriteLine();
            Console.WriteLine("Best rule: " + bestRule);
            Console.WriteLine("Best Cmax: " + bestCmax);

            /*
             Return the best result.

             Later:
             TabuSearch can start from result.bestInstance.
            */
            return new InitialHeuristicResult(
                bestInstance,
                bestRule,
                bestCmax
            );
        }
    }
}