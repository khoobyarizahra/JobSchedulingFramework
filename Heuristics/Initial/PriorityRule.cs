namespace JobShopSchedulingFramework.Heuristics.Initial
{
    /*
    Definiert die Prioritätsregeln für die Konstruktion der Startlösung.
    Die Regeln werden verwendet, um verfügbare Operationen in der Heuristik auszuwählen.
    */
    public enum PriorityRule
    {
        // Wählt die Operation mit der größten verbleibenden Bearbeitungszeit im Job.
        LRPT,

        // Wählt die Operation mit der längsten einzelnen Bearbeitungszeit.
        LPT,

        // Wählt die Operation mit der kürzesten einzelnen Bearbeitungszeit.
        SPT,

        // Wählt die Operation mit der kleinsten verbleibenden Bearbeitungszeit im Job.
        SRPT,

        // Wählt zufällig eine verfügbare Operation aus.
        Random,

        // Erweiterte LRPT-Regel, die zusätzlich Setup-Zeiten berücksichtigt.
        SetupAwareLRPT
    }
}