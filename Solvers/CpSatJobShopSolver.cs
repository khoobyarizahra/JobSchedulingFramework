using Google.OrTools.Sat;
using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JobShopSchedulingFramework.ExactSolvers
{
    public class CpSatJobShopSolver
    {
        public int Solve(
            Instance instance,
            int timeLimitSeconds)
        {
            CpModel model =
                new CpModel();

            List<Operation> operations =
                instance.Jobs
                    .SelectMany(job => job.Operations)
                    .ToList();

            int horizon =
                CalculateHorizon(instance, operations);

            Dictionary<Operation, IntVar> startVars =
                new Dictionary<Operation, IntVar>();

            Dictionary<Operation, IntVar> endVars =
                new Dictionary<Operation, IntVar>();

            foreach (Operation operation in operations)
            {
                string name =
                    $"J{operation.JobID}_O{operation.OperationID}";

                IntVar start =
                    model.NewIntVar(0, horizon, "start_" + name);

                IntVar end =
                    model.NewIntVar(0, horizon, "end_" + name);

                startVars[operation] = start;
                endVars[operation] = end;

                model.Add(end == start + operation.ProcessingTime);
            }

            AddJobPrecedenceConstraints(
                model,
                instance,
                startVars,
                endVars);

            AddMachineConstraintsWithSetupTimes(
                model,
                instance,
                operations,
                startVars,
                endVars);

            IntVar makespan =
                model.NewIntVar(0, horizon, "makespan");

            List<IntVar> lastOperationEnds =
                new List<IntVar>();

            foreach (Job job in instance.Jobs)
            {
                Operation lastOperation =
                    job.Operations[job.Operations.Count - 1];

                lastOperationEnds.Add(
                    endVars[lastOperation]);
            }

            model.AddMaxEquality(
                makespan,
                lastOperationEnds);

            model.Minimize(makespan);

            CpSolver solver =
                new CpSolver();

            solver.StringParameters =
                $"max_time_in_seconds:{timeLimitSeconds}";

            CpSolverStatus status =
                solver.Solve(model);
            if (status == CpSolverStatus.Optimal ||
            status == CpSolverStatus.Feasible)
            {
                foreach (Operation operation in operations)
                {
                    operation.StartTime =
                        (int)solver.Value(startVars[operation]);

                    operation.EndTime =
                        (int)solver.Value(endVars[operation]);
                }

                return (int)solver.ObjectiveValue;
            }

            return int.MaxValue;

            Console.WriteLine();
            Console.WriteLine("CP-SAT SOLVER");
            Console.WriteLine("Status: " + status);
            Console.WriteLine("Runtime seconds: " + solver.WallTime());

            if (status == CpSolverStatus.Optimal ||
                status == CpSolverStatus.Feasible)
            {
                foreach (Operation operation in operations)
                {
                    operation.StartTime =
                        (int)solver.Value(startVars[operation]);

                    operation.EndTime =
                        (int)solver.Value(endVars[operation]);
                }

                int cmax =
                    (int)solver.ObjectiveValue;

                Console.WriteLine("CP Cmax: " + cmax);

                return cmax;
            }

            Console.WriteLine("No CP solution found.");

            return int.MaxValue;
        }

        private int CalculateHorizon(
            Instance instance,
            List<Operation> operations)
        {
            int processingSum =
                operations.Sum(operation => operation.ProcessingTime);

            int maxSetup =
                0;

            if (instance.SetupTimes != null)
            {
                for (int i = 0; i < instance.SetupTimes.GetLength(0); i++)
                {
                    for (int j = 0; j < instance.SetupTimes.GetLength(1); j++)
                    {
                        maxSetup =
                            Math.Max(maxSetup, instance.SetupTimes[i, j]);
                    }
                }
            }

            return processingSum + operations.Count * maxSetup;
        }

        private void AddJobPrecedenceConstraints(
            CpModel model,
            Instance instance,
            Dictionary<Operation, IntVar> startVars,
            Dictionary<Operation, IntVar> endVars)
        {
            foreach (Job job in instance.Jobs)
            {
                for (int i = 0; i < job.Operations.Count - 1; i++)
                {
                    Operation current =
                        job.Operations[i];

                    Operation next =
                        job.Operations[i + 1];

                    model.Add(
                        startVars[next] >= endVars[current]);
                }
            }
        }

        private void AddMachineConstraintsWithSetupTimes(
            CpModel model,
            Instance instance,
            List<Operation> operations,
            Dictionary<Operation, IntVar> startVars,
            Dictionary<Operation, IntVar> endVars)
        {
            Dictionary<int, List<Operation>> operationsByMachine =
                operations
                    .GroupBy(operation => operation.Machine)
                    .ToDictionary(
                        group => group.Key,
                        group => group.ToList());

            foreach (var pair in operationsByMachine)
            {
                List<Operation> machineOperations =
                    pair.Value;

                for (int i = 0; i < machineOperations.Count - 1; i++)
                {
                    for (int j = i + 1; j < machineOperations.Count; j++)
                    {
                        Operation first =
                            machineOperations[i];

                        Operation second =
                            machineOperations[j];

                        BoolVar firstBeforeSecond =
                            model.NewBoolVar(
                                $"J{first.JobID}O{first.OperationID}_before_J{second.JobID}O{second.OperationID}");

                        int setupFirstSecond =
                            GetSetupTime(instance, first.JobID, second.JobID);

                        int setupSecondFirst =
                            GetSetupTime(instance, second.JobID, first.JobID);

                        model.Add(
                            startVars[second] >=
                            endVars[first] + setupFirstSecond)
                            .OnlyEnforceIf(firstBeforeSecond);

                        model.Add(
                            startVars[first] >=
                            endVars[second] + setupSecondFirst)
                            .OnlyEnforceIf(firstBeforeSecond.Not());
                    }
                }
            }
        }

        private int GetSetupTime(
            Instance instance,
            int previousJobId,
            int nextJobId)
        {
            if (instance.SetupTimes == null)
            {
                return 0;
            }

            return instance.SetupTimes[
                previousJobId - 1,
                nextJobId - 1];
        }
    }
}