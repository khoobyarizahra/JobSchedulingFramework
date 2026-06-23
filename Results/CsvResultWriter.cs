using System.Globalization;
using System.Text;

namespace JobShopSchedulingFramework.Results
{
    /// <summary>
    /// Writes evaluation results to a semicolon-separated CSV file.
    ///
    /// The file is stored in:
    /// Results/Csv/Scheduling_Results.csv
    ///
    /// The required CSV format is:
    /// instanceSetName;instanceName;algorithm;status;objVal;compTime
    /// </summary>
    public static class CsvResultWriter
    {
        /// <summary>
        /// Writes the given result rows to the CSV file.
        ///
        /// If the file already exists, the new rows are appended.
        /// If the file does not exist yet, the header line is written first.
        /// </summary>
        public static string WriteResults(
            List<CsvResultRow> rows)
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
                    "Scheduling_Results.csv");

            bool fileAlreadyExists =
                File.Exists(csvPath);

            List<string> lines =
                new List<string>();

            if (!fileAlreadyExists)
            {
                lines.Add(
                    "instanceSetName;instanceName;algorithm;status;objVal;compTime");
            }

            foreach (CsvResultRow row in rows)
            {
                lines.Add(
                    CreateCsvLine(row));
            }

            File.AppendAllLines(
                csvPath,
                lines,
                Encoding.UTF8);

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