using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace JobShopSchedulingFramework.Evaluation
{
    public static class TabuEvaluationCsvWriter
    {
        /*
        Diese Klasse schreibt die ausführlichen Evaluationsdaten in eine eigene CSV.

        Wichtig:
        Die ursprüngliche Abgabe-CSV bleibt unverändert.
        Diese neue CSV ist nur für die wissenschaftliche Analyse der Varianten gedacht.

        Dadurch können wir neue Experimente durchführen, ohne die ursprünglichen
        Benchmark-Ergebnisse mit Analysewerten oder zusätzlichen Spalten zu vermischen.
        */

        private static readonly CultureInfo GermanCulture =
            CultureInfo.GetCultureInfo("de-DE");

        public static string WriteResultsToFile(
            List<TabuEvaluationResultRow> rows,
            string fileName,
            bool append)
        {
            /*
            Die Datei wird im Ordner Results/Csv gespeichert.
            Damit liegen alle Ergebnisdateien weiterhin an einer bekannten Stelle,
            aber die neue Datei kann einen eigenen Namen bekommen, z. B.
            Tabu_Evaluation_Screening.csv.
            */

            string outputFolder =
                Path.Combine(
                    GetProjectRootFolder(),
                    "Results",
                    "Csv");

            Directory.CreateDirectory(
                outputFolder);

            string csvPath =
                Path.Combine(
                    outputFolder,
                    fileName);

            bool writeHeader =
                !append || !File.Exists(csvPath);

            List<string> lines =
                new List<string>();

            if (writeHeader)
            {
                lines.Add(
                    CreateHeaderLine());
            }

            foreach (TabuEvaluationResultRow row in rows)
            {
                lines.Add(
                    CreateCsvLine(row));
            }

            if (append)
            {
                File.AppendAllLines(
                    csvPath,
                    lines,
                    Encoding.UTF8);
            }
            else
            {
                File.WriteAllLines(
                    csvPath,
                    lines,
                    Encoding.UTF8);
            }

            return Path.GetFullPath(
                csvPath);
        }

        private static string CreateHeaderLine()
        {
            /*
            Die Spalten sind bewusst ausführlich.
            Für die Hausarbeit können daraus später Tabellen, Diagramme und
            gruppierte Auswertungen erzeugt werden.
            */

            return string.Join(
                ";",
                new[]
                {
                    "instanceName",
                    "variantName",
                    "category",
                    "runMode",
                    "neighborhood",
                    "isBaseline",
                    "multiStartCount",
                    "maxIterations",
                    "timeLimitSeconds",
                    "initialRule",
                    "initialCmax",
                    "bestCmax",
                    "improvementPercent",
                    "runtimeSeconds",
                    "stopReason",
                    "iterations",
                    "generatedMoves",
                    "estimatedMoves",
                    "exactEvaluations",
                    "feasibleCandidates",
                    "infeasibleCandidates",
                    "tabuRejectedMoves",
                    "appliedMoves",
                    "improvements",
                    "restartEnabled",
                    "restartThreshold",
                    "restartPerturbationMoves",
                    "restarts",
                    "successfulRestarts",
                    "fallbackRestarts",
                    "restartSuccessRate",
                    "frequencyPenaltyEnabled",
                    "lowPenaltyRate",
                    "mediumPenaltyRate",
                    "highPenaltyRate",
                    "maxExactEvaluationsPerIteration",
                    "tabuMinTenureFactor",
                    "tabuMaxTenureFactor"
                });
        }

        private static string CreateCsvLine(
            TabuEvaluationResultRow row)
        {
            /*
            Jede Eigenschaft wird in eine CSV-Spalte übertragen.
            Zahlen mit Dezimalstellen werden mit deutschem Dezimaltrennzeichen
            geschrieben, damit die Datei direkt in deutschem Excel lesbar ist.
            */

            return string.Join(
                ";",
                new[]
                {
                    EscapeCsvValue(row.InstanceName),
                    EscapeCsvValue(row.VariantName),
                    EscapeCsvValue(row.Category),
                    EscapeCsvValue(row.RunMode),
                    EscapeCsvValue(row.NeighborhoodName),
                    FormatBool(row.IsBaseline),
                    row.MultiStartCount.ToString(CultureInfo.InvariantCulture),
                    row.MaxIterations.ToString(CultureInfo.InvariantCulture),
                    row.TimeLimitSeconds.ToString(CultureInfo.InvariantCulture),
                    EscapeCsvValue(row.InitialRule),
                    row.InitialCmax.ToString(CultureInfo.InvariantCulture),
                    row.BestCmax.ToString(CultureInfo.InvariantCulture),
                    FormatDouble(row.ImprovementPercent),
                    FormatDouble(row.RuntimeSeconds),
                    EscapeCsvValue(row.StopReason),
                    row.Iterations.ToString(CultureInfo.InvariantCulture),
                    row.GeneratedMoves.ToString(CultureInfo.InvariantCulture),
                    row.EstimatedMoves.ToString(CultureInfo.InvariantCulture),
                    row.ExactEvaluations.ToString(CultureInfo.InvariantCulture),
                    row.FeasibleCandidates.ToString(CultureInfo.InvariantCulture),
                    row.InfeasibleCandidates.ToString(CultureInfo.InvariantCulture),
                    row.TabuRejectedMoves.ToString(CultureInfo.InvariantCulture),
                    row.AppliedMoves.ToString(CultureInfo.InvariantCulture),
                    row.Improvements.ToString(CultureInfo.InvariantCulture),
                    FormatBool(row.RestartEnabled),
                    row.RestartThreshold.ToString(CultureInfo.InvariantCulture),
                    row.RestartPerturbationMoves.ToString(CultureInfo.InvariantCulture),
                    row.Restarts.ToString(CultureInfo.InvariantCulture),
                    row.SuccessfulRestarts.ToString(CultureInfo.InvariantCulture),
                    row.FallbackRestarts.ToString(CultureInfo.InvariantCulture),
                    FormatDouble(row.RestartSuccessRate),
                    FormatBool(row.FrequencyPenaltyEnabled),
                    FormatDouble(row.LowPenaltyRate),
                    FormatDouble(row.MediumPenaltyRate),
                    FormatDouble(row.HighPenaltyRate),
                    row.MaxExactEvaluationsPerIteration.ToString(CultureInfo.InvariantCulture),
                    FormatDouble(row.TabuMinTenureFactor),
                    FormatDouble(row.TabuMaxTenureFactor)
                });
        }

        private static string FormatDouble(
            double value)
        {
            /*
            Dezimalzahlen werden mit vier Nachkommastellen gespeichert.
            Das ist genauer als die Konsolenausgabe und reicht für Vergleiche
            in Tabellen und Diagrammen aus.
            */

            return value.ToString(
                "F4",
                GermanCulture);
        }

        private static string FormatBool(
            bool value)
        {
            /*
            Boolean-Werte werden auf Englisch geschrieben.
            Das ist für spätere automatische Auswertungen in Python, Excel
            oder Power BI einfacher als übersetzte Werte.
            */

            return value
                ? "true"
                : "false";
        }

        private static string EscapeCsvValue(
            string value)
        {
            /*
            Textwerte werden nur dann in Anführungszeichen gesetzt,
            wenn dies wegen Semikolon, Anführungszeichen oder Zeilenumbruch
            notwendig ist.
            */

            if (value.Contains(";") ||
                value.Contains("\"") ||
                value.Contains("\n") ||
                value.Contains("\r"))
            {
                return "\"" +
                       value.Replace("\"", "\"\"") +
                       "\"";
            }

            return value;
        }

        private static string GetProjectRootFolder()
        {
            /*
            Während der Ausführung startet das Programm normalerweise aus
            bin/Debug/netX. Durch drei Ebenen nach oben wird wieder der
            Projektordner erreicht.
            */

            return Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    @"..\..\.."));
        }
    }
}