using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Restart;
using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /*
    Ergebnis eines vollständigen Tabu-Search-Laufs.

    Neben dem besten gefundenen Cmax werden auch Laufzeit, Maschinenreihenfolge,
    Suchstatistiken und Restart-Informationen gespeichert. Dadurch kann später
    nicht nur das Ergebnis, sondern auch das Verhalten der Suche analysiert werden.
    */
    public class TabuSearchResult
    {
        // Cmax der Startlösung vor Beginn der Tabu Search.
        public int InitialCmax { get; set; }

        // Bester Cmax, der während der Tabu Search gefunden wurde.
        public int BestCmax { get; set; }

        // Gesamtlaufzeit des Tabu-Search-Laufs.
        public TimeSpan Runtime { get; set; } = TimeSpan.Zero;

        /*
        Maschinenreihenfolge der besten gefundenen Lösung.

        Aus dieser Reihenfolge können die konkreten Start- und Endzeiten erneut
        berechnet oder als Gantt-Diagramm visualisiert werden.
        */
        public Dictionary<int, List<Operation>> BestMachineOrders { get; set; } =
            new Dictionary<int, List<Operation>>();

        // Allgemeine Suchstatistiken, z.B. Iterationen, Abbruchgrund und bewertete Moves.
        public TabuSearchStatistics Statistics { get; set; } =
            new TabuSearchStatistics();

        // Speichert, wie oft Restart ausgeführt wurde und ob Restarts erfolgreich waren.
        public RestartStatistics RestartStatistics { get; set; } =
            new RestartStatistics();

        // Prozentuale Verbesserung gegenüber der Initiallösung.
        public double ImprovementPercent
        {
            get
            {
                if (InitialCmax <= 0)
                {
                    return 0.0;
                }

                return (double)(InitialCmax - BestCmax) / InitialCmax * 100.0;
            }
        }
    }
}