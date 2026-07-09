using System.IO;
using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Data
{
    /*
    Liest eine Probleminstanz aus einer Datei ein und überführt sie in das interne Datenmodell.
    Erwartet wird das im Projekt definierte Format mit Meta-Daten, Operationen und Setup-Matrix.
    */
    public class InstanceReader
    {
        public static Instance ReadFromFile(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);
            Instance instance = new Instance();

            int line = 0;

            line++; // Überspringt "#Meta infos".

            string[] meta = lines[line].Split(',');
            instance.NumJobs = int.Parse(meta[0]);
            instance.NumMachines = int.Parse(meta[1]);
            line++;

            line++; // Überspringt "#Processing times".

            // Liest alle Jobs mit ihren Operationen in der vorgegebenen Reihenfolge ein.
            for (int jobID = 1; jobID <= instance.NumJobs; jobID++)
            {
                string[] values = lines[line].Split(',');
                Job job = new Job(jobID);

                int numberOfOperations = int.Parse(values[0]);
                int valueIndex = 1;

                for (int opID = 1; opID <= numberOfOperations; opID++)
                {
                    int machine = int.Parse(values[valueIndex]);
                    int processingTime = int.Parse(values[valueIndex + 1]);

                    job.Operations.Add(
                        new Operation(jobID, opID, machine, processingTime));

                    valueIndex += 2;
                }

                instance.Jobs.Add(job);
                line++;
            }

            line++; // Überspringt "#Setup times".

            instance.SetupTimes = new int[instance.NumJobs, instance.NumJobs];

            // Liest die reihenfolgeabhängigen Setup-Zeiten zwischen Jobs ein.
            for (int i = 0; i < instance.NumJobs; i++)
            {
                string[] row = lines[line].Split(',');

                for (int j = 0; j < instance.NumJobs; j++)
                {
                    instance.SetupTimes[i, j] = int.Parse(row[j]);
                }

                line++;
            }

            return instance;
        }
    }
}