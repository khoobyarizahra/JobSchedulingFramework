using System.Globalization;
using System.Text;

namespace JobShopSchedulingFramework.Results
{
    /// <summary>
    /// Writes evaluation results to semicolon-separated CSV files.
    ///
    /// Required CSV format:
    /// instanceSetName;instanceName;algorithm;status;objVal;compTime
    /// </summary>
    public static class CsvResultWriter
    {
        /// <summary>
        /// Writes result rows to the default CSV file.
        ///
        /// This method is useful for interactive experiments.
        /// If the file already exists, the new rows are appended.
        /// </summary>
        public static string WriteResults(
            List<CsvResultRow> rows)
        {
            return WriteResultsToFile(
                rows,
                "Scheduling_Results.csv",
                append: true);
        }

        /// <summary>
        /// Writes result rows to a selected CSV file inside Results/Csv.
        ///
        /// For final benchmark evaluation, append should be false,
        /// so the file is recreated on every full run.
        /// </summary>
        public static string WriteResultsToFile(
            List<CsvResultRow> rows,
            string fileName,
            bool append)
        {
            string outputFolder =
                Path.Combine(
                    GetProjectRootFolder(),
                    "Results",
                    "Csv");

            Directory.CreateDirectory(outputFolder);

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
                    "instanceSetName;instanceName;algorithm;status;objVal;compTime");
            }

            foreach (CsvResultRow row in rows)
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

            return Path.GetFullPath(csvPath);
        }

        /// <summary>
        /// Creates one semicolon-separated CSV line from one result row.
        /// </summary>
        private static string CreateCsvLine(
            CsvResultRow row)
        {
            return
                EscapeCsvValue(row.InstanceSetName) + ";" +
                EscapeCsvValue(row.InstanceName) + ";" +
                EscapeCsvValue(row.Algorithm) + ";" +
                EscapeCsvValue(row.Status) + ";" +
                row.ObjectiveValue.ToString(CultureInfo.InvariantCulture) + ";" +
                row.ComputationTimeSeconds.ToString("F2", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Escapes values that contain semicolons, quotation marks, or line breaks.
        /// </summary>
        private static string EscapeCsvValue(
            string value)
        {
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

        /// <summary>
        /// Returns the project root folder.
        /// </summary>
        private static string GetProjectRootFolder()
        {
            return Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    @"..\..\.."));
        }
    }
}