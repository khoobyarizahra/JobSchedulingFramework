## Project Structure

JobShopSchedulingFramework/
│
├── Application/
│   └── SchedulingApplication.cs
│
├── Data/
│   ├── InstanceReader.cs
│   └── InstanceWriter.cs
│
├── DataGeneration/
│   ├── InstanceGeneratorAdvanced.cs
│   ├── InstanceGeneratorEasy.cs
│   └── InstanceType.cs
│
├── Evaluation/
│   ├── HeuristicExperiment.cs
│   └── InitialHeuristicResult.cs
│
├── Heuristics/
│   ├── Initial/
│   │   ├── GifflerThompsonHeuristic.cs
│   │   └── PriorityRule.cs
│   ├── Metaheuristics/
│   └── Neighborhoods/
│
├── Instances/
│  ├── Benchmark
│  ├── generated
│  ├── Screenshots
│
├── Models/
│   ├── Instance.cs
│   ├── Job.cs
│   ├── Operation.cs
│   ├── Schedule.cs
│   └── ScheduledOperation.cs
│
├── Solvers/
│	└── CpSolverRunner.cs
│
├── Utils/
│   └── ScheduleEvaluator.cs
│
├── Visualisation/
│   └── GanttChart.cs
│
├── Program.cs
└── README.md
