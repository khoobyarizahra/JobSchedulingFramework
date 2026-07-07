using JobShopSchedulingFramework.Results;


namespace JobShopSchedulingFramework.Solvers.CPBenchmarkEvaluation
{
    public static class CpBenchmarkApplication
    {
        public static void Run(string folder)
        {
            var files =
                Directory.GetFiles(folder, "*.txt")
                         .OrderBy(x => x)
                         .ToList();

            string outputPath =
    Path.Combine(
        GetProjectRootFolder(),
        "Results",
        "Csv",
        "cp_benchmark.csv");


            Directory.CreateDirectory(
                Path.GetDirectoryName(outputPath)!);


            CpBenchmarkCsvWriter.WriteHeader(
                outputPath);


            var results =
                CpBenchmarkRunner.RunBatch(
                    files,
                    timeLimitSeconds: 90,
                    outputPath);


            Console.WriteLine("CP benchmark CSV written to:");
            Console.WriteLine(outputPath);

            Console.WriteLine("CP benchmark CSV written to:");
            Console.WriteLine(outputPath);
        }


        private static string GetProjectRootFolder()
        {
            return Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    @"..\..\.."));
        }
    }
}