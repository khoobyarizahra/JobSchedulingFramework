using System.Collections.Generic;
using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /*
    Zentrale Schnittstelle für den Umgang mit Maschinenreihenfolgen.
    Die konkrete Logik liegt in spezialisierten Hilfsklassen.
    */
    public static class ScheduleOrderHelper
    {
        // Erstellt aus den aktuellen Startzeiten die Reihenfolge der Operationen je Maschine.
        public static Dictionary<int, List<Operation>> BuildMachineOrders(
            Instance instance)
        {
            return MachineOrderBuilder.Build(instance);
        }

        // Kopiert Maschinenreihenfolgen, damit Moves die Ursprungslösung nicht direkt verändern.
        public static Dictionary<int, List<Operation>> CopyMachineOrders(
            Dictionary<int, List<Operation>> originalOrders)
        {
            return MachineOrderCopier.Copy(originalOrders);
        }

        /*
        Berechnet für eine Maschinenreihenfolge die Start- und Endzeiten neu.
        Gibt false zurück, falls die Reihenfolge keinen zulässigen Schedule ergibt.
        */
        public static bool RecalculateScheduleFromMachineOrders(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders,
            out int cmax)
        {
            return ScheduleRecalculator.Recalculate(
                instance,
                machineOrders,
                out cmax);
        }
    }
}