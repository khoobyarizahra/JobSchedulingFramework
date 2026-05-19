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

            for (int machine = 1; machine <= instance.numMachines; machine++)
            {
                List<Operation> operationsOnMachine = instance.jobs
                    .SelectMany(job => job.operations)
                    .Where(op => op.machine == machine)
                    .Where(op => op.endTime > op.startTime)
                    .OrderBy(op => op.startTime)
                    .ToList();

                int previousJobId = 0;

                foreach (Operation operation in operationsOnMachine)
                {
                    if (previousJobId != 0)
                    {
                        int setupTime =
                            instance.setupTimes[previousJobId - 1, operation.jobID - 1];

                        if (setupTime > 0)
                        {
                            int setupEnd = operation.startTime;
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
                        "J" + operation.jobID + "-O" + operation.operationID;

                    rows.Add(CreateRow(
                        machine,
                        operationLabel,
                        operation.startTime,
                        operation.endTime));

                    colors.Add(GetJobColor(operation.jobID));

                    previousJobId = operation.jobID;
                }
            }

            int maxEndTime = instance.jobs
                .SelectMany(job => job.operations)
                .Max(operation => operation.endTime);

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

            string directory =
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

Instance with n=" + instance.numJobs + @", m=" + instance.numMachines + @"<br>

Objective function: Makespan<br>

Algorithm: " + WebUtility.HtmlEncode(algorithmName) + @"<br>

Objective value: " + objectiveValue + @"<br>

Status: " + WebUtility.HtmlEncode(status) + @"

</div>

<div id='schedule'></div>

</body>
</html>";
        }
    }
}