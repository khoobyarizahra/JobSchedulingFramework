using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Project_Scheduling_ZahraAndCarolin.Models;
using Project_Scheduling_ZahraAndCarolin.DataInput;
using Project_Scheduling_ZahraAndCarolin.Heuristics;
using Project_Scheduling_ZahraAndCarolin.Utils;
using Project_Scheduling_ZahraAndCarolin.Experiments;
using Project_Scheduling_ZahraAndCarolin.DataGeneration;


/*
 Program.cs

 Einstiegspunkt des gesamten Projekts.
 Startet den experimentellen Vergleich der Heuristiken.
*/


public class Program
{
    public static void Main(string[] args)
    {
        //Generieren von Instanzen
        
        // !Nur ausklammern zum generieren von Instanzen!
        //InstanceGeneratorAdvanced.Generate();

        string fileName = @"instances\instance_1.txt";
        HeuristicExperiment.Run(fileName);
    }
}
