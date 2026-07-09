using System;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /*
    Verwaltet die Tabu-Liste der Tabu Search.

    Wenn ein Move ausgeführt wird, wird der direkte Rückmove für eine bestimmte
    Anzahl an Iterationen tabu gesetzt. Dadurch verhindert die Suche, dass sie
    sofort wieder zur vorherigen Lösung zurückspringt.

    Zusätzlich zählt diese Klasse, wie häufig Moves verwendet wurden. Diese
    Information wird später für die Frequency Penalty genutzt. Damit erhält die
    Tabu Search neben dem kurzfristigen Gedächtnis der Tabu-Liste auch ein
    einfaches Langzeitgedächtnis zur Diversifikation.

    Refactoring-Hinweis:
    Die Berechnung der Tabu-Dauer verwendet jetzt TabuSearchSettings. Dadurch
    können verschiedene Tenure-Parameter getestet werden, ohne die Logik dieser
    Klasse direkt ändern zu müssen.
    */
    public class MoveTabuList
    {
        private readonly Dictionary<string, int> tabuUntil;
        private readonly Dictionary<string, int> moveFrequency;
        private readonly Random random;
        private readonly TabuSearchSettings settings;

        private readonly int baseTenure;
        private readonly int minTenure;
        private readonly int maxTenure;
        private readonly int updateInterval;

        private int currentTenure;

        public MoveTabuList(
            int numberOfJobs,
            int numberOfMachines,
            int maxIterations)
            : this(
                  numberOfJobs,
                  numberOfMachines,
                  maxIterations,
                  TabuSearchSettings.CreateDefault(
                      maxIterations,
                      0))
        {
            /*
            Dieser Konstruktor bleibt erhalten, damit bestehender Code weiter
            funktioniert. Intern wird eine Standardkonfiguration verwendet.
            Dadurch bleibt das Verhalten zunächst kompatibel zum bisherigen Solver.
            */
        }

        public MoveTabuList(
            int numberOfJobs,
            int numberOfMachines,
            int maxIterations,
            TabuSearchSettings settings)
        {
            this.settings = settings;

            tabuUntil =
                new Dictionary<string, int>();

            moveFrequency =
                new Dictionary<string, int>();

            /*
            Der feste Seed macht die dynamische Tabu-Dauer reproduzierbar.
            Das ist für faire Experimente wichtig.
            */
            random =
                new Random(settings.TabuRandomSeed);

            baseTenure =
                CalculateBaseTenure(
                    numberOfJobs,
                    numberOfMachines);

            minTenure =
                Math.Max(
                    1,
                    (int)Math.Round(baseTenure * settings.MinTenureFactor));

            maxTenure =
                Math.Max(
                    minTenure + 1,
                    (int)Math.Round(baseTenure * settings.MaxTenureFactor));

            /*
            Das Update-Intervall hängt bewusst nicht von maxIterations ab.

            Grund:
            Zwei Läufe mit identischer Instanz, aber unterschiedlichem Zeit- oder
            Iterationslimit, sollen nicht automatisch eine komplett andere Tenure-
            Entwicklung bekommen. Deshalb wird das Intervall aus der geschätzten
            Problemgröße berechnet.
            */
            updateInterval =
                CalculateTenureUpdateInterval(
                    numberOfJobs,
                    numberOfMachines);

            currentTenure =
                GenerateDynamicTenure();
        }

        private int CalculateBaseTenure(
            int numberOfJobs,
            int numberOfMachines)
        {
            /*
            Die Basis-Tenure wird abhängig von der Problemgröße berechnet.

            Größere Instanzen haben meist einen größeren Suchraum. Eine etwas
            längere Tabu-Dauer kann dort helfen, kurze Zyklen zu vermeiden.
            */
            int problemSize =
                numberOfJobs + numberOfMachines;

            return Math.Max(
                settings.MinBaseTenure,
                problemSize / 2);
        }

        private int CalculateTenureUpdateInterval(
            int numberOfJobs,
            int numberOfMachines)
        {
            /*
            Die Tabu-Dauer wird nicht in jeder Iteration geändert, sondern nur
            nach einem festen Intervall. Dadurch bleibt die Suche stabil, kann
            aber langfristig trotzdem zwischen kürzerer und längerer Tenure
            variieren.
            */
            int estimatedOperationCount =
                numberOfJobs * numberOfMachines;

            return Math.Max(
                settings.MinTenureUpdateInterval,
                estimatedOperationCount * settings.TenureUpdateOperationFactor);
        }

        private int GenerateDynamicTenure()
        {
            /*
            Die aktuelle Tabu-Dauer wird zufällig zwischen minTenure und maxTenure
            gewählt. Dadurch ist die Tabu-Liste nicht vollständig statisch und kann
            sich während der Suche leicht anpassen.
            */
            return random.Next(
                minTenure,
                maxTenure + 1);
        }

        public void UpdateTenureIfNeeded(
            int iteration)
        {
            /*
            Nach jedem Update-Intervall wird eine neue dynamische Tabu-Dauer
            erzeugt. Das unterstützt Diversifikation, ohne die Suche in jeder
            Iteration zufällig zu stark zu verändern.
            */
            if (iteration > 0 &&
                iteration % updateInterval == 0)
            {
                currentTenure =
                    GenerateDynamicTenure();
            }
        }

        public bool IsTabu(
            Move move,
            int iteration,
            int candidateMakespan,
            int bestMakespan,
            int currentCmax)
        {
            /*
            Aspiration:
            Ein tabu Move darf trotzdem angenommen werden, wenn er eine neue globale
            Bestlösung erzeugt. Dadurch verhindert die Tabu-Liste keine klare
            Verbesserung.
            */
            if (settings.UseAspiration &&
                candidateMakespan < bestMakespan)
            {
                return false;
            }

            string moveKey =
                move.GetKey();

            /*
            Wenn der Move nicht in der Tabu-Liste steht, ist er erlaubt.
            */
            if (!tabuUntil.ContainsKey(moveKey))
            {
                return false;
            }

            /*
            Ein Move ist tabu, solange die aktuelle Iteration kleiner ist als die
            gespeicherte Ablaufiteration.
            */
            return iteration <
                   tabuUntil[moveKey];
        }

        public void RegisterMove(
            Move move,
            int iteration)
        {
            /*
            Nach Ausführung eines Moves wird nicht der Move selbst tabu gesetzt,
            sondern sein direkter Rückmove. So wird verhindert, dass die Suche
            sofort zur vorherigen Maschinenreihenfolge zurückkehrt.
            */
            string reverseKey =
                move.GetReverseKey();

            tabuUntil[reverseKey] =
                iteration + currentTenure;

            /*
            Zusätzlich wird gezählt, wie häufig der ausgeführte Move verwendet wurde.
            Diese Häufigkeit kann später bei der Frequency Penalty berücksichtigt
            werden.
            */
            string moveKey =
                move.GetKey();

            if (!moveFrequency.ContainsKey(moveKey))
            {
                moveFrequency[moveKey] =
                    0;
            }

            moveFrequency[moveKey]++;
        }

        public int GetFrequencyPenalty(
            Move move)
        {
            /*
            Gibt zurück, wie oft ein Move bereits ausgeführt wurde.

            Der Name der Methode bleibt aus Kompatibilitätsgründen erhalten.
            Fachlich handelt es sich hier nicht direkt um die Penalty selbst,
            sondern um die Häufigkeit, aus der später eine Penalty berechnet wird.
            */
            string moveKey =
                move.GetKey();

            if (!moveFrequency.ContainsKey(moveKey))
            {
                return 0;
            }

            return moveFrequency[moveKey];
        }

        public void ClearShortTermMemory()
        {
            /*
            Löscht nur die kurzfristige Tabu-Liste.

            Die Move-Frequenzen bleiben erhalten, weil sie als Langzeitgedächtnis
            der Suche dienen. Dadurch bleibt die Diversifikationsinformation auch
            nach einem Restart verfügbar.
            */
            tabuUntil.Clear();
        }

        public int CurrentTenure
        {
            get { return currentTenure; }
        }

        public int BaseTenure
        {
            get { return baseTenure; }
        }

        public int MinTenure
        {
            get { return minTenure; }
        }

        public int MaxTenure
        {
            get { return maxTenure; }
        }

        public int UpdateInterval
        {
            get { return updateInterval; }
        }
    }
}