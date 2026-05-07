using Project_Scheduling_ZahraAndCarolin.Models;
using System;
using System.Collections.Generic;
using System.Text;

//INSTANCE READER Liest die Instanz aus einer Datei ein.

namespace Project_Scheduling_ZahraAndCarolin.DataInput
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
            instance.numJobs = int.Parse(meta[0]);
            instance.numMachines = int.Parse(meta[1]);
            line++;

            line++; // Skip "#Processing times"

            // Jobs und Operationen einlesen
            for (int jobID = 1; jobID <= instance.numJobs; jobID++)
            {
                string[] values = lines[line].Split(',');
                Job job = new Job(jobID);

                int numberOfOperations = int.Parse(values[0]);
                int valueIndex = 1;

                for (int opID = 1; opID <= numberOfOperations; opID++)
                {
                    int machine = int.Parse(values[valueIndex]);
                    int processingTime = int.Parse(values[valueIndex + 1]);

                    job.operations.Add(new Operation(jobID, opID, machine, processingTime));

                    valueIndex += 2;
                }

                instance.jobs.Add(job);
                line++;
            }

            line++; // Skip "#Setup times"

            instance.setupTimes = new int[instance.numJobs, instance.numJobs];

            // Setup-Matrix einlesen
            for (int i = 0; i < instance.numJobs; i++)
            {
                string[] row = lines[line].Split(',');

                for (int j = 0; j < instance.numJobs; j++)
                {
                    instance.setupTimes[i, j] = int.Parse(row[j]);
                }

                line++;
            }

            return instance;
        }
    }
}
