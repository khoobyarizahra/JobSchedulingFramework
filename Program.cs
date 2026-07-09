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
            Dabei wird geprüft, ob die Abschätzung bessere Kandidaten auswählt
            als eine vollständige exakte Bewertung oder eine zufällige Auswahl.
        */

        Console.WriteLine("Select mode:");
        Console.WriteLine("1 - Interactive application");
        Console.WriteLine("2 - Full benchmark evaluation");
        Console.WriteLine("3 - CP benchmark only");
        Console.WriteLine("4 - Experimental evaluation screening");
        Console.WriteLine("5 - Move selection evaluation");
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
            - Estimate_Top20
            - NoEstimate_AllExact
            - NoEstimate_Random20

            Dadurch kann untersucht werden, ob die schnelle Move-Abschätzung
            methodisch sinnvoll ist.
            */
            TabuEvaluationRunner.RunMoveSelectionEvaluation();
        }
        else
        {
            SchedulingApplication.Run(args);
        }
    }
}