using System;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods;

namespace JobShopSchedulingFramework.Evaluation
{
    public sealed class TabuEvaluationVariant
    {
        /*
        Diese Klasse beschreibt eine einzelne experimentelle Variante der Tabu Search.

        Eine Variante legt nicht selbst fest, welche Instanz gelöst wird.
        Sie beschreibt nur:
        - welchen Namen die Variante hat,
        - zu welcher Evaluationskategorie sie gehört,
        - welche Tabu-Search-Parameter verwendet werden,
        - welche Nachbarschaftsdefinition verwendet wird.

        Dadurch kann der Benchmark-Runner später viele Varianten systematisch
        und vergleichbar auf denselben Instanzen ausführen.
        */

        public string VariantName { get; }

        public string Category { get; }

        public string Description { get; }

        public bool IsBaseline { get; }

        public int MultiStartCount { get; }

        private readonly Func<int, int, TabuSearchSettings> settingsFactory;

        private readonly Func<INeighborhoodDefinition> neighborhoodFactory;

        public TabuEvaluationVariant(
            string variantName,
            string category,
            string description,
            Func<int, int, TabuSearchSettings> settingsFactory,
            Func<INeighborhoodDefinition> neighborhoodFactory,
            bool isBaseline = false,
            int multiStartCount = 1)
        {
            /*
            Die Variante wird bewusst über Factory-Funktionen definiert.
            Dadurch werden Settings und Nachbarschaft nicht einmalig gespeichert,
            sondern für jeden Lauf neu erzeugt.

            Das ist wichtig für die Evaluation, weil verschiedene Läufe keine
            veränderbaren Objekte miteinander teilen sollen.
            */

            if (string.IsNullOrWhiteSpace(variantName))
            {
                throw new ArgumentException(
                    "VariantName darf nicht leer sein.",
                    nameof(variantName));
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException(
                    "Category darf nicht leer sein.",
                    nameof(category));
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException(
                    "Description darf nicht leer sein.",
                    nameof(description));
            }

            if (settingsFactory == null)
            {
                throw new ArgumentNullException(
                    nameof(settingsFactory));
            }

            if (neighborhoodFactory == null)
            {
                throw new ArgumentNullException(
                    nameof(neighborhoodFactory));
            }

            if (multiStartCount < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(multiStartCount),
                    "MultiStartCount muss mindestens 1 sein.");
            }

            VariantName =
                variantName;

            Category =
                category;

            Description =
                description;

            this.settingsFactory =
                settingsFactory;

            this.neighborhoodFactory =
                neighborhoodFactory;

            IsBaseline =
                isBaseline;

            MultiStartCount =
                multiStartCount;
        }

        public TabuSearchSettings CreateSettings(
            int maxIterations,
            int timeLimitSeconds)
        {
            /*
            Für jeden konkreten Benchmark-Lauf werden eigene Settings erzeugt.

            Beispiel:
            Dieselbe Variante kann einmal im 90s-Modus und einmal im Extended-Modus
            ausgeführt werden. Beide Läufe benötigen unterschiedliche Zeitlimits,
            sollen aber dieselbe experimentelle Logik verwenden.
            */

            TabuSearchSettings settings =
                settingsFactory(
                    maxIterations,
                    timeLimitSeconds);

            if (settings == null)
            {
                throw new InvalidOperationException(
                    "Die Variante '" +
                    VariantName +
                    "' hat keine gültigen TabuSearchSettings erzeugt.");
            }

            return settings;
        }

        public INeighborhoodDefinition CreateNeighborhood()
        {
            /*
            Die Nachbarschaft wird ebenfalls pro Lauf neu erzeugt.

            Das ist besonders wichtig für den Nachbarschaftsvergleich.
            Jede Variante soll isoliert laufen, damit der Einfluss von N1, N2,
            N3, N4 oder SetupHeavy klar vergleichbar bleibt.
            */

            INeighborhoodDefinition neighborhood =
                neighborhoodFactory();

            if (neighborhood == null)
            {
                throw new InvalidOperationException(
                    "Die Variante '" +
                    VariantName +
                    "' hat keine gültige Nachbarschaft erzeugt.");
            }

            return neighborhood;
        }

        public override string ToString()
        {
            /*
            Diese Darstellung ist hilfreich für Konsolenausgaben und Debugging.
            Sie zeigt direkt, welche Variante gerade ausgeführt wird.
            */

            return
                VariantName +
                " [" +
                Category +
                "]";
        }
    }
}