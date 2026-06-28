using JobShopSchedulingFramework.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace JobShopSchedulingFramework.Visualisation
{
    public static class GantChart
    {
        private static readonly string[] JobColors =
        {
            "#98eb63",
            "#282b3a",
            "#d1cbe7",
            "#82420b",
            "#e61aec",
            "#4dec60",
            "#c5c57b",
            "#15e094",
            "#7860b7",
            "#04b08c",
            "#f4a261",
            "#2a9d8f",
            "#e76f51",
            "#457b9d",
            "#ffbe0b"
        };

        private const string SetupColor = "#747474";

        public static void CreateHtml(
            Instance instance,
            string outputPath,
            string algorithmName,
            int objectiveValue,
            string status = "FEASIBLE")
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            List<string> rows = new List<string>();
            List<string> colors = new List<string>();

            int setupCounter = 0;

            for (int machine = 1; machine <= instance.NumMachines; machine++)
            {
                List<Operation> operationsOnMachine = instance.Jobs
                    .SelectMany(job => job.Operations)
                    .Where(op => op.Machine == machine)
                    .Where(op => op.EndTime > op.StartTime)
                    .OrderBy(op => op.StartTime)
                    .ToList();

                int previousJobId = 0;

                foreach (Operation operation in operationsOnMachine)
                {
                    if (previousJobId != 0)
                    {
                        int setupTime =
                            instance.SetupTimes[previousJobId - 1, operation.JobID - 1];

                        if (setupTime > 0)
                        {
                            int setupEnd = operation.StartTime;
                            int setupStart = setupEnd - setupTime;

                            if (setupStart >= 0 && setupStart < setupEnd)
                            {
                                rows.Add(CreateRow(
                                    machine,
                                    "setup" + setupCounter,
                                    setupStart,
                                    setupEnd));

                                colors.Add(SetupColor);

                                setupCounter++;
                            }
                        }
                    }

                    string operationLabel =
                        "J" + operation.JobID + "-O" + operation.OperationID;

                    rows.Add(CreateRow(
                        machine,
                        operationLabel,
                        operation.StartTime,
                        operation.EndTime));

                    colors.Add(GetJobColor(operation.JobID));

                    previousJobId = operation.JobID;
                }
            }

            int maxEndTime = instance.Jobs
                .SelectMany(job => job.Operations)
                .Max(operation => operation.EndTime);

            string colorArray =
                string.Join(", ", colors.Select(c => "'" + c + "'"));

            string rowsArray =
                string.Join("," + Environment.NewLine + "        ", rows);

            string html =
                BuildHtml(
                    instance,
                    rowsArray,
                    colorArray,
                    algorithmName,
                    objectiveValue,
                    status,
                    maxEndTime);

            string? directory =
                Path.GetDirectoryName(outputPath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(outputPath, html, Encoding.UTF8);
        }

        private static string GetJobColor(int jobId)
        {
            int index = (jobId - 1) % JobColors.Length;
            return JobColors[index];
        }

        private static string CreateRow(
            int machine,
            string label,
            int start,
            int end)
        {
            return
                "[\"Machine " + (machine) + "\", " +
                "\"" + JavaScriptEncode(label) + "\", " +
                CreateDate(start) + ", " +
                CreateDate(end) + "]";
        }

        private static string CreateDate(int time)
        {
            return "new Date(0,0,0,0," + time + ",0,0)";
        }

        private static string JavaScriptEncode(string value)
        {
            return WebUtility.HtmlEncode(value)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }


        private static string BuildHtml(
            Instance instance,
            string rowsArray,
            string colorArray,
            string algorithmName,
            int objectiveValue,
            string status,
            int maxEndTime)
        {
            return @"<!DOCTYPE html>
        <html>
        <head>
        <meta charset=""utf-8"">

        <style>
        body {
            font-family: Helvetica, Arial, sans-serif;
            font-size: 12px;
        }

        #schedule {
            height: 500px;
        }
        </style>

        <script type=""text/javascript"" src=""https://www.gstatic.com/charts/loader.js""></script>

        <script type=""text/javascript"">

        google.charts.load('current', { packages:['timeline'] });
        google.charts.setOnLoadCallback(drawChart);

        function drawChart()
        {
            var container =
                document.getElementById('schedule');

            var chart =
                new google.visualization.Timeline(container);

            var dataTable =
                new google.visualization.DataTable();

            dataTable.addColumn({ type: 'string', id: 'Machine' });
            dataTable.addColumn({ type: 'string', id: 'Operation' });
            dataTable.addColumn({ type: 'date', id: 'Start' });
            dataTable.addColumn({ type: 'date', id: 'End' });

            dataTable.addRows([
                " + rowsArray + @"
            ]);

            var options =
            {
                colors: [" + colorArray + @"],

                hAxis:
                {
                    minValue: new Date(0,0,0,0,0,0,0),
                    maxValue: new Date(0,0,0,0," + maxEndTime + @",0,0)
                },

                timeline:
                {
                    rowLabelStyle:
                    {
                        fontName: 'Helvetica',
                        fontSize: 12
                    },

                    barLabelStyle:
                    {
                        fontName: 'Helvetica',
                        fontSize: 12
                    }
                }
            };

            chart.draw(dataTable, options);
        }

        </script>
        </head>

        <body>

        <div style='margin-top:10px; margin-bottom:20px;'>

        Instance with n=" + instance.NumJobs + @", m=" + instance.NumMachines + @"<br>

        Objective function: Makespan<br>

        Algorithm: " + WebUtility.HtmlEncode(algorithmName) + @"<br>

        Objective value: " + objectiveValue + @"<br>

        Status: " + WebUtility.HtmlEncode(status) + @"

        </div>

        <div id='schedule'></div>

        </body>
        </html>";
        }
        public static void CreateComparisonHtml(
    string outputPath,
    string initialChartFile,
    string tabuChartFile,
    string cpChartFile,
    string initialAlgorithmName,
    string tabuAlgorithmName,
    int initialCmax,
    int tabuCmax,
    int cpCmax,
    string cpStatus)
        {
            int improvement =
                initialCmax - tabuCmax;

            double improvementPercent =
                initialCmax > 0
                    ? (double)improvement / initialCmax * 100.0
                    : 0.0;

            int gapToCp =
                tabuCmax - cpCmax;

            double gapToCpPercent =
                cpCmax > 0
                    ? (double)gapToCp / cpCmax * 100.0
                    : 0.0;

            string html =
        $@"<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"">
<style>
body {{
    font-family: Helvetica, Arial, sans-serif;
    font-size: 13px;
    margin: 20px;
}}

.summary {{
    border: 1px solid #cccccc;
    background: #f7f7f7;
    padding: 12px;
    margin-bottom: 25px;
}}

iframe {{
    width: 100%;
    height: 620px;
    border: 1px solid #cccccc;
}}
</style>
</head>

<body>

    <h1>Initial Heuristic vs Tabu Search</h1>

    <div class=""summary"">
    <b>Initial heuristic:</b> {WebUtility.HtmlEncode(initialAlgorithmName)}<br>
    <b>Initial Cmax:</b> {initialCmax}<br><br>

    <b>Tabu Search:</b> {WebUtility.HtmlEncode(tabuAlgorithmName)}<br>
    <b>Tabu Cmax:</b> {tabuCmax}<br><br>

    <b>CP Solver:</b> {WebUtility.HtmlEncode(cpStatus)}<br>
    <b>CP Cmax:</b> {cpCmax}<br>
    <b>Gap Tabu to CP:</b> {gapToCp} ({gapToCpPercent:F2}%)<br><br>

    <b>Tabu Improvement:</b> {improvement} ({improvementPercent:F2}%)
    </div>

    <h2>Initial Heuristic Schedule</h2>
    <iframe src=""{WebUtility.HtmlEncode(initialChartFile)}""></iframe>

    <h2>Tabu Search Schedule</h2>
    <iframe src=""{WebUtility.HtmlEncode(tabuChartFile)}""></iframe>

    <h2>CP Solver Schedule</h2>
    <iframe src=""{WebUtility.HtmlEncode(cpChartFile)}""></iframe>

</body>
</html>";

            string? directory =
                 Path.GetDirectoryName(outputPath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(outputPath, html, Encoding.UTF8);
        }
    }
}