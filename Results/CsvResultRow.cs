namespace JobShopSchedulingFramework.Results
{
    /// <summary>
    /// Represents one row in the evaluation CSV file.
    ///
    /// The required CSV format is:
    /// instanceSetName;instanceName;algorithm;status;objVal;compTime
    /// </summary>
    public class CsvResultRow
    {
        public string InstanceSetName { get; set; } = "";

        public string InstanceName { get; set; } = "";

        public string Algorithm { get; set; } = "";

        public string Status { get; set; } = "";

        public int ObjectiveValue { get; set; }

        public double ComputationTimeSeconds { get; set; }
    }
}