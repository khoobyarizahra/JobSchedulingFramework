using System.Collections.Generic;
using System.Linq;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Restart
{
    /*
    Diese Klasse sammelt alle Restart-Ereignisse eines Tabu-Search-Laufs.

    Der Restart selbst verbessert die Lösung nicht direkt. Direkt nach einem Restart
    ist der Cmax häufig sogar schlechter, weil die bisher beste Maschinenreihenfolge
    bewusst verändert wird. Entscheidend ist deshalb, ob die Suche nach dem Restart
    einen neuen Bereich des Suchraums erreicht und später eine neue globale Bestlösung
    findet.

    Diese Klasse speichert daher:
    - wie oft Restart ausgeführt wurde,
    - wie viele Restarts später zu einer neuen Bestlösung geführt haben,
    - wie hoch die Erfolgsrate der Restart-Strategie war.
    */
    public class RestartStatistics
    {
        /*
        Jeder Eintrag in Events beschreibt einen konkreten Restart:
        Iteration, Cmax vor und nach dem Restart, Perturbationsstärke und späterer Erfolg.
        */
        public List<RestartEvent> Events { get; } =
            new List<RestartEvent>();

        public int TotalRestarts
        {
            get { return Events.Count; }
        }

        public int SuccessfulRestarts
        {
            get
            {
                /*
                Ein Restart gilt als erfolgreich, wenn nach diesem Restart
                und vor dem nächsten relevanten Suchwechsel eine neue globale
                Bestlösung gefunden wurde.
                */
                return Events.Count(restartEvent =>
                    restartEvent.LedToNewBestSolution);
            }
        }

        public double SuccessRate
        {
            get
            {
                /*
                Die Erfolgsrate ist für die spätere Auswertung wichtig.
                Damit kann geprüft werden, ob Restart wirklich zur Verbesserung
                beiträgt oder hauptsächlich zusätzliche Laufzeit verursacht.
                */
                if (TotalRestarts == 0)
                {
                    return 0.0;
                }

                return (double)SuccessfulRestarts / TotalRestarts;
            }
        }

        public RestartEvent AddRestart(
            int iteration,
            int cmaxBeforeRestart,
            int cmaxAfterRestart,
            int bestCmaxBeforeRestart,
            int perturbationMoves,
            bool usedFallbackSolution)
        {
            /*
            Diese Methode wird direkt nach einem Restart aufgerufen.

            Zu diesem Zeitpunkt ist noch nicht bekannt, ob der Restart erfolgreich war.
            Deshalb wird LedToNewBestSolution zunächst nicht gesetzt. Erst wenn später
            eine neue globale Bestlösung gefunden wird, wird der passende Restart über
            MarkLatestRestartAsSuccessful als erfolgreich markiert.
            */
            RestartEvent restartEvent =
                new RestartEvent
                {
                    Iteration = iteration,
                    CmaxBeforeRestart = cmaxBeforeRestart,
                    CmaxAfterRestart = cmaxAfterRestart,
                    BestCmaxBeforeRestart = bestCmaxBeforeRestart,
                    PerturbationMoves = perturbationMoves,
                    UsedFallbackSolution = usedFallbackSolution
                };

            Events.Add(
                restartEvent);

            return restartEvent;
        }

        public void MarkLatestRestartAsSuccessful(
            int improvementIteration,
            int improvedBestCmax)
        {
            /*
            Wenn nach einem Restart eine neue globale Bestlösung gefunden wird,
            soll der letzte noch nicht erfolgreiche Restart markiert werden.

            Dadurch kann später ausgewertet werden:
            - bei welchem Restart die Verbesserung ausgelöst wurde,
            - nach wie vielen Iterationen die Verbesserung kam,
            - welcher neue beste Cmax erreicht wurde.
            */
            RestartEvent? latestOpenRestart =
                Events
                    .LastOrDefault(restartEvent =>
                        !restartEvent.LedToNewBestSolution);

            /*
            Falls noch kein Restart ausgeführt wurde, gibt es nichts zu markieren.
            Dieser Fall kann auftreten, wenn die Tabu Search bereits vor dem ersten
            Restart eine Verbesserung findet.
            */
            if (latestOpenRestart == null)
            {
                return;
            }

            latestOpenRestart.LedToNewBestSolution = true;
            latestOpenRestart.IterationOfNextImprovement = improvementIteration;
            latestOpenRestart.BestCmaxAfterImprovement = improvedBestCmax;
        }
    }
}