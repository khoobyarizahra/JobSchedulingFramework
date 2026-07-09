using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Restart
{
    /*
    Diese Klasse kapselt die Restart-Strategie der Tabu Search.

    Ein Restart wird verwendet, wenn die Suche über viele Iterationen keine neue
    globale Bestlösung mehr findet. In diesem Fall besteht die Gefahr, dass die
    Tabu Search in einem lokalen Optimum oder in einem wenig produktiven Bereich
    des Suchraums stagniert.

    Die Idee ist deshalb:
    - Die bisher beste Maschinenreihenfolge wird als Ausgangspunkt verwendet.
    - Diese Lösung wird durch mehrere zufällige Swaps leicht verändert.
    - Danach wird aus der veränderten Maschinenreihenfolge wieder ein vollständiger
      Schedule berechnet.
    - Nur wenn dieser Schedule zulässig ist, wird der Restart akzeptiert.

    Wichtig:
    Der Restart startet bewusst nicht von einer komplett zufälligen Lösung.
    Dadurch bleibt die gute Struktur der bisher besten Lösung teilweise erhalten.
    Gleichzeitig wird die Lösung genug verändert, um einen anderen Bereich des
    Suchraums zu untersuchen.
    */
    public class RestartManager
    {
        private readonly TabuSearchSettings settings;
        private readonly Random random;

        public RestartManager(
            TabuSearchSettings settings)
        {
            this.settings = settings;

            /*
            Der feste Seed macht die Restart-Perturbationen reproduzierbar.
            Das ist für Experimente wichtig, weil Varianten dann fairer
            miteinander verglichen werden können.
            */
            random = new Random(settings.RestartRandomSeed);
        }

        public int GetRestartThreshold(
            int operationCount)
        {
            /*
            Die Restart-Schwelle wird abhängig von der Instanzgröße berechnet.

            Beispiel:
            operationCount = 100
            RestartAfterNoImprovementFactor = 25
            => Restart nach 2500 Iterationen ohne globale Verbesserung

            Dadurch wird bei größeren Instanzen nicht zu früh restarted,
            während kleine Instanzen trotzdem nicht unnötig lange stagnieren.
            */
            return operationCount *
                   settings.RestartAfterNoImprovementFactor;
        }

        public int GetPerturbationMoveCount(
            int operationCount)
        {
            /*
            Die Anzahl der zufälligen Swaps bestimmt die Stärke des Restarts.

            Wenige Swaps:
            - Lösung bleibt nahe an der besten bekannten Lösung.
            - Risiko: Die Suche landet wieder im gleichen lokalen Bereich.

            Viele Swaps:
            - Lösung wird stärker verändert.
            - Risiko: Gute Struktur der bisherigen Lösung geht verloren.

            Deshalb wird die Anzahl abhängig von der Operationszahl berechnet,
            aber nie kleiner als RestartMinPerturbationMoves.
            */
            return Math.Max(
                settings.RestartMinPerturbationMoves,
                operationCount / settings.RestartOperationDivisor);
        }

        public bool ShouldRestart(
            int iterationsSinceImprovement,
            int operationCount)
        {
            /*
            Restart wird nur verwendet, wenn er in den Settings aktiviert ist.
            Dadurch können wir später Experimente mit und ohne Restart durchführen.
            */
            if (!settings.UseRestart)
            {
                return false;
            }

            int restartThreshold =
                GetRestartThreshold(
                    operationCount);

            /*
            Sobald die Anzahl der Iterationen ohne globale Verbesserung die
            Restart-Schwelle erreicht, soll die Suche diversifiziert werden.
            */
            return iterationsSinceImprovement >= restartThreshold;
        }

        public RestartResult RestartFromBestSolution(
            Instance instance,
            Dictionary<int, List<Operation>> bestOrders,
            int perturbationMoves)
        {
            /*
            Nicht jede zufällig veränderte Maschinenreihenfolge ist automatisch
            zulässig. Durch Swaps können zyklische Abhängigkeiten entstehen.

            Beispiel:
            Eine Operation wartet auf ihren Job-Vorgänger, während gleichzeitig
            durch die Maschinenreihenfolge eine umgekehrte Abhängigkeit entsteht.
            Dann kann kein gültiger Schedule berechnet werden.

            Deshalb werden mehrere Restart-Versuche erlaubt.
            */
            for (int attempt = 1; attempt <= settings.MaxRestartAttempts; attempt++)
            {
                /*
                Die beste bekannte Maschinenreihenfolge wird kopiert.
                Die Originaldaten dürfen nicht verändert werden, weil sie weiterhin
                als globale Bestlösung gespeichert bleiben müssen.
                */
                Dictionary<int, List<Operation>> restartOrders =
                    ScheduleOrderHelper.CopyMachineOrders(
                        bestOrders);

                /*
                Die Kopie wird durch zufällige Swaps verändert.
                Diese Änderung findet nur auf den Maschinenreihenfolgen statt.
                Start- und Endzeiten werden danach neu berechnet.
                */
                ApplyRandomPerturbation(
                    restartOrders,
                    perturbationMoves);

                /*
                Aus der veränderten Maschinenreihenfolge wird ein vollständiger
                Schedule berechnet. Dabei werden Job-Reihenfolge, Maschinenreihenfolge
                und Setup-Zeiten berücksichtigt.
                */
                bool feasible =
                    ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                        instance,
                        restartOrders,
                        out int restartCmax);

                /*
                Nur zulässige Restart-Lösungen werden akzeptiert.
                Eine unzulässige Maschinenreihenfolge wird verworfen und ein neuer
                Restart-Versuch wird gestartet.
                */
                if (feasible)
                {
                    return new RestartResult
                    {
                        MachineOrders = restartOrders,
                        Cmax = restartCmax,
                        UsedFallbackSolution = false
                    };
                }
            }

            /*
            Falls alle Restart-Versuche unzulässig sind, wird als Sicherheitslösung
            wieder die beste bekannte Maschinenreihenfolge verwendet.

            Dadurch bleibt der Algorithmus stabil:
            Er bricht nicht ab und arbeitet nicht mit einer ungültigen Lösung weiter.
            Der Nachteil ist, dass dieser Restart keine echte Diversifikation bewirkt.
            Deshalb wird UsedFallbackSolution auf true gesetzt, damit wir das später
            in der Restart-Auswertung erkennen können.
            */
            Dictionary<int, List<Operation>> fallbackOrders =
                ScheduleOrderHelper.CopyMachineOrders(
                    bestOrders);

            ScheduleOrderHelper.RecalculateScheduleFromMachineOrders(
                instance,
                fallbackOrders,
                out int fallbackCmax);

            return new RestartResult
            {
                MachineOrders = fallbackOrders,
                Cmax = fallbackCmax,
                UsedFallbackSolution = true
            };
        }

        private void ApplyRandomPerturbation(
            Dictionary<int, List<Operation>> machineOrders,
            int numberOfSwaps)
        {
            /*
            Für zufällige Swaps sind nur Maschinen sinnvoll, auf denen mindestens
            zwei Operationen eingeplant sind. Eine Maschine mit null oder einer
            Operation kann durch einen Swap nicht verändert werden.
            */
            List<int> usableMachines =
                machineOrders
                    .Where(pair => pair.Value.Count >= 2)
                    .Select(pair => pair.Key)
                    .ToList();

            if (usableMachines.Count == 0)
            {
                return;
            }

            for (int i = 0; i < numberOfSwaps; i++)
            {
                /*
                Zuerst wird zufällig eine Maschine ausgewählt.
                Danach werden zwei Positionen innerhalb der Maschinenreihenfolge
                zufällig gewählt und vertauscht.
                */
                int machine =
                    usableMachines[
                        random.Next(usableMachines.Count)];

                List<Operation> operationsOnMachine =
                    machineOrders[machine];

                int firstIndex =
                    random.Next(operationsOnMachine.Count);

                int secondIndex =
                    random.Next(operationsOnMachine.Count);

                /*
                Wenn beide Zufallspositionen gleich sind, würde sich die Reihenfolge
                nicht ändern. Dieser Versuch wird übersprungen.
                */
                if (firstIndex == secondIndex)
                {
                    continue;
                }

                /*
                Der Swap verändert nur die Reihenfolge auf dieser Maschine.
                Ob daraus ein zulässiger vollständiger Schedule entsteht, wird
                anschließend durch ScheduleRecalculator geprüft.
                */
                Operation temp =
                    operationsOnMachine[firstIndex];

                operationsOnMachine[firstIndex] =
                    operationsOnMachine[secondIndex];

                operationsOnMachine[secondIndex] =
                    temp;
            }
        }
    }
}