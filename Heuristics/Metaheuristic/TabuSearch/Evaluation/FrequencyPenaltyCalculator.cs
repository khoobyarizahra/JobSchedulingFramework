using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Evaluation
{
    /*
    Berechnet die Frequenzstrafe für häufig verwendete Moves.

    Die Frequency Penalty ist ein Langzeitgedächtnis der Tabu Search.
    Während die Tabu-Liste kurzfristig direkte Rückbewegungen verhindert,
    bestraft die Frequency Penalty Moves, die im Laufe der Suche sehr oft
    verwendet wurden.

    Ziel:
    Die Suche soll nicht dauerhaft dieselben Bewegungen bevorzugen, sondern
    bei Stagnation stärker diversifizieren und andere Bereiche des Suchraums
    untersuchen.

    Für Experimente kann diese Komponente über TabuSearchSettings deaktiviert
    oder über unterschiedliche Penalty-Raten angepasst werden.
    */
    public static class FrequencyPenaltyCalculator
    {
        public static double CalculatePenaltyRate(
            TabuSearchSettings settings,
            int iterationsSinceImprovement)
        {
            /*
            Wenn Frequency Penalty deaktiviert ist, wird keine Zusatzstrafe
            berechnet. Der Bewertungswert entspricht dann dem reinen Cmax.
            */
            if (!settings.UseFrequencyPenalty)
            {
                return 0.0;
            }

            /*
            Bei kurzer Stagnation wird nur schwach bestraft.
            Die Suche soll in dieser Phase noch hauptsächlich intensifizieren,
            also gute lokale Moves weiter verfolgen.
            */
            if (iterationsSinceImprovement < settings.MediumStagnationThreshold)
            {
                return settings.LowStagnationPenaltyRate;
            }

            /*
            Bei mittlerer Stagnation wird die Strafe erhöht.
            Dadurch werden häufig genutzte Moves weniger attraktiv.
            */
            if (iterationsSinceImprovement < settings.HighStagnationThreshold)
            {
                return settings.MediumStagnationPenaltyRate;
            }

            /*
            Bei langer Stagnation wird die höchste Penalty verwendet.
            Das verstärkt die Diversifikation, weil die Suche offenbar seit
            längerer Zeit keine neue globale Bestlösung gefunden hat.
            */
            return settings.HighStagnationPenaltyRate;
        }

        public static double CalculateEvaluationValue(
            int cmax,
            int moveFrequency,
            int iterationsSinceImprovement,
            TabuSearchSettings settings)
        {
            double penaltyRate =
                CalculatePenaltyRate(
                    settings,
                    iterationsSinceImprovement);

            /*
            Bewertungswert eines Moves.

            Der reine Cmax wird um eine frequenzabhängige Strafe erhöht.
            Je häufiger ein Move bereits verwendet wurde, desto stärker wird
            er bestraft.

            Formel:
            evaluation = cmax + cmax * penaltyRate * moveFrequency

            Dadurch bleibt Cmax weiterhin der wichtigste Bestandteil der Bewertung,
            aber häufig wiederholte Moves werden bei Stagnation unattraktiver.
            */
            return cmax +
                   cmax * penaltyRate * moveFrequency;
        }
    }
}