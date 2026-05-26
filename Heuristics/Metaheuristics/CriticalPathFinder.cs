// Klasse zur Berechnung des kritischen Pfades
// basierend auf dem Graph-Ansatz aus dem Paper
using JobShopSchedulingFramework.Heuristics.Metaheuristics;
using JobShopSchedulingFramework.Models;

public static class CriticalPathFinder
{
    // Repräsentiert eine Kante im Graphen
    // eine Kante verbindet zwei Operationen
    //
    // from  = Vorgängeroperation
    // to    = Nachfolgeroperation
    //
    // setup:
    // Setupzeit zwischen den beiden Operationen
    // nur relevant bei Maschinen-Kanten
    private class Edge
    {
        public Operation from;
        public Operation to;
        public int setup;

        public Edge(
            Operation from,
            Operation to,
            int setup)
        {
            this.from = from;
            this.to = to;
            this.setup = setup;
        }
    }

    // Hauptmethode:
    // berechnet den kritischen Pfad
    //
    // Übergabe:
    // - Instanz
    // - aktuelle Maschinenreihenfolge
    public static List<Operation> FindCriticalPath(
        Instance instance,
        Dictionary<int, List<Operation>> machineOrders)
    {
        // Alle Operationen aller Jobs sammeln
        List<Operation> allOperations =
            instance.jobs
            .SelectMany(job => job.operations)
            .ToList();

        // Nachfolgergraph erzeugen
        //
        // successors[operation]
        // enthält alle Nachfolgerkanten
        Dictionary<Operation, List<Edge>> successors =
            CreateSuccessorGraph(
                instance,
                machineOrders,
                allOperations);

        // Vorgängergraph erzeugen
        //
        // einfach inverse Richtung
        Dictionary<Operation, List<Edge>> predecessors =
            CreatePredecessorGraph(
                allOperations,
                successors);

        // Topologische Reihenfolge erzeugen
        //
        // Jede Operation erscheint erst
        // nach ihren Vorgängern.
        List<Operation> topologicalOrder =
            CreateTopologicalOrder(
                allOperations,
                predecessors,
                successors);

        // Sicherheitsprüfung:
        // Wenn nicht alle Operationen enthalten sind,
        // existiert ein Zyklus.
        if (topologicalOrder.Count != allOperations.Count)
        {
            Console.WriteLine(
                "No critical path found. Machine order may contain a cycle.");

            return new List<Operation>();
        }

        // Vorwärtsrechnung:
        // r_i berechnen
        //
        // r_i =
        // frühestmöglicher Startzeitpunkt
        Dictionary<Operation, int> r =
            CalculateReleaseDates(
                topologicalOrder,
                predecessors);

        // Rückwärtsrechnung:
        // q_i berechnen
        //
        // q_i =
        // längster Restpfad bis zum Ende
        Dictionary<Operation, int> q =
            CalculateTails(
                topologicalOrder,
                successors);

        // Makespan bestimmen
        //
        // r_i + q_i entspricht
        // der Gesamtlänge des Pfades über diese Operation
        int cmax =
            allOperations.Max(
                op => r[op] + q[op]);

        // Kritische Operationen bestimmen
        //
        // Eine Operation ist kritisch,
        // wenn:
        //
        // r_i + q_i == Cmax
        List<Operation> criticalOperations =
            allOperations
            .Where(
                op => r[op] + q[op] == cmax)
            .OrderBy(
                op => r[op])
            .ToList();

        // Sicherheitsprüfung
        if (criticalOperations.Count == 0)
        {
            Console.WriteLine(
                "No critical operations found.");

            return new List<Operation>();
        }

        // Einen konkreten kritischen Pfad erzeugen
        //
        // Die kritischen Operationen müssen
        // korrekt verbunden werden.
        List<Operation> criticalPath =
            BuildOneCriticalPath(
                criticalOperations,
                successors,
                r,
                q,
                cmax);

        // Sicherheitsprüfung
        if (criticalPath == null
            || criticalPath.Count == 0)
        {
            Console.WriteLine(
                "No critical path found.");

            return new List<Operation>();
        }

        return criticalPath;
    }

    // Erstellt den Nachfolgergraphen
    //
    // Der Graph enthält:
    // - Job-Kanten
    // - Maschinen-Kanten
    private static Dictionary<Operation, List<Edge>>
        CreateSuccessorGraph(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders,
            List<Operation> allOperations)
    {
        Dictionary<Operation, List<Edge>> successors =
            new Dictionary<Operation, List<Edge>>();

        // Für jede Operation Liste erzeugen
        foreach (Operation operation in allOperations)
        {
            successors[operation] =
                new List<Edge>();
        }

        // -------------------------------------------------
        // JOB-KANTEN
        // -------------------------------------------------
        //
        // Beispiel:
        //
        // O11 -> O12 -> O13
        //
        // Jede Operation eines Jobs
        // zeigt auf die nächste.
        foreach (Job job in instance.jobs)
        {
            for (int i = 0;
                i < job.operations.Count - 1;
                i++)
            {
                Operation before =
                    job.operations[i];

                Operation after =
                    job.operations[i + 1];

                successors[before].Add(
                    new Edge(
                        before,
                        after,
                        0));
            }
        }

        // -------------------------------------------------
        // MASCHINEN-KANTEN
        // -------------------------------------------------
        //
        // Die Reihenfolge stammt
        // aus machineOrders.
        //
        // Beispiel:
        //
        // Maschine 2:
        // O31 -> O12 -> O23
        //
        // erzeugt:
        //
        // O31 -> O12
        // O12 -> O23
        foreach (var pair in machineOrders)
        {
            List<Operation> operationsOnMachine =
                pair.Value;

            for (int i = 0;
                i < operationsOnMachine.Count - 1;
                i++)
            {
                Operation before =
                    operationsOnMachine[i];

                Operation after =
                    operationsOnMachine[i + 1];

                // Setupzeit zwischen den Jobs
                int setup =
                    instance.setupTimes[
                        before.jobID - 1,
                        after.jobID - 1];

                successors[before].Add(
                    new Edge(
                        before,
                        after,
                        setup));
            }
        }

        return successors;
    }

    // Erstellt Vorgängergraphen
    //
    // Dreht alle Kanten um.
    private static Dictionary<Operation, List<Edge>>
        CreatePredecessorGraph(
            List<Operation> allOperations,
            Dictionary<Operation, List<Edge>> successors)
    {
        Dictionary<Operation, List<Edge>> predecessors =
            new Dictionary<Operation, List<Edge>>();

        foreach (Operation operation in allOperations)
        {
            predecessors[operation] =
                new List<Edge>();
        }

        // Alle Kanten umdrehen
        foreach (var pair in successors)
        {
            foreach (Edge edge in pair.Value)
            {
                predecessors[edge.to]
                    .Add(edge);
            }
        }

        return predecessors;
    }

    // Topologische Sortierung
    //
    // Erzeugt eine Reihenfolge,
    // in der Vorgänger immer
    // zuerst kommen.
    private static List<Operation>
        CreateTopologicalOrder(
            List<Operation> allOperations,
            Dictionary<Operation, List<Edge>> predecessors,
            Dictionary<Operation, List<Edge>> successors)
    {
        // Anzahl offener Vorgänger
        Dictionary<Operation, int>
            remainingPredecessors =
            allOperations.ToDictionary(
                op => op,
                op => predecessors[op].Count);

        // Startoperationen:
        // keine Vorgänger
        Queue<Operation> readyOperations =
            new Queue<Operation>(
                allOperations.Where(
                    op =>
                        remainingPredecessors[op] == 0));

        List<Operation> order =
            new List<Operation>();

        while (readyOperations.Count > 0)
        {
            Operation current =
                readyOperations.Dequeue();

            order.Add(current);

            // Nachfolger bearbeiten
            foreach (Edge edge in successors[current])
            {
                remainingPredecessors[edge.to]--;

                // Wenn keine offenen Vorgänger mehr:
                // Operation freigeben
                if (remainingPredecessors[edge.to] == 0)
                {
                    readyOperations.Enqueue(
                        edge.to);
                }
            }
        }

        return order;
    }

    // -------------------------------------------------
    // VORWÄRTSRECHNUNG
    // -------------------------------------------------
    //
    // Berechnet r_i
    //
    // r_i =
    // frühestmöglicher Startzeitpunkt
    private static Dictionary<Operation, int>
        CalculateReleaseDates(
            List<Operation> topologicalOrder,
            Dictionary<Operation, List<Edge>> predecessors)
    {
        // Dictionary für r_i erzeugen
        Dictionary<Operation, int> r =
            topologicalOrder.ToDictionary(
                op => op,
                op => 0);

        // Operationen in topologischer Reihenfolge
        // durchlaufen
        foreach (Operation operation in topologicalOrder)
        {
            // maximalen Vorgänger-Endzeitpunkt speichern
            int maxValue = 0;

            // Alle Vorgänger betrachten
            foreach (Edge edge in predecessors[operation])
            {
                // Kandidat berechnen:
                //
                // r[vorgänger]
                // + Bearbeitungszeit
                // + Setupzeit
                int candidate =
                    r[edge.from]
                    + edge.from.processingTime
                    + edge.setup;

                // Maximum bestimmen
                maxValue =
                    Math.Max(
                        maxValue,
                        candidate);
            }

            // Frühesten Start speichern
            r[operation] = maxValue;
        }

        return r;
    }

    // -------------------------------------------------
    // RÜCKWÄRTSRECHNUNG
    // -------------------------------------------------
    //
    // Berechnet q_i
    //
    // q_i =
    // längster Restpfad bis Projektende
    private static Dictionary<Operation, int>
        CalculateTails(
            List<Operation> topologicalOrder,
            Dictionary<Operation, List<Edge>> successors)
    {
        // q_i initialisieren
        //
        // mindestens eigene Bearbeitungszeit
        Dictionary<Operation, int> q =
            topologicalOrder.ToDictionary(
                op => op,
                op => op.processingTime);

        // Rückwärts durch die Reihenfolge laufen
        for (int i = topologicalOrder.Count - 1;
            i >= 0;
            i--)
        {
            Operation operation =
                topologicalOrder[i];

            int maxSuccessorTail = 0;

            // Nachfolger betrachten
            foreach (Edge edge in successors[operation])
            {
                // Kandidat berechnen:
                //
                // Setupzeit
                // + q[nachfolger]
                int candidate =
                    edge.setup
                    + q[edge.to];

                maxSuccessorTail =
                    Math.Max(
                        maxSuccessorTail,
                        candidate);
            }

            // q_i berechnen
            q[operation] =
                operation.processingTime
                + maxSuccessorTail;
        }

        return q;
    }

    // Baut einen konkreten kritischen Pfad
    //
    // Verbindet kritische Operationen
    // korrekt entlang der Kanten.
    private static List<Operation>
        BuildOneCriticalPath(
            List<Operation> criticalOperations,
            Dictionary<Operation, List<Edge>> successors,
            Dictionary<Operation, int> r,
            Dictionary<Operation, int> q,
            int cmax)
    {
        // Erste kritische Operation
        Operation current =
            criticalOperations
            .OrderBy(op => r[op])
            .First();

        List<Operation> path =
            new List<Operation>();

        path.Add(current);

        while (true)
        {
            // Kritischen Nachfolger suchen
            Edge nextEdge =
                successors[current]
                .Where(edge =>

                    // Nachfolger ebenfalls kritisch
                    r[edge.to]
                    + q[edge.to]
                    == cmax

                    &&

                    // zeitlich direkt verbunden
                    r[edge.to]
                    ==
                    r[current]
                    + current.processingTime
                    + edge.setup)

                .OrderBy(edge => r[edge.to])
                .FirstOrDefault();

            // Kein Nachfolger mehr:
            // Ende erreicht
            if (nextEdge == null)
                break;

            current = nextEdge.to;

            path.Add(current);
        }

        return path;
    }

    // Extrahiert kritische Blöcke
    //
    // Kritische Blöcke:
    // mehrere kritische Operationen
    // direkt hintereinander
    // auf derselben Maschine
    public static List<CriticalBlock>
        ExtractCriticalBlocks(
            List<Operation> criticalPath)
    {
        List<CriticalBlock> blocks =
            new List<CriticalBlock>();

        if (criticalPath.Count == 0)
            return blocks;

        // Ersten Block starten
        CriticalBlock currentBlock =
            new CriticalBlock(
                criticalPath[0].machine);

        currentBlock.operations
            .Add(criticalPath[0]);

        // Durch kritischen Pfad laufen
        for (int i = 1;
            i < criticalPath.Count;
            i++)
        {
            Operation operation =
                criticalPath[i];

            // gleiche Maschine:
            // zum aktuellen Block hinzufügen
            if (operation.machine
                ==
                currentBlock.machine)
            {
                currentBlock.operations
                    .Add(operation);
            }
            else
            {
                // nur echte Blöcke speichern
                if (currentBlock.operations.Count >= 2)
                {
                    blocks.Add(currentBlock);
                }

                // neuen Block starten
                currentBlock =
                    new CriticalBlock(
                        operation.machine);

                currentBlock.operations
                    .Add(operation);
            }
        }

        // letzten Block speichern
        if (currentBlock.operations.Count >= 2)
        {
            blocks.Add(currentBlock);
        }

        return blocks;
    }
}