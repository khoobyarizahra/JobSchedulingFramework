using Project_Scheduling_ZahraAndCarolin.DataInput;
using Project_Scheduling_ZahraAndCarolin.Heuristics;
using Project_Scheduling_ZahraAndCarolin.Models;
using Project_Scheduling_ZahraAndCarolin.Utils;
using System;

/*
 HEURISTIC EXPERIMENT
 Diese Klasse führt einen experimentellen Vergleich
 aller definierten Prioritätsregeln durch.

 Ziel:
 - Für jede Regel einen vollständigen Schedule erzeugen
 - Den resultierenden Cmax berechnen
 - Die beste Regel bestimmen

*/

namespace Project_Scheduling_ZahraAndCarolin.Experiments
{
    public class HeuristicExperiment
    {
        /*
         Run-Methode:
         Führt den kompletten Vergleich aller Prioritätsregeln aus.
        */
        public static void Run(string fileName)
        {
            /*
             int.MaxValue = größtmöglicher int-Wert.
             Dadurch ist garantiert, dass der erste echte Cmax kleiner ist.
            */
            int bestCmax = int.MaxValue;

            /*
             Speichert die aktuell beste Prioritätsregel.
             Initialwert ist hier LRPT,
             wird aber später ggf. überschrieben.
            */
            PriorityRule bestRule = PriorityRule.LRPT;

            Console.WriteLine("EXPERIMENTAL COMPARISON\n");

            /*
             Iteriert automatisch über ALLE Werte des Enums PriorityRule.

             Enum.GetValues(typeof(PriorityRule)) liefert:
             - LRPT
             - LPT
             - SPT
             - SRPT
             - Random
             - SetupAwareLRPT

             Dadurch muss man neue Regeln nur im Enum ergänzen.
            */
            foreach (PriorityRule rule in Enum.GetValues(typeof(PriorityRule)))
            {
                /*
                 Für jede Regel wird die Instanz neu geladen.

                 Sehr wichtig:
                 Das Scheduling verändert die Daten
                 (Startzeiten, Endzeiten usw.).
                 Deshalb braucht jede Regel eine "frische" Instanz.
                */
                Instance instance = InstanceReader.ReadFromFile(fileName);

                /*
                 Setzt den Zufalls-Seed.
                 Dadurch bleibt die Random-Regel reproduzierbar,
                 also bei jedem Programmlauf identisch.
                */
                InitialHeuristic.SetRandomSeed(42);
                /*
                 Berechnet für jede Operation
                 die Remaining Processing Time.

                 Wird für:
                 - LRPT
                 - SRPT
                 - SetupAwareLRPT

                 benötigt.
                */
                InitialHeuristic.CalculateRemainingProcessingTimes(instance);

                /*
                 Erzeugt den eigentlichen Schedule
                 mit der aktuell getesteten Prioritätsregel.
                */
                InitialHeuristic.CreateInitialSchedule(instance, rule);

                /*
                 Berechnet den Makespan (Cmax).

                 Cmax = Fertigstellungszeit
                         der letzten Operation.
                */
                int cmax = ScheduleEvaluator.CalculateCmax(instance);

                /*
                 Ausgabe der aktuellen Regel
                 und ihres Ergebnisses.
                */
                Console.WriteLine(rule + " -> Cmax = " + cmax);

                /*
                 Prüft:
                 Ist der neue Cmax besser (kleiner)
                 als der bisher beste?

                 Falls ja:
                 - neuen besten Cmax speichern
                 - zugehörige Regel merken
                */
                if (cmax < bestCmax)
                {
                    bestCmax = cmax;
                    bestRule = rule;
                }
            }

            /*
             Finale Ausgabe
             der besten gefundenen Regel
             und ihres Cmax.
            */
            Console.WriteLine("\nBest rule: " + bestRule);
            Console.WriteLine("Best Cmax: " + bestCmax);
        }
    }
}
