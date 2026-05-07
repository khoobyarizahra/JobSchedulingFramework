# Project-Scheduling-ZahraAndCarolin

Modularer Aufbau:


Projektmappe
|
|---Project-Scheduling-ZahraAndCarolin
    |
    |___DataGeneration
    |   |__InstanceGeneratorAdvanced.cs
    |   |__InstanceGeneratorEasy.cs
    |
    |___DataInput
    |   |__InstanceReader.cs
    |
    |___Experiments
    |   |__HeuristicExperiment.cs
    |
    |___Heuristics
    |   |_InitialHeuristic.cs
    |
    |___Instances
    |   |__Ausgaben_screenshots
    |   |__Instanzen.txt
    |
    |___Models
    |   |__instance.cs
    |   |__Jobs.cs
    |   |__Operation.cs
    |   |__PriorityRule.cs
    |
    |___Utils
    |   |__ScheduleEvaluator.cs
    |
    |___Visiualisation
    |   |__GantChart.cs
    |
    |___Program.cs


Was kommt in welchen Ordner:

Models = „Was ist das Problem?“
DataInput = „Wo kommen die Daten her?“
Heuristics = „Wie löse ich es?“
Experiments = „Was ist besser?“
Utils = „Hilfsfunktionen“
Program = „Start“
DataGeneration = "Instanz Generatoren"
Visiualisation = "Visualisationen/Gant Chart"