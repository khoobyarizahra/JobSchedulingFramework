using System;
using System.Collections.Generic;
using System.Text;

/* Instance Generator mit zuälligen Instanzen
   Vorherige Eingabe von jobs und Maschinen*/


/*
namespace JobShopSchedulingFramework.DataGeneration
{
    class InstanceGeneratorEasy
    {
        static Random rnd = new Random();

        static void Main()
        {
            int jobs = 10;
            int machines = 5;

            int count = 1;

            while (File.Exists($"MediumInstance_{count}.txt"))
            {
                count++;
            }

            string fileName = $"MediumInstance_{count}.txt";

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                //Meta Infos
                writer.WriteLine("#Meta infos");
                // Erste Zeile
                writer.WriteLine(jobs + "," + machines);

                // Processing Times
                writer.WriteLine("#Processing times");

                for (int j = 0; j < jobs; j++)
                {
                    // Zufällige Anzahl Operationen zwischen 1 und Maschinenzahl
                    int ops = rnd.Next(1, machines + 1);

                    string line = ops.ToString();

                    bool[] usedMachines = new bool[machines];

                    for (int i = 0; i < ops; i++)
                    {
                        int machine;

                        // Maschine darf nur einmal pro Job vorkommen
                        do
                        {
                            machine = rnd.Next(1, machines + 1);
                        }
                        while (usedMachines[machine - 1]);

                        usedMachines[machine - 1] = true;

                        int time = rnd.Next(10, 101); // Dauer 10 bis 100

                        line += "," + machine + "," + time;
                    }

                    writer.WriteLine(line);
                }

                // Setup Times
                writer.WriteLine("#Setup times");

                for (int i = 0; i < jobs; i++)
                {
                    string line = "";

                    for (int j = 0; j < jobs; j++)
                    {
                        int setup;

                        if (i == j)
                            setup = 0;
                        else
                            setup = rnd.Next(5, 41); // Setupzeit 5 bis 40

                        line += setup;

                        if (j < jobs - 1)
                            line += ",";
                    }

                    writer.WriteLine(line);
                }
            }

            Console.WriteLine("Instanz gespeichert in " + fileName);
        }
    }
}
*/