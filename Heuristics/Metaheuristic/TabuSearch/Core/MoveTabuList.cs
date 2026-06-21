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
    /// Die Tabu-Dauer wird abhängig von der Größe der Instanz bestimmt
    /// und während der Suche regelmäßig angepasst.
    ///
    /// Zusätzlich wird gezählt, wie häufig ein Move bereits verwendet wurde.
    /// Diese Information kann später für eine Frequenzstrafe genutzt werden,
    /// um die Diversifikation der Suche weiter zu erhöhen.
    /// </summary>
    public class MoveTabuList
    {
        // Speichert für jeden tabuisierten Move die Iteration,
        // bis zu der dieser tabu bleibt.
        private readonly Dictionary<string, int> tabuUntil;

        // Speichert, wie oft ein Move bereits ausgeführt wurde.
        private readonly Dictionary<string, int> moveFrequency;
        private readonly Random random;

        // Basiswert der Tabu-Dauer.
        private readonly int baseTenure;

        // Untere Grenze der dynamischen Tenure.
        private readonly int minTenure;

        // Obere Grenze der dynamischen Tenure.
        private readonly int maxTenure;

        // Gibt an, nach wie vielen Iterationen die Tenure
        // neu erzeugt wird.
        private readonly int updateInterval;

        // Aktuell verwendete Tabu-Dauer.
        private int currentTenure;

        /// <summary>
        /// Erstellt eine neue Tabu-Liste.
        ///
        /// Die Basis-Tenure wird aus der Größe der Instanz berechnet.
        /// Anschließend wird daraus eine dynamische Tenure erzeugt,
        /// die während der Suche regelmäßig aktualisiert wird.
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

            // Fester Seed für reproduzierbare Ergebnisse.
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

            // Alle 5 % der maximalen Iterationen wird
            // eine neue Tenure erzeugt.
            updateInterval =
                Math.Max(
                    1,
                    maxIterations / 20);

            // Erste dynamische Tenure erzeugen.
            currentTenure =
                GenerateDynamicTenure();
        }

        /// <summary>
        /// Berechnet die Basis-Tenure abhängig von der Größe der Instanz.
        ///
        /// Die Tenure wächst mit der Problemgröße,
        /// damit größere Instanzen eine stärkere Diversifikation erhalten.
        ///
        /// Als Maß für die Problemgröße wird die Summe aus
        /// Jobs und Maschinen verwendet.
        ///
        /// Dadurch bleibt die Tenure moderat und verhindert,
        /// dass zu viele Moves gleichzeitig tabu werden.
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
        /// Die Tenure wird innerhalb eines Bereichs um die Basis-Tenure gewählt.
        /// Dadurch bleibt die Suche flexibler als bei einer festen Tabu-Dauer.
        /// </summary>
        private int GenerateDynamicTenure()
        {
            return random.Next(
                minTenure,
                maxTenure + 1);
        }

        /// <summary>
        /// Aktualisiert die aktuelle Tenure während der Suche.
        ///
        /// Dadurch kann die Stärke der Tabu-Restriktionen
        /// im Verlauf der Suche variieren.
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
        /// Das Aspiration Criterion erlaubt tabuierte Moves,
        /// wenn dadurch eine neue globale Bestlösung entsteht.
        /// </summary>
        public bool IsTabu(
            Move move,
            int iteration,
            int candidateMakespan,
            int bestMakespan,
            int currentCmax)
        {
            // Aspiration Criterion:
            // Verbesserungen dürfen trotz Tabu ausgeführt werden.
           if (candidateMakespan < bestMakespan)
            {
                return false;
            }

          //TEST
          /*  const int aspirationThreshold = 3;

            if (candidateMakespan <= bestMakespan + aspirationThreshold)
            {
                return false;
            }
            //TEST
            */
            //TEST 2
            /*if (candidateMakespan < currentCmax)
            {
                return false;
            }*/
            //TEST"

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
        /// Gespeichert wird der Reverse-Move,
        /// damit die Suche nicht unmittelbar zur vorherigen
        /// Lösung zurückkehren kann.
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
        /// Dieser Wert kann später als Frequenzstrafe verwendet werden,
        /// um häufig verwendete Moves weniger attraktiv zu machen.
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
        /// Gibt die aktuell verwendete dynamische Tenure zurück.
        /// </summary>
        public int CurrentTenure
        {
            get { return currentTenure; }
        }
    }
}