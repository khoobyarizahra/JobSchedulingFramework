using JobShopSchedulingFramework.Models;

//INSTANCE READER Liest die Instanz aus einer Datei ein.

namespace JobShopSchedulingFramework.Data
{

    public class InstanceReader
    {
        public static Instance ReadFromFile(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);
            Instance instance = new Instance();

            int line = 0;

            line++; // Skip "#Meta infos"

            // Anzahl Jobs und Maschinen
            string[] meta = lines[line].Split(',');
            instance.NumJobs = int.Parse(meta[0]);
            instance.NumMachines = int.Parse(meta[1]);
            line++;

            line++; // Skip "#Processing times"

            // Jobs und Operationen einlesen
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

                    job.Operations.Add(new Operation(jobID, opID, machine, processingTime));

                    valueIndex += 2;
                }

                instance.Jobs.Add(job);
                line++;
            }

            line++; // Skip "#Setup times"

            instance.SetupTimes = new int[instance.NumJobs, instance.NumJobs];

            // Setup-Matrix einlesen
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
