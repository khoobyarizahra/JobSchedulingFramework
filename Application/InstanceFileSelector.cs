using System;
using System.IO;

namespace JobShopSchedulingFramework.Application
{
    public static class InstanceFileSelector
    {
        public static string SelectFromFolder(
            string folder)
        {
            string fullFolderPath =
                Path.GetFullPath(folder);

            Console.WriteLine("Looking for instances in:");
            Console.WriteLine(fullFolderPath);
            Console.WriteLine();

            string[] files =
                Directory.GetFiles(
                    fullFolderPath,
                    "*.txt");

            if (files.Length == 0)
            {
                throw new InvalidOperationException(
                    "No instance files found in " + fullFolderPath);
            }

            Console.WriteLine("Available instances:");
            Console.WriteLine();

            for (int i = 0; i < files.Length; i++)
            {
                Console.WriteLine(
                    (i + 1) + ": " + Path.GetFileName(files[i]));
            }

            Console.WriteLine();
            Console.Write("Please select an instance number: ");

            while (true)
            {
                string input =
                    Console.ReadLine() ?? "";

                bool validNumber =
                    int.TryParse(
                        input,
                        out int selectedIndex);

                if (validNumber &&
                    selectedIndex >= 1 &&
                    selectedIndex <= files.Length)
                {
                    return files[selectedIndex - 1];
                }

                Console.Write(
                    "Invalid input. Please enter a number between 1 and " +
                    files.Length +
                    ": ");
            }
        }
    }
}