using System;
using System.IO;
using JobShopSchedulingFramework.Application;
using JobShopSchedulingFramework.Evaluation;
using JobShopSchedulingFramework.Solvers.CPBenchmarkEvaluation;

public class Program
{
    public static void Main(string[] args)
    {
        /*
        Dieses Hauptmenü trennt die wichtigsten Programmteile klar voneinander.

        1 - Interaktive Anwendung:
            Einzelne Instanzen lösen, Gantt-Diagramme erzeugen und CP vergleichen.

        2 - Full benchmark evaluation:
            Ursprünglicher vollständiger Benchmark-Lauf für die Abgabe-CSV.

        3 - CP benchmark only:
            Separater Lauf nur für den CP-Solver.

        4 - Experimental evaluation screening:
            Systematische Variantenanalyse mit Restart-, Penalty-, Tabu-Dauer-,
            Exact-Evaluation- und Nachbarschaftsvarianten.

        5 - Move selection evaluation:
            Separate Analyse der schnellen Move-Abschätzung.

        6 - Adaptive exact evaluation:
            Vergleich fester Grenzen für exakte Move-Bewertung mit der neuen
            adaptiven Strategie.
        */

        Console.WriteLine("Select mode:");
        Console.WriteLine("1 - Interactive application");
        Console.WriteLine("2 - Full benchmark evaluation");
        Console.WriteLine("3 - CP benchmark only");
        Console.WriteLine("4 - Experimental evaluation screening");
        Console.WriteLine("5 - Move selection evaluation");
        Console.WriteLine("6 - Adaptive exact evaluation");
        Console.Write("Choice: ");

        string? choice =
            Console.ReadLine();

        if (choice == "2")
        {
            BenchmarkEvaluationRunner.Run();
        }
        else if (choice == "3")
        {
            CpBenchmarkApplication.Run(
                Path.GetFullPath(
                    Path.Combine(
                        AppContext.BaseDirectory,
                        @"..\..\..\Instances\Benchmark")));
        }
        else if (choice == "4")
        {
            /*
            Diese Option startet die breite Screening-Evaluation.

            Der normale Benchmark-Runner bleibt unverändert, damit die ursprüngliche
            Abgabe-Baseline nicht mit den späteren Experimenten vermischt wird.
            */
            TabuEvaluationRunner.RunScreeningEvaluation();
        }
        else if (choice == "5")
        {
            /*
            Diese Option startet die gezielte Move-Selection-Evaluation.

            Verglichen werden:
            - Baseline mit Move-Abschätzung und 20 exakten Bewertungen
            - keine Abschätzung mit vollständiger exakter Bewertung
            - keine Abschätzung mit zufälliger Auswahl von 20 Kandidaten
            */
            TabuEvaluationRunner.RunMoveSelectionEvaluation();
        }
        else if (choice == "6")
        {
            /*
            Diese Option startet die adaptive Exact-Evaluation.

            Verglichen werden:
            - feste Grenzen mit 20, 50, 100 und 200 exakt bewerteten Kandidaten
            - adaptive Grenze abhängig von Anzahl generierter Moves und Stagnation

            Ziel ist zu prüfen, ob die neue adaptive Strategie einen besseren
            Kompromiss zwischen Laufzeit und Lösungsqualität liefert.
            */
            TabuEvaluationRunner.RunAdaptiveExactEvaluation();
        }
        else
        {
            SchedulingApplication.Run(args);
        }
    }
}