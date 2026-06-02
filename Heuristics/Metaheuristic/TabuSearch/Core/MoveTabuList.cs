using System;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /// <summary>
    /// Diese Klasse verwaltet die Tabu-Liste für die Moves im Tabu Search Algorithmus. Sie speichert, 
    /// bis zu welchem Iteration ein Move tabu ist, und wie oft jeder Move bereits ausgeführt wurde.
    /// Das Algorithmus passt die Tabu-Dauer dynamisch an, um eine bessere Balance zwischen Intensifikation und Diversifikation zu erreichen.
    /// Das Ziel ist es, die Suche vor dem Verharren in lokalen Optima zu schützen, indem kürzlich durchgeführte Moves für eine bestimmte Anzahl von Iterationen tabuisiert werden.
    public class MoveTabuList
    {
        //tabuUntil speichert für jeden Move (identifiziert durch einen String-Key) die Iteration, bis zu der dieser Move tabu ist. randomly generierte Tenure sorgt für eine dynamische Anpassung der Tabu-Dauer, um die Suche zu diversifizieren und das Risiko von Zyklen zu reduzieren.
        //moveFrequency zählt, wie oft jeder Move bereits ausgeführt wurde, was als Grundlage für eine Frequenzstrafe dienen kann, um übermäßig häufige Moves zu vermeiden.
        private readonly Dictionary<string, int> tabuUntil;
        private readonly Dictionary<string, int> moveFrequency;
        private readonly Random random;
        //baseTenure ist die durchschnittliche Tabu-Dauer, die auf der Anzahl der Jobs und Maschinen basiert. updateInterval bestimmt, wie oft die Tabu-Dauer aktualisiert wird,
        //um die Suche dynamisch zu gestalten. currentTenure speichert die aktuelle Tabu-Dauer, die für neue Moves verwendet wird.
        private readonly int baseTenure;
        //updateInterval bestimmt, wie oft die Tabu-Dauer aktualisiert wird, um die Suche dynamisch zu gestalten.
        private readonly int updateInterval;
        //currentTenure speichert die aktuelle Tabu-Dauer, die für neue Moves verwendet wird.
        private int currentTenure;
        //Der Konstruktor initialisiert die Tabu-Liste basierend auf der Anzahl der Jobs, Maschinen
        //und maximalen Iterationen. Er berechnet die Basis-Tabu-Dauer und legt fest, wie oft diese aktualisiert werden soll.
        public MoveTabuList(
            int numberOfJobs,
            int numberOfMachines,
            int maxIterations)
        {
            //tabuUntil und moveFrequency werden als leere Dictionaries initialisiert,
            //um die Tabu-Informationen zu speichern. Key ist ein String, der den Move eindeutig identifiziert (z.B. durch die beteiligten Operationen und Maschinen),
            //und Value ist die Iteration, bis zu der der Move tabu ist oder wie oft er ausgeführt wurde.
            //Ein Random-Objekt wird mit einem festen Seed (42) erstellt, um reproduzierbare Ergebnisse zu gewährleisten.
            tabuUntil = new Dictionary<string, int>();
            moveFrequency = new Dictionary<string, int>();

            random = new Random(42);
            //baseTenure wird basierend auf der Anzahl der Jobs und Maschinen berechnet, um eine angemessene Tabu-Dauer zu bestimmen. Es wird genau so berechnet:
            //Wenn die Anzahl der Jobs und Maschinen ähnlich ist (unterschied von 2 oder weniger), wird die Basis-Tabu-Dauer als die Hälfte der Summe von Jobs und Maschinen festgelegt.
            //Andernfalls wird sie als die Hälfte der Gesamtzahl der Operationen (Jobs * Maschinen) festgelegt. Dies soll sicherstellen, dass die Tabu-Dauer für schwierigere Probleme angemessen ist.
            baseTenure = CalculateBaseTenure(
                numberOfJobs,
                numberOfMachines);

            //updateInterval wird so festgelegt, dass die Tabu-Dauer alle 5% der maximalen Iterationen aktualisiert wird,
            //um eine dynamische Anpassung während der Suche zu ermöglichen.
            updateInterval = Math.Max(1, maxIterations / 20);
            //currentTenure wird so berechnet: Die aktuelle Tabu-Dauer wird durch die Methode GenerateDynamicTenure() bestimmt,
            //die eine zufällige Tabu-Dauer innerhalb eines Bereichs um die Basis-Tabu-Dauer generiert.
            currentTenure = GenerateDynamicTenure();
        }
        //BaseTenure wird basierend auf der Anzahl der Jobs und Maschinen berechnet, um eine angemessene Tabu-Dauer zu bestimmen. 
        //Als Parameter erhält die Methode die Anzahl der Jobs und Maschinen. Wenn die Anzahl der Jobs und Maschinen ähnlich ist (Unterschied von 2 oder weniger),
        //wird die Basis-Tabu-Dauer als die Hälfte der Summe von Jobs und Maschinen festgelegt. Andernfalls wird sie als die Hälfte der Gesamtzahl der Operationen (Jobs * Maschinen) festgelegt.
        //Dies soll sicherstellen, dass die Tabu-Dauer für schwierigere Probleme angemessen ist.
        private int CalculateBaseTenure(
            int numberOfJobs,
            int numberOfMachines)
        {
            //Ein Problem gilt als "schwierig", wenn die Anzahl der Jobs und Maschinen ähnlich ist (Unterschied von 2 oder weniger).
            //In diesem Fall wird die Basis-Tabu-Dauer als die Hälfte der Summe von Jobs und Maschinen festgelegt. 
            //wir machen hier das Argument, dass wenn die Anzahl der Jobs und Maschinen ähnlich ist, das Problem tendenziell komplexer sein könnte,
            //da es mehr Möglichkeiten für Konflikte und Engpässe gibt.
            bool hardProblem =
                Math.Abs(numberOfJobs - numberOfMachines) <= 2;

            if (hardProblem)
            {
                return Math.Max(1, (numberOfJobs + numberOfMachines) / 2);
            }
            //Für weniger schwierige Probleme wird die Basis-Tabu-Dauer als die Hälfte der Gesamtzahl der Operationen (Jobs * Maschinen) festgelegt.
            //Weil die Anzahl der Operationen ein guter Indikator für die Komplexität des Problems ist, soll dies sicherstellen, dass die Tabu-Dauer für schwierigere Probleme angemessen ist.
            int numberOfOperations =
                numberOfJobs * numberOfMachines;

            return Math.Max(1, numberOfOperations / 2);
        }
        //diese Methode generiert eine dynamische Tabu-Dauer, die um die Basis-Tabu-Dauer herum variiert.
        //Sie berechnet einen Mindestwert (80% der Basis-Tabu-Dauer) und einen Höchstwert (120% der Basis-Tabu-Dauer) und generiert dann eine zufällige Tabu-Dauer innerhalb dieses Bereichs.
        //Dies ermöglicht eine gewisse Flexibilität in der Tabu-Dauer, um die Suche zu diversifizieren und das Risiko von Zyklen zu reduzieren.
        private int GenerateDynamicTenure()
        {
            int minTenure =
                Math.Max(1, (int)Math.Round(0.8 * baseTenure));

            int maxTenure =
                Math.Max(minTenure + 1, (int)Math.Round(1.2 * baseTenure));

            return random.Next(minTenure, maxTenure + 1);
        }
        //Diese Methode aktualisiert die aktuelle Tabu-Dauer, wenn die Anzahl der Iterationen ein Vielfaches des updateInterval erreicht.
        //Das bedeutet, dass die Tabu-Dauer alle 5% der maximalen Iterationen aktualisiert wird, um eine dynamische Anpassung während der Suche zu ermöglichen.
        public void UpdateTenureIfNeeded(int iteration)
        {
            if (iteration > 0 && iteration % updateInterval == 0)
            {
                currentTenure = GenerateDynamicTenure();
            }
        }
        //Diese Methode überprüft, ob ein gegebener Move tabu ist.
        //Ein Move ist tabu, wenn er in der Tabu-Liste enthalten ist und die aktuelle Iteration kleiner ist als die Iteration, bis zu der der Move tabu ist.
        public bool IsTabu(
            Move move,
            int iteration,
            int candidateMakespan,
            int bestMakespan)
        {
            if (candidateMakespan < bestMakespan)
            {
                return false;
            }

            string reverseKey = move.GetReverseKey();

            if (!tabuUntil.ContainsKey(reverseKey))
            {
                return false;
            }

            return iteration < tabuUntil[reverseKey];
        }
        //Diese Methode registriert einen Move als tabu, indem sie die Iteration speichert, bis zu der der Move tabu ist.
        //Der Move wird durch seinen Reverse-Key identifiziert, da der Tabu-Status für die Umkehrung des Moves gilt
        //(z.B. wenn ein Move das Verschieben einer Operation von Maschine A zu Maschine B ist, wäre der Reverse-Move das Verschieben derselben Operation zurück von Maschine B zu Maschine A).
        public void RegisterMove(
            Move move,
            int iteration)
        {
            string reverseKey = move.GetReverseKey();

            tabuUntil[reverseKey] =
                iteration + currentTenure;

            string moveKey = move.GetKey();

            if (!moveFrequency.ContainsKey(moveKey))
            {
                moveFrequency[moveKey] = 0;
            }

            moveFrequency[moveKey]++;
        }
        //Diese Methode gibt die Frequenzstrafe für einen gegebenen Move zurück, basierend darauf, wie oft dieser Move bereits ausgeführt wurde.
        //Die Methode funktioniert genau so: Sie erhält einen Move als Parameter und
        //extrahiert den Key des Moves. Wenn der Move-Key nicht in der moveFrequency-Dictionary enthalten ist, wird eine Frequenzstrafe von 0 zurückgegeben.
        public int GetFrequencyPenalty(Move move)
        {
            string moveKey = move.GetKey();
            //Wenn der Move-Key nicht in der moveFrequency-Dictionary enthalten ist, wird eine Frequenzstrafe von 0 zurückgegeben.
            //Andernfalls wird die Anzahl der Ausführungen dieses Moves zurückgegeben.

            if (!moveFrequency.ContainsKey(moveKey))
            {
                return 0;
            }

            return moveFrequency[moveKey];
        }

        public int CurrentTenure
        {
            get { return currentTenure; }
        }
    }
}