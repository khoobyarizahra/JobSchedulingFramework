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
                writer.WriteLine($"{instance.numJobs},{instance.numMachines}");

                /*
                 PROCESSING TIMES
                */
                writer.WriteLine("#Processing times");

                foreach (Job job in instance.jobs)
                {
                    /*
                     First value:
                     number of operations of this job
                    */
                    string line = job.operations.Count.ToString();

                    /*
                     Then:
                     machine,processingTime pairs
                    */
                    foreach (Operation operation in job.operations)
                    {
                        line += $",{operation.machine},{operation.processingTime}";
                    }

                    writer.WriteLine(line);
                }

                /*
                 SETUP TIMES
                */
                writer.WriteLine("#Setup times");

                for (int fromJob = 0; fromJob < instance.numJobs; fromJob++)
                {
                    string line = "";

                    for (int toJob = 0; toJob < instance.numJobs; toJob++)
                    {
                        line += instance.setupTimes[fromJob, toJob];

                        if (toJob < instance.numJobs - 1)
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