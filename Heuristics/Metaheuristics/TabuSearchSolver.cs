using JobShopSchedulingFramework.Heuristics.Metaheuristics;
using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JobShopSchedulingFramework.Heuristics.Tabu
{
    public class TabuSearchSolver
    {
        private readonly int maxIterations;
        //tabuTenure gibt an, wie viele Iterationen ein Move tabu bleibt, nachdem er ausgeführt wurde.
        private readonly int tabuTenure;

        //in konstruktor werden die Parameter maxIterations und tabuTenure übergeben und in den entsprechenden Feldern gespeichert.
        public TabuSearchSolver(int maxIterations, int tabuTenure)
        {
            this.maxIterations = maxIterations;
            this.tabuTenure = tabuTenure;
        }
        //Die Run-Methode führt die Tabu Search aus. Sie nimmt eine Instanz des Job-Shop-Problems als Eingabe
        //und gibt die beste gefundene Cmax zurück.
        public int Run(Instance instance)

        {
            //stopwatch wird gestartet, um die Laufzeit der Tabu Search zu messen.
            Stopwatch stopwatch = Stopwatch.StartNew();
            //timeLimitSeconds gibt die maximale Laufzeit der Tabu Search in Sekunden an. In diesem Fall sind es 90 Sekunden.
            int timeLimitSeconds = 90;
            //iterationsWithoutImprovement zählt, wie viele Iterationen ohne Verbesserung des besten Cmax vergangen sind.
            //Wenn dieser Wert einen bestimmten Schwellenwert überschreitet, wird die Suche abgebrochen.
            int iterationsWithoutImprovement = 0;
            int maxIterationsWithoutImprovement = 20;
            //currentOrders speichert die aktuelle Reihenfolge der Operationen auf den Maschinen. Es ist ein Dictionary,
            //bei dem der Schlüssel die Maschinennummer und der Wert eine Liste von Operationen ist, die auf dieser Maschine ausgeführt werden.
            Dictionary<int, List<Operation>> currentOrders =
                ScheduleOrderHelper.BuildMachineOrders(instance);
            //currentCmax speichert den aktuellen Cmax-Wert der Lösung, die durch currentOrders repräsentiert wird.
            int currentCmax;
            // currentFeasible gibt an, ob die aktuelle Lösung (repräsentiert durch currentOrders) machbar ist.
            // Die Methode RecalculateScheduleFromMachineOrders berechnet den Cmax-Wert basierend auf der Reihenfolge der Operationen und überprüft gleichzeitig die Machbarkeit der Lösung.
            bool currentFeasible =
                ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                    instance,
                    currentOrders,
                    out currentCmax);

            if (!currentFeasible)
                throw new InvalidOperationException("Initial schedule is infeasible.");

            int bestCmax = currentCmax;
            //currentOrders wird als Ausgangspunkt für die Tabu Search verwendet,
            //und bestOrders speichert die beste gefundene Reihenfolge der Operationen auf den Maschinen.

            Dictionary<int, List<Operation>> bestOrders =
                ScheduleOrderHelper.CopyMachineOrders(currentOrders);

            Dictionary<string, int> tabuList =
                new Dictionary<string, int>();

            Console.WriteLine();
            Console.WriteLine("TABU SEARCH");
            Console.WriteLine("Initial Cmax: " + currentCmax);

            for (int iteration = 1; iteration <= maxIterations; iteration++)
            {
                if (stopwatch.Elapsed.TotalSeconds >= timeLimitSeconds)
                {
                    Console.WriteLine("Time limit of 90 seconds reached. Stop.");
                    break;
                }

                RemoveExpiredTabuMoves(tabuList, iteration);

                //zuerst wird der kritische Pfad der aktuellen Lösung berechnet, indem die Methode FindCriticalPath aufgerufen wird.
                //Der kritische Pfad besteht aus den Operationen, die den längsten Pfad durch das Zeitplanungsdiagramm bilden und somit den Cmax-Wert bestimmen.
                List<Operation> criticalPath =
                    CriticalPathFinder.FindCriticalPath(instance, currentOrders);
                //Anschließend werden die kritischen Blöcke aus dem kritischen Pfad extrahiert.
                //Ein kritischer Block ist eine zusammenhängende Sequenz von Operationen auf derselben Maschine, die alle auf dem kritischen Pfad liegen.
                List<CriticalBlock> criticalBlocks =
                    CriticalPathFinder.ExtractCriticalBlocks(criticalPath);
                //then werden mögliche Nachbarschaftsbewegungen generiert, indem die Methode GenerateAdjacentSwapMoves aufgerufen wird.
                List<Move> moves =
                    NeighborhoodGenerator.GenerateAdjacentSwapMoves(criticalBlocks);

                if (moves.Count == 0)
                {
                    Console.WriteLine("No critical-block moves found. Stop.");
                    break;
                }
                //Move? hat ?, was bedeutet, dass die Variable bestMoveThisIteration entweder ein Move-Objekt oder null sein kann.
                Move? bestMoveThisIteration = null;
                int bestCandidateCmax = int.MaxValue;

                Dictionary<int, List<Operation>>? bestCandidateOrders = null;
                //wir iterieren über alle generierten Bewegungen (moves) und bewerten jede Bewegung,
                //um die beste Bewegung für diese Iteration zu finden.
                foreach (Move move in moves)
                {
                    //Für jede Bewegung wird eine Kopie der aktuellen Reihenfolge der Operationen auf den Maschinen erstellt,
                    //um die Auswirkungen der Bewegung zu simulieren.
                    Dictionary<int, List<Operation>> candidateOrders =
                        ScheduleOrderHelper.CopyMachineOrders(currentOrders);

                    ScheduleOrderHelper.ApplyMove(candidateOrders, move);

                    int candidateCmax;
                    //Die Methode RecalculateScheduleFromMachineOrders wird aufgerufen,
                    //um den Cmax-Wert der neuen Lösung zu berechnen, die durch die Anwendung der Bewegung entsteht.
                    //Die Lösung ist nicht machbar, falls die Bewegung zu Konflikten oder ungültigen Zeitplänen führt,
                    //und in diesem Fall wird die Bewegung übersprungen.
                    bool feasible =
                        ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                            instance,
                            candidateOrders,
                            out candidateCmax);

                    if (!feasible)
                        continue;
                    //Es wird überprüft, ob die Bewegung tabu ist, indem der Schlüssel der Bewegung in der Tabu-Liste nachgeschlagen wird.
                    bool isTabu =
                        tabuList.ContainsKey(move.GetKey());
                    //aspiration gibt an, ob die Bewegung trotz Tabu-Status akzeptiert werden kann,
                    //wenn sie eine Verbesserung gegenüber dem besten bisher gefundenen Cmax darstellt.
                    bool aspiration =
                        candidateCmax < bestCmax;
                    //Wenn die Bewegung tabu ist und keine Aspiration vorliegt, wird sie übersprungen.
                    if (isTabu && !aspiration)
                        continue;
                    //Wenn die Bewegung nicht tabu ist oder eine Aspiration vorliegt,
                    //wird sie als Kandidat für die beste Bewegung dieser Iteration betrachtet.
                    if (candidateCmax < bestCandidateCmax)
                    {
                        bestCandidateCmax = candidateCmax;
                        //Wenn diese Bewegung die beste bisher gefundene Bewegung in dieser Iteration ist,
                        //wird sie als bestMoveThisIteration gespeichert,
                        bestMoveThisIteration = move;
                        //und die entsprechende Reihenfolge der Operationen (candidateOrders) wird als bestCandidateOrders gespeichert.
                        bestCandidateOrders = candidateOrders;
                    }
                }
                //Nach der Bewertung aller Bewegungen wird überprüft, ob eine gültige Bewegung gefunden wurde.
                if (bestMoveThisIteration == null || bestCandidateOrders == null)
                {
                    Console.WriteLine("No admissible move found. Stop.");
                    break;
                }
                //Wenn eine gültige Bewegung gefunden wurde, wird diese Bewegung auf die aktuelle Lösung angewendet
                currentOrders = bestCandidateOrders;
                //und der Cmax-Wert der neuen Lösung wird berechnet.
                ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                    instance,
                    currentOrders,
                    out currentCmax);
                //Die Bewegung wird in die Tabu-Liste aufgenommen,
                //indem der Schlüssel der Bewegung mit dem Wert der Iteration plus der Tabu-Dauer (tabuTenure) gespeichert wird.
                tabuList[bestMoveThisIteration.GetReverseKey()] =
                    iteration + tabuTenure;
                //Anschließend wird überprüft, ob der Cmax-Wert der neuen Lösung besser ist als der beste bisher gefundene Cmax.
                if (currentCmax < bestCmax)
                {
                    bestCmax = currentCmax;
                    //Wenn die neue Lösung besser ist, wird sie als neue beste Lösung gespeichert,
                    //indem bestOrders auf die aktuelle Reihenfolge der Operationen gesetzt wird.
                    bestOrders =
                        ScheduleOrderHelper.CopyMachineOrders(currentOrders);
                    //und der Zähler für Iterationen ohne Verbesserung wird zurückgesetzt.
                    iterationsWithoutImprovement = 0;
                }
                else
                {
                    //wenn die neue Lösung nicht besser ist, wird der Zähler für Iterationen ohne Verbesserung erhöht.
                    //wir brauchen diesen Zähler, um zu verfolgen, wie viele Iterationen vergangen sind,
                    //ohne dass eine Verbesserung des besten Cmax erreicht wurde.
                    iterationsWithoutImprovement++;
                }

                Console.WriteLine(
                    "Iteration " + iteration +
                    " | Move: " + bestMoveThisIteration +
                    " | Current Cmax: " + currentCmax +
                    " | Best Cmax: " + bestCmax);
                //Wenn die Anzahl der Iterationen ohne Verbesserung einen bestimmten Schwellenwert (maxIterationsWithoutImprovement) überschreitet,
                //dann wird die Suche abgebrochen, da dies darauf hindeutet, dass die Suche in einem lokalen Optimum feststeckt
                //und keine weiteren Verbesserungen zu erwarten sind.
                if (iterationsWithoutImprovement >= maxIterationsWithoutImprovement)
                {
                    Console.WriteLine(
                        "No improvement for " +
                        maxIterationsWithoutImprovement +
                        " iterations. Stop.");

                    break;
                }
            }
            //Nachdem die Tabu Search abgeschlossen ist, wird die beste gefundene Lösung (bestOrders) verwendet,
            //um den endgültigen Cmax-Wert zu berechnen.
            ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                instance,
                bestOrders,
                out bestCmax);

            Console.WriteLine("Final best Cmax after Tabu Search: " + bestCmax);

            return bestCmax;
        }
        //Die Methode RemoveExpiredTabuMoves wird verwendet, um abgelaufene Tabu-Bewegungen aus der Tabu-Liste zu entfernen.
        private void RemoveExpiredTabuMoves(
            Dictionary<string, int> tabuList,
            int currentIteration)
        {
            //Die Methode iteriert über die Tabu-Liste und identifiziert alle Bewegungen, deren Tabu-Dauer abgelaufen ist.
            //where-Klausel filtert die Einträge in der Tabu-Liste, um diejenigen zu finden, deren Wert (die Iteration,
            //bis zu der die Bewegung tabu ist) kleiner oder gleich der aktuellen Iteration ist. pair.value ist die Iteration,
            //bis zu der die Bewegung tabu ist, und currentIteration ist die aktuelle Iteration der Tabu Search.
            //select-Klausel extrahiert die Schlüssel (die Bewegungs-Keys) der abgelaufenen Tabu-Bewegungen und
            //erstellt eine Liste dieser Schlüssel.
            List<string> expiredKeys = tabuList
                .Where(pair => pair.Value <= currentIteration)
                .Select(pair => pair.Key)
                .ToList();

            foreach (string key in expiredKeys)
            {
                tabuList.Remove(key);
            }
        }
    }
}