using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods;
using JobShopSchedulingFramework.Models;
using System.Diagnostics;
using System.Linq;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /// <summary>
    /// Führt die Tabu Search aus.
    /// Die Klasse steuert den gesamten Suchprozess:
    /// Erzeugung von Nachbarschaften, Bewertung von Kandidaten,
    /// Anwendung der Tabu-Regeln und Aktualisierung der besten Lösung.
    /// </summary>
    public class TabuSearchSolver
    {
        // Maximale Anzahl von Iterationen der Tabu Search.
        private readonly int maxIterations;

        // Zeitlimit der Suche
        private readonly int timeLimitSeconds;

        // Einzige, feste Nachbarschaftsstruktur (keine variable Nachbarschaft).
        private readonly INeighborhoodDefinition neighborhood;

        /* Entfernt:
        private readonly INeighborhoodDefinition mainNeighborhood;
        private readonly INeighborhoodDefinition diversificationNeighborhood;
        private int iterationsWithoutImprovement;
        private const int StagnationLimit = 300;
        */


        public TabuSearchSolver(
        int maxIterations,
        int timeLimitSeconds,
        INeighborhoodDefinition neighborhoodDefinition)
            {
            this.maxIterations = maxIterations;
            this.timeLimitSeconds = timeLimitSeconds;

            //NEU
            neighborhood = neighborhoodDefinition;
        }


        /* Entfernt
            mainNeighborhood = neighborhoodDefinition;
            diversificationNeighborhood = new CriticalBlockInsertNeighborhood();
            iterationsWithoutImprovement = 0;
             }
        */

        /// <summary>
        /// Führt die Tabu Search auf der übergebenen Instanz aus
        /// und gibt den besten gefundenen Makespan zurück.
        /// </summary>


        public int Run(Instance instance)
        {
            Stopwatch stopwatch =
            Stopwatch.StartNew();
            // Maschinenreihenfolgen aus dem aktuellen Schedule erzeugen.
            Dictionary<int, List<Operation>> currentOrders =
                ScheduleOrderHelper.BuildMachineOrders(instance);

            // Prüfen, ob die Ausgangslösung zulässig ist.
            bool initialFeasible =
                ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                    instance,
                    currentOrders,
                    out int currentCmax);

            if (!initialFeasible)
            {
                throw new InvalidOperationException(
                    "Initial schedule infeasible.");
            }

            // Die Startlösung ist zunächst auch die beste bekannte Lösung.
            int bestCmax = currentCmax;

            Dictionary<int, List<Operation>> bestOrders =
                ScheduleOrderHelper.CopyMachineOrders(currentOrders);

            // Tabu-Liste mit dynamischer Tenure erzeugen.
            MoveTabuList tabuList =
                new MoveTabuList(
                    instance.NumJobs,
                    instance.NumMachines,
                    maxIterations);

            Console.WriteLine();
            Console.WriteLine("TABU SEARCH PROCESS");
            Console.WriteLine("Initial Cmax: " + currentCmax);
            Console.WriteLine();

            Console.WriteLine(
                "Iter".PadRight(8) + " | " +
                "Current".PadRight(10) + " | " +
                "Best".PadRight(10) + " | " +
                "Tenure".PadRight(8) + " | " +
                "Move");

            Console.WriteLine(new string('-', 95));

            // Zähler für Iterationen nd Iteration ohne Verbesserung
            int iteration = 0;
int iterationsSinceImprovement = 0;


            while (
    iteration < maxIterations &&
    iterationsSinceImprovement < 2000 &&
    stopwatch.Elapsed.TotalSeconds < timeLimitSeconds)
{
    iteration++;

               


                /* Enternt 
                                // Hauptschleife der Tabu Search.
                                for (int iteration = 1; iteration <= maxIterations; iteration++)
                            {
                                if (stopwatch.Elapsed.TotalSeconds >= timeLimitSeconds)
                                {
                                    Console.WriteLine(
                                        $"Time limit reached after {stopwatch.Elapsed.TotalSeconds:F2} seconds.");

                                    break;
                                }
                */

                // Aktualisiert die Tabu-Dauer in regelmäßigen Abständen.
                tabuList.UpdateTenureIfNeeded(iteration);

                // Kritische Operationen der aktuellen Lösung bestimmen.
                CriticalOperationAnalysisResult analysisResult =
                    CriticalOperationAnalyzer.Analyze(
                        instance,
                        currentOrders);

                // Kritische Operationen zu kritischen Blöcken zusammenfassen.
                List<CriticalBlock> criticalBlocks =
                    CriticalBlockBuilder.BuildCriticalBlocks(
                        currentOrders,
                        analysisResult.criticalOperations);
                //Debug
                if (iteration <= 10)
                {
                    Console.WriteLine(
                        $"Critical operations: {analysisResult.criticalOperations.Count}");
                }

            
                /* Diversifizierung enfernt:
                 *
                bool useDiversification =
                iterationsWithoutImprovement >= StagnationLimit;

                INeighborhoodDefinition activeNeighborhood =
                    useDiversification
                        ? diversificationNeighborhood
                        : mainNeighborhood;

                if (useDiversification)
                {
                    Console.WriteLine();
                    Console.WriteLine(
                        "Diversification triggered: switching to " +
                        diversificationNeighborhood.GetType().Name);
                    Console.WriteLine();
                }
                if (iterationsWithoutImprovement == StagnationLimit)
                {
                    Console.WriteLine();
                    Console.WriteLine(
                        "Diversification triggered: switching to " +
                        diversificationNeighborhood.GetType().Name);
                    Console.WriteLine();
                }
                */
                List<Move> moves =
                    neighborhood.GenerateMoves(
                        instance,
                        currentOrders,
                        criticalBlocks);



                if (iteration <= 10 || iteration % 100 == 0)
                {
                    Console.WriteLine(
                        "Iteration " + iteration +
                        " | Neighborhood: " + neighborhood.GetType().Name +
                        " | Critical blocks: " + criticalBlocks.Count +
                        " | Generated moves: " + moves.Count);
                }
    
                
                if (moves.Count == 0)
                {
                    Console.WriteLine("No neighborhood moves found.");
                    break;
                }
                
                Move bestMove = null;
                int bestCandidateCmax = int.MaxValue;
                int bestCandidateEvaluationValue = int.MaxValue;

                Dictionary<int, List<Operation>> bestCandidateOrders =
                    null;




                // Alle Kandidatenlösungen untersuchen.
      

                foreach (Move move in moves)
                {
                    // Kopie der aktuellen Lösung erzeugen.
                    Dictionary<int, List<Operation>> candidateOrders =
                        ScheduleOrderHelper.CopyMachineOrders(
                            currentOrders);

                    // Move auf die Kopie anwenden.
                    ApplyMove(candidateOrders, move);

                    // Makespan der Kandidatenlösung berechnen.
                    bool candidateFeasible =
                        ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                            instance,
                            candidateOrders,
                            out int candidateCmax);

                    if (!candidateFeasible)
                    {
                        continue;
                    }

                    // Prüfen, ob der Move tabu ist.
                    bool isTabu =
                        tabuList.IsTabu(
                            move,
                            iteration,
                            candidateCmax,
                            bestCmax,
                            currentCmax);

                    if (isTabu)
                    {
                        continue;
                    }

                    // Häufig verwendete Moves erhalten eine Frequenzstrafe.
                    int frequencyPenalty =
                        tabuList.GetFrequencyPenalty(move);

                    // Bewertungsfunktion der Tabu Search.

                    //statisch
                    /*
                    int candidateEvaluationValue =
                        candidateCmax + 15 * frequencyPenalty;
                    */

                    //Cmax abhängig
                    double penaltyWeight = candidateCmax * 0.03;

                     double candidateEvaluationValue =
                         candidateCmax +
                         penaltyWeight * frequencyPenalty;
                   
                    

                    //Anzahl Mschienen & Jobs abhängig
                    /*  int penaltyWeight =
                          (int)Math.Round(
                              Math.Sqrt(instance.NumJobs * instance.NumMachines));

                      int candidateEvaluationValue =
                          candidateCmax +
                              penaltyWeight * frequencyPenalty;
                     */
                    // Besten zulässigen Kandidaten auswählen.
                    if (candidateEvaluationValue <
                                  bestCandidateEvaluationValue)
                                      {
                                          bestCandidateEvaluationValue =
                                              candidateEvaluationValue;

                                            bestCandidateCmax =
                                                candidateCmax;

                                            bestMove =
                                                move;
                               
                                           bestCandidateOrders =
                                              candidateOrders;
                                        }
                
            
                  

                }
                //Debug
                if (iteration <= 10 || iteration % 100 == 0)
                {
                    Console.WriteLine(
                        $"Best candidate Cmax: {bestCandidateCmax}");
                }

                // Falls kein zulässiger Kandidat existiert, wird die Suche beendet.
                if (bestMove == null ||
                    bestCandidateOrders == null)
                {
                    Console.WriteLine("No admissible move found.");
                    iterationsSinceImprovement++;
                    continue;
                }

                // Zur besten Nachbarlösung wechseln.
                currentOrders =
                    bestCandidateOrders;

                currentCmax =
                    bestCandidateCmax;

                // Ausgeführten Move in die Tabu-Liste eintragen.
                tabuList.RegisterMove(
                    bestMove,
                    iteration);
                // Globale Bestlösung aktualisieren.
                // If the current solution improves the global best solution,
                // the stagnation counter is reset. Otherwise, it is increased.
                if (currentCmax < bestCmax)
                {
                    bestCmax = currentCmax;

                    bestOrders =
                        ScheduleOrderHelper.CopyMachineOrders(currentOrders);

                    iterationsSinceImprovement = 0;   // nur hier reset
                }
                else
                {
                    iterationsSinceImprovement++;
                }


                    /* Entfernt Dviersifikation:
                    if (useDiversification)
                    {
                        iterationsWithoutImprovement = 0;
                    }
                    */


                    /* Diversifikation entfernt:
                                        iterationsWithoutImprovement = 0;
                                    }
                                    else
                                    {
                                        iterationsWithoutImprovement++;
                                    }
                    */


                    if (iteration <= 10 || iteration % 100 == 0)
                {

                    Console.WriteLine(
                        iteration.ToString().PadRight(8) + " | " +
                        currentCmax.ToString().PadRight(10) + " | " +
                        bestCmax.ToString().PadRight(10) + " | " +
                        tabuList.CurrentTenure.ToString().PadRight(8) + " | " +
                        bestMove);
                }
            }

            // Den besten gefundenen Schedule erneut berechnen,
            // damit die Zeiten der Operationen konsistent sind.
            ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                instance,
                bestOrders,
                out bestCmax);

            Console.WriteLine();
            Console.WriteLine("Final best Cmax: " + bestCmax);
            PrintMachineOrder(bestOrders);

            return bestCmax;
        }

        /// <summary>
        /// Vertauscht zwei Operationen innerhalb einer Maschinenreihenfolge.
        /// </summary>
        private void ApplyMove(
    Dictionary<int, List<Operation>> machineOrders,
    Move move)
        {
            List<Operation> operationsOnMachine =
                machineOrders[move.Machine];

            if (move.IsInsertMove)
            {
                Operation movedOperation =
                    operationsOnMachine[move.MachineIndex1];

                operationsOnMachine.RemoveAt(
                    move.MachineIndex1);

                int targetIndex =
                    move.MachineIndex2;

                if (move.MachineIndex1 < move.MachineIndex2)
                {
                    targetIndex--;
                }

                operationsOnMachine.Insert(
                    targetIndex,
                    movedOperation);

                return;
            }

            Operation temp =
                operationsOnMachine[move.MachineIndex1];

            operationsOnMachine[move.MachineIndex1] =
                operationsOnMachine[move.MachineIndex2];

            operationsOnMachine[move.MachineIndex2] =
                temp;
        }




        private void PrintMachineOrder(
    Dictionary<int, List<Operation>> machineOrders)
        {
            Console.WriteLine();
            Console.WriteLine("TABU MACHINE ORDER");
            Console.WriteLine("--------------------------------");

            foreach (var pair in machineOrders.OrderBy(pair => pair.Key))
            {
                Console.Write("Machine " + pair.Key + ": ");

                Console.WriteLine(
                    string.Join(
                        " -> ",
                        pair.Value.Select(operation =>
                            "J" +
                            operation.JobID +
                            "O" +
                            operation.OperationID)));
            }

            Console.WriteLine();
        }
    }
}