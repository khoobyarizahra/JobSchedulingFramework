# Job Shop Scheduling Framework

## Overview

This project implements a framework for solving and analyzing
Job Shop Scheduling Problems (JSSP) with sequence-dependent setup times.

The framework supports:

- generation of benchmark instances
- reading and writing scheduling instances
- construction of schedules using heuristics
- evaluation using makespan (Cmax)
- visualization using Gantt charts
- comparison of different priority rules


# Project Structure


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
│   │
│   ├── Metaheuristics/
│   │   └── (reserved for future extensions)
│   │
│   └── Neighborhoods/
│       └── (reserved for future local search methods)
│
├── Instances/
│   ├── Benchmark/
│   ├── Generated/
│   └── Screenshots/
│
├── Models/
│   ├── Instance.cs
│   ├── Job.cs
│   ├── Operation.cs
│   ├── Schedule.cs
│   └── ScheduledOperation.cs
│
├── Solvers/
│   └── CpSolverRunner.cs
│
├── Utils/
│   └── ScheduleEvaluator.cs
│
├── Visualization/
│   └── GanttChartVisualizer.cs
│
├── Program.cs
│
└── README.md

Arcitecture:
Instanz
   ↓
Giffler-Thompson
   ↓
Schedule
   ↓
Nachbarschaftsmodell
   ↓
kritischer Pfad
   ↓
kritische Blöcke
   ↓
zulässige Nachbarn
   ↓
Tabusuche
