# Job Shop Scheduling Framework

Dieses Projekt wurde im Rahmen des Moduls **Scheduling** entwickelt. Ziel des Projekts ist die Lösung eines **Job-Shop-Scheduling-Problems mit Rüstzeiten**. Dafür werden zunächst verschiedene Eröffnungsheuristiken verglichen. Anschließend wird die beste Startlösung mit einer **Tabu Search** weiter verbessert. Zusätzlich wird ein **CP-SAT-Solver** als Referenzvergleich verwendet.

Die finale Abgabeversion befindet sich im Branch `main`. Der Branch `zahra-final-benchmark` dokumentiert die experimentelle Entwicklung und die Benchmark-Verbesserungen.

---

## Projektziel

Das Ziel des Projekts besteht darin, für gegebene Job-Shop-Instanzen einen möglichst guten Ablaufplan zu erzeugen. Bewertet wird der Schedule über den Makespan `Cmax`.

Ein niedrigerer `Cmax` bedeutet, dass alle Jobs früher abgeschlossen werden.

Das Projekt berücksichtigt:

- mehrere Jobs mit mehreren Operationen,
- Maschinenzuordnungen,
- Bearbeitungszeiten,
- Rüstzeiten zwischen Operationen,
- kritische Operationen und kritische Blöcke,
- Verbesserung der Startlösung durch Tabu Search,
- Vergleich mit einem CP-SAT-Referenzsolver.

---

## Implementierte Methoden

### 1. Eröffnungsheuristiken

Für jede Instanz werden mehrere Dispatching-Regeln getestet. Die beste daraus erzeugte Lösung dient anschließend als Startlösung für die Tabu Search.

Implementierte Regeln:

- `SPT` – Shortest Processing Time: Es wird die verfügbare Operation mit der kürzesten Bearbeitungszeit bevorzugt.
- `LPT` – Longest Processing Time: Es wird die verfügbare Operation mit der längsten Bearbeitungszeit bevorzugt.
- `SRPT` – Shortest Remaining Processing Time: Es wird der Job bevorzugt, dessen verbleibende Gesamtbearbeitungszeit am kleinsten ist.
- `LRPT` – Longest Remaining Processing Time: Es wird der Job bevorzugt, dessen verbleibende Gesamtbearbeitungszeit am größten ist.
- `Random` – Es wird zufällig eine der aktuell verfügbaren Operationen ausgewählt. Diese Regel dient vor allem als Vergleichs- und Diversifikationsstrategie.
- `SetupAwareLRPT` – Erweiterung von `LRPT`, bei der neben der verbleibenden Bearbeitungszeit auch Rüstzeiten berücksichtigt werden. 
Dadurch werden Jobs mit hoher verbleibender Bearbeitungszeit bevorzugt, ohne die Rüstzeitstruktur vollständig zu ignorieren.

Die Auswahl mehrerer Regeln ist wichtig, da keine einzelne Regel auf allen Benchmarkinstanzen eindeutig dominiert.

---

### 2. Tabu Search

Die finale Tabu Search verbessert die beste Initiallösung iterativ. Dabei wird die Maschinenreihenfolge des Schedules verändert und nach jedem Move der resultierende Makespan bewertet.

Die finale Konfiguration verwendet:

- kritische Operationen,
- kritische Blöcke,
- Nachbarschaft `N3 - All Pair Critical Block Swaps`,
- Tabu-Liste zur Vermeidung direkter Rückmoves,
- Aspiration-Kriterium,
- Frequency Penalty,
- Restart-Strategie bei Stagnation,
- adaptive exakte Move-Bewertung.

---

### 3. Finale Nachbarschaft

Die finale Lösung verwendet die Nachbarschaft:

N3 - All Pair Critical Block Swaps

Dabei werden innerhalb kritischer Blöcke paarweise Operationen auf derselben Maschine vertauscht. Diese Nachbarschaft konzentriert die Suche auf besonders relevante Bereiche des Schedules, da Änderungen an kritischen Blöcken direkt Einfluss auf den Makespan haben können.

---

### 4. Adaptive exakte Move-Bewertung

In der ursprünglichen Baseline wurden nach einer schnellen Move-Abschätzung maximal 20 Move-Kandidaten exakt bewertet. Die Evaluation zeigte jedoch, dass diese feste Grenze teilweise zu restriktiv ist.

Daher wurde eine adaptive Strategie implementiert:

- Die schnelle Move-Abschätzung bleibt als Vorauswahl aktiv.
- Bei wenigen generierten Moves werden alle Kandidaten exakt bewertet.
- Bei größeren Nachbarschaften wird eine Mindestanzahl exakt geprüft.
- Bei längerer Stagnation wird die Anzahl exakt geprüfter Kandidaten erhöht.
- Eine Obergrenze verhindert zu hohe Rechenkosten pro Iteration.

Die finale adaptive Strategie bewegt sich zwischen 50 und 200 exakt bewerteten Kandidaten pro Iteration.

---

## CP-SAT-Referenzsolver

Zusätzlich zur heuristischen Lösung wird ein CP-SAT-Solver verwendet. Dieser dient als Referenz zur Bewertung der Tabu-Search-Ergebnisse.

Der CP-SAT-Solver liefert je nach Instanzstatus:

- `Optimal`
- `Feasible`
- oder keinen gültigen Schedule innerhalb des Zeitlimits.

Der Vergleich mit CP-SAT wird verwendet, um den Abstand der heuristischen Lösung zur Referenzlösung zu bewerten.

---

## Programm starten

Das Projekt kann über die Konsole gestartet werden:

```bash
dotnet run
```

Danach erscheint ein Auswahlmenü.

---

## Hauptmenü

Nach dem Start stehen folgende Modi zur Verfügung:


1 - Interactive application
2 - Full benchmark evaluation
3 - CP benchmark only
4 - Experimental evaluation screening
5 - Move selection evaluation
6 - Adaptive exact evaluation


---

## Modus 1: Interactive application

Dieser Modus erlaubt das interaktive Lösen einzelner Instanzen. Dabei kann zwischen generierten Instanzen und Benchmarkinstanzen gewählt werden.

Typischer Ablauf:

1. Benchmarkinstanz auswählen
2. Laufmodus wählen
3. Initialheuristiken werden verglichen
4. Tabu Search wird ausgeführt
5. CP-SAT-Solver wird als Referenz gestartet
6. CSV-Ergebnisse und Gantt-Charts werden erzeugt

---

## Modus 2: Full benchmark evaluation

Dieser Modus führt die finale Team-F-Tabu-Search-Konfiguration auf allen Benchmarkinstanzen aus.

Dabei werden pro Instanz zwei Läufe durchgeführt:

- 90-Sekunden-Lauf
- Extended-Lauf ohne festes 90-Sekunden-Zeitlimit

Die Ergebnisse werden gespeichert unter: Results/Csv/TeamF_BenchmarkResults.csv


---

## Modus 3: CP benchmark only

Dieser Modus führt nur den CP-SAT-Referenzsolver auf den Benchmarkinstanzen aus.

---

## Modus 4: Experimental evaluation screening

Dieser Modus führt die Screening-Evaluation verschiedener Tabu-Search-Varianten durch.

Untersucht wurden unter anderem:

- Restart-Parameter,
- Perturbationsstärke,
- Frequency Penalty,
- Tabu-Dauer,
- Nachbarschaftsdefinitionen,
- Anzahl exakt bewerteter Moves.

Die Ergebnisse werden gespeichert unter:


Results/Csv/Tabu_Evaluation_Screening.csv


---

## Modus 5: Move selection evaluation

Dieser Modus untersucht gezielt die Move-Auswahl.

Verglichen werden:

- schnelle Abschätzung mit 20 exakt bewerteten Kandidaten,
- vollständige exakte Bewertung ohne Abschätzung,
- zufällige Auswahl von 20 Kandidaten ohne Abschätzung.

Die Ergebnisse werden gespeichert unter: Results/Csv/Tabu_Evaluation_MoveSelection.csv


---

## Modus 6: Adaptive exact evaluation

Dieser Modus bewertet die adaptive exakte Move-Bewertung.

Verglichen werden:

- Baseline mit 20 exakt bewerteten Kandidaten,
- feste Grenzen mit 50, 100 und 200 Kandidaten,
- adaptive Grenze zwischen 50 und 200 Kandidaten.

Die Ergebnisse werden gespeichert unter: Results/Csv/Tabu_Evaluation_AdaptiveExact.csv


---

## Ergebnisdateien

Die wichtigsten Ergebnisdateien befinden sich im Ordner: Results/Csv


Wichtige Dateien:

| Datei                                | Inhalt                                                  |
|--------------------------------------|---------------------------------------------------------|
| `Scheduling_Results.csv`             | Ergebnisse einzelner interaktiver Läufe                 |
| `TeamF_BenchmarkResults.csv`         | finale Full-Benchmark-Ergebnisse der Team-F-Tabu-Search |
| `Tabu_Evaluation_Screening.csv`      | Screening-Evaluation verschiedener Varianten            |
| `Tabu_Evaluation_MoveSelection.csv`  | gezielte Evaluation der Move-Auswahl                    |
| `Tabu_Evaluation_AdaptiveExact.csv`  | Evaluation der adaptiven exakten Move-Bewertung         |

---

## Gantt-Charts

Für einzelne Läufe werden zusätzlich Gantt-Charts erzeugt. Diese befinden sich im Ordner:

```text
Results/GanttCharts
```

Je nach Lauf können folgende Diagramme erzeugt werden:

- Initiallösung,
- Tabu-Search-Lösung im 90-Sekunden-Modus,
- Tabu-Search-Lösung im Extended-Modus,
- CP-SAT-Lösung,
- Vergleichsdiagramm.

Die Gantt-Charts werden als HTML-Dateien gespeichert und können im Browser geöffnet werden.

---

## Benchmarkinstanzen

Die Benchmarkinstanzen befinden sich im Ordner:

```text
Instances/Benchmark
```

Das Projekt wurde auf 60 Benchmarkinstanzen getestet. Dazu gehören:

- Classroom-Instanzen,
- Team-A-Instanzen,
- Team-B-Instanzen,
- Team-C-Instanzen,
- Team-D-Instanzen,
- Team-E-Instanzen,
- Team-F-Instanzen.

---

## Finale Evaluation

Die finale adaptive Tabu Search wurde auf allen 60 Benchmarkinstanzen validiert.

Im Vergleich zur ursprünglichen Ausgangskonfiguration verbesserte sich die Summe der `Cmax`-Werte:

| Modus | Ausgangskonfiguration | Adaptive finale Version | Verbesserung |
|---|---:|---:|---:|
| 90-Sekunden-Lauf | 95.290 | 94.017 | -1.273 |
| Extended-Lauf | 95.124 | 93.625 | -1.499 |

Die Ergebnisse zeigen, dass die adaptive exakte Move-Bewertung die Gesamtleistung verbessert. Gleichzeitig ist die Strategie nicht auf jeder einzelnen Instanz überlegen. Einige Instanzen profitieren stärker von festen Grenzen, während die adaptive Strategie über den gesamten Benchmarkdatensatz die bessere Gesamtleistung erreicht.

---

## Projektstruktur

Eine vereinfachte Übersicht der wichtigsten Ordner:


JobShopSchedulingFramework
│
├── Application
│   └── Steuerung der Konsolenanwendung
│
├── Data
│   └── Einlesen der Instanzdateien
│
├── Heuristics
│   ├── Initial
│   │   └── Giffler-Thompson-Heuristik und Dispatching-Regeln
│   │
│   └── Metaheuristic
│       └── TabuSearch
│           ├── Core
│           ├── Criticality
│           ├── Evaluation
│           ├── Neighborhoods
│           └── Restart
│
├── Models
│   └── Datenmodelle für Jobs, Operationen und Instanzen
│
├── Solvers
│   └── CP-SAT-Referenzsolver
│
├── Results
│   ├── Csv
│   └── GanttCharts
│
└── Program.cs

---

## Team

Dieses Projekt wurde von Team F bearbeitet:

- Zahra Khoobyari
- Carolin Cabuk

---

## Hinweise zur Abgabe

Die finale Version des Projekts befindet sich im Branch:

main


Der Entwicklungsbranch:


zahra-final-benchmark

enthält zusätzlich die experimentelle Entwicklungshistorie. Für die Bewertung sollte der Branch `main` verwendet werden.