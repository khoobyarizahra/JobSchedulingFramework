using System;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /// <summary>
    /// Verwaltet die Tabu-Liste für die Tabu Search.
    ///
    /// Grundidee:
    /// Wenn ein Move ausgeführt wird, wird sein Reverse-Move tabu gesetzt.
    /// Dadurch wird verhindert, dass die Suche sofort zur vorherigen Lösung zurückkehrt.
    ///
    /// Die Tabu-Dauer ist nicht statisch, sondern wird abhängig von der Instanzgröße
    /// und mit einer kleinen zufälligen Variation bestimmt.
    ///
    /// Zusätzlich wird gezählt, wie häufig ein Move bereits verwendet wurde.
    /// Diese Information kann später als Frequenzstrafe genutzt werden.
    /// </summary>
    public class MoveTabuList
    {
        // Speichert für jeden tabuisierten Move,
        // bis zu welcher Iteration dieser Move tabu bleibt.
        private readonly Dictionary<string, int> tabuUntil;

        // Speichert, wie oft ein Move bereits ausgeführt wurde.
        private readonly Dictionary<string, int> moveFrequency;

        // Zufallszahlengenerator für leichte Tenure-Variation.
        private readonly Random random;

        // Basiswert der Tabu-Dauer.
        private readonly int baseTenure;

        // Untere Grenze der dynamischen Tenure.
        private readonly int minTenure;

        // Obere Grenze der dynamischen Tenure.
        private readonly int maxTenure;

        // Gibt an, nach wie vielen Iterationen die Tenure neu erzeugt wird.
        private readonly int updateInterval;

        // Aktuell verwendete Tabu-Dauer.
        private int currentTenure;

        /// <summary>
        /// Erstellt eine neue Tabu-Liste.
        ///
        /// Die Tenure wird abhängig von der Anzahl der Jobs und Maschinen berechnet.
        /// Dadurch ist sie instanzabhängig und nicht willkürlich fest gewählt.
        /// </summary>
        public MoveTabuList(
            int numberOfJobs,
            int numberOfMachines,
            int maxIterations)
        {
            tabuUntil =
                new Dictionary<string, int>();

            moveFrequency =
                new Dictionary<string, int>();

            // Fester Seed, damit die Ergebnisse reproduzierbar bleiben.
            random =
                new Random(42);

            // Instanzabhängige Basis-Tenure berechnen.
            baseTenure =
                CalculateBaseTenure(
                    numberOfJobs,
                    numberOfMachines);

            // Dynamischen Bereich um die Basis-Tenure festlegen.
            minTenure =
                Math.Max(
                    1,
                    (int)Math.Round(baseTenure * 0.8));

            maxTenure =
                Math.Max(
                    minTenure + 1,
                    (int)Math.Round(baseTenure * 1.2));

            // Alle 5 % der maximalen Iterationen wird eine neue Tenure gewählt.
            updateInterval =
                Math.Max(
                    1,
                    maxIterations / 20);

            // Erste Tenure erzeugen.
            currentTenure =
                GenerateDynamicTenure();
        }

        /// <summary>
        /// Berechnet die Basis-Tenure abhängig von der Größe der Instanz.
        ///
        /// Für Job Shop Scheduling ist eine zu große Tenure problematisch,
        /// weil dann zu viele Moves blockiert werden.
        ///
        /// Deshalb verwenden wir hier nicht:
        /// numberOfJobs * numberOfMachines / 2
        ///
        /// Stattdessen orientieren wir uns an:
        /// numberOfJobs + numberOfMachines
        ///
        /// Beispiel:
        /// 10 Jobs, 5 Maschinen:
        /// (10 + 5) / 2 = 7
        ///
        /// 10 Jobs, 10 Maschinen:
        /// (10 + 10) / 2 = 10
        /// </summary>
        private int CalculateBaseTenure(
            int numberOfJobs,
            int numberOfMachines)
        {
            int problemSize =
                numberOfJobs + numberOfMachines;

            return Math.Max(
                3,
                problemSize / 2);
        }

        /// <summary>
        /// Erzeugt eine neue dynamische Tenure.
        ///
        /// Die Tenure liegt zwischen 80 % und 120 % der Basis-Tenure.
        /// Dadurch bleibt die Suche flexibler als bei einer konstanten Tabu-Dauer.
        /// </summary>
        private int GenerateDynamicTenure()
        {
            return random.Next(
                minTenure,
                maxTenure + 1);
        }

        /// <summary>
        /// Aktualisiert die Tenure regelmäßig während der Suche.
        ///
        /// Dadurch bleibt die Tabu-Liste dynamisch.
        /// </summary>
        public void UpdateTenureIfNeeded(
            int iteration)
        {
            if (iteration > 0 &&
                iteration % updateInterval == 0)
            {
                currentTenure =
                    GenerateDynamicTenure();
            }
        }

        /// <summary>
        /// Prüft, ob ein Move tabu ist.
        ///
        /// Ein Move ist tabu, wenn sein Reverse-Key in der Tabu-Liste steht
        /// und die aktuelle Iteration noch innerhalb der Tabu-Dauer liegt.
        ///
        /// Aspiration Criterion:
        /// Falls der Move eine neue beste Lösung erzeugt,
        /// darf er trotz Tabu-Status ausgeführt werden.
        /// </summary>
        public bool IsTabu(
            Move move,
            int iteration,
            int candidateMakespan,
            int bestMakespan)
        {
            // Aspiration Criterion:
            // Eine globale Verbesserung darf nicht durch die Tabu-Liste blockiert werden.
            if (candidateMakespan < bestMakespan)
            {
                return false;
            }

            string reverseKey =
                move.GetReverseKey();

            if (!tabuUntil.ContainsKey(reverseKey))
            {
                return false;
            }

            return iteration <
                tabuUntil[reverseKey];
        }

        /// <summary>
        /// Registriert einen ausgeführten Move.
        ///
        /// Wichtig:
        /// Nicht der ausgeführte Move selbst wird tabu gesetzt,
        /// sondern sein Reverse-Move.
        ///
        /// Dadurch wird verhindert, dass die Suche direkt im nächsten Schritt
        /// wieder zur vorherigen Lösung zurückspringt.
        /// </summary>
        public void RegisterMove(
            Move move,
            int iteration)
        {
            string reverseKey =
                move.GetReverseKey();

            tabuUntil[reverseKey] =
                iteration + currentTenure;

            string moveKey =
                move.GetKey();

            if (!moveFrequency.ContainsKey(moveKey))
            {
                moveFrequency[moveKey] = 0;
            }

            moveFrequency[moveKey]++;
        }

        /// <summary>
        /// Gibt zurück, wie oft ein Move bereits ausgeführt wurde.
        ///
        /// Dieser Wert kann in der Bewertung eines Kandidaten-Moves
        /// als Frequenzstrafe verwendet werden.
        /// Häufig verwendete Moves werden dadurch weniger attraktiv.
        /// </summary>
        public int GetFrequencyPenalty(
            Move move)
        {
            string moveKey =
                move.GetKey();

            if (!moveFrequency.ContainsKey(moveKey))
            {
                return 0;
            }

            return moveFrequency[moveKey];
        }

        /// <summary>
        /// Gibt die aktuell verwendete Tenure zurück.
        /// Nützlich für Debugging oder Konsolenausgabe.
        /// </summary>
        public int CurrentTenure
        {
            get { return currentTenure; }
        }
    }
}