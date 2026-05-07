using System;
using System.Collections.Generic;
using System.Text;


/*
 PRIORITY RULES
 Hier definieren wir alle Prioritätsregeln, die wir vergleichen wollen.
 Vorteil von enum:
 - typsicher
 - keine Tippfehler (kein String!)
 - einfach iterierbar im Experiment
 */


namespace Project_Scheduling_ZahraAndCarolin.Models
{
    public enum PriorityRule
    {
        LRPT,              // Longest Remaining Processing Time (das beste für unsere Ziefunktion laut dem Artikel)
        LPT,               // Longest Processing Time
        SPT,               // Shortest Processing Time
        SRPT,              // Shortest Remaining Processing Time
        Random,            // Zufällige Auswahl (mit Seed reproduzierbar)
        SetupAwareLRPT     // Eigene Erweiterung: berücksichtigt Setup Times
    }
}
