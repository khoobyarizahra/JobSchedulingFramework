using System.Globalization;
using System.Text;

namespace JobShopSchedulingFramework.Results
{
    /// <summary>
    /// Writes evaluation results to semicolon-separated CSV files.
    ///
    /// Output CSV format:
    /// instanceName;algorithm;status;objVal;compTime
    ///
    /// The instanceName contains the complete benchmark file name
    /// without the .txt extension, for example:
    /// ClassroomInstanceSet1_1, TeamA_3, TeamF_Instance1.
    ///
    /// The compTime value is written with a comma as decimal separator
    /// because the CSV uses semicolons and is opened with German Excel.
    /// Example: 4,13 instead of 4.13.
    /// This prevents Excel from converting times such as 4.13 into dates.
    /// </summary>
    public static class CsvResultWriter
    {
        private static readonly CultureInfo GermanCulture =
            CultureInfo.GetCultureInfo("de-DE");

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
                    "instanceName;algorithm;status;objVal;compTime");
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

            return Path.GetFullPath(
                csvPath);
        }

        /// <summary>
        /// Creates one semicolon-separated CSV line from one result row.
        ///
        /// Only the full instanceName is written.
        /// The instanceSetName is intentionally not written to the CSV.
        /// </summary>
        private static string CreateCsvLine(
            CsvResultRow row)
        {
            return
                EscapeCsvValue(row.InstanceName) + ";" +
                EscapeCsvValue(row.Algorithm) + ";" +
                EscapeCsvValue(row.Status) + ";" +
                row.ObjectiveValue.ToString(CultureInfo.InvariantCulture) + ";" +
                FormatComputationTime(row.ComputationTimeSeconds);
        }

        /// <summary>
        /// Formats the computation time as a German decimal number.
        ///
        /// Example:
        /// 4.13 seconds  -> 4,13
        /// 90.00 seconds -> 90,00
        /// </summary>
        private static string FormatComputationTime(
            double seconds)
        {
            return seconds.ToString(
                "F2",
                GermanCulture);
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
        ///
        /// During execution, relative paths start in bin/Debug/netX.
        /// This method moves three levels up to the project folder.
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