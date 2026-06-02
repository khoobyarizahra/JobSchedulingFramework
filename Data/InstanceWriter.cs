using JobShopSchedulingFramework.Models;
using System.IO;

namespace JobShopSchedulingFramework.Data
{
    /*
     InstanceWriter

     This class saves scheduling instances into text files.

     Important:
     - Only responsible for writing files.
     - No generation logic.
     - No scheduling logic.
     Single Responsibility Principle:
     one class = one task.
    */
    public class InstanceWriter
    {
        /*
         WriteToFile

         Saves one Instance object into a .txt file.

         Parameters:
         instance -> scheduling instance to save
         fileName -> output path
        */
        public void WriteToFile(Instance instance, string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                /*
                 META INFORMATION
                */
                writer.WriteLine("#Meta infos");
                writer.WriteLine($"{instance.NumJobs},{instance.NumMachines}");

                /*
                 PROCESSING TIMES
                */
                writer.WriteLine("#Processing times");

                foreach (Job job in instance.Jobs)
                {
                    /*
                     First value:
                     number of Operations of this job
                    */
                    string line = job.Operations.Count.ToString();

                    /*
                     Then:
                     Machine,ProcessingTime pairs
                    */
                    foreach (Operation operation in job.Operations)
                    {
                        line += $",{operation.Machine},{operation.ProcessingTime}";
                    }

                    writer.WriteLine(line);
                }

                /*
                 SETUP TIMES
                */
                writer.WriteLine("#Setup times");

                for (int fromJob = 0; fromJob < instance.NumJobs; fromJob++)
                {
                    string line = "";

                    for (int toJob = 0; toJob < instance.NumJobs; toJob++)
                    {
                        line += instance.SetupTimes[fromJob, toJob];

                        if (toJob < instance.NumJobs - 1)
                        {
                            line += ",";
                        }
                    }

                    writer.WriteLine(line);
                }
            }
        }
    }
}