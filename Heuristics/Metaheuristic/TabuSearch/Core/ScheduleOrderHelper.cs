using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /// <summary>
    /// Fassade für die Verwaltung von Maschinenreihenfolgen und die
    /// Neuberechnung von Zeitplänen.
    /// Die eigentliche Logik befindet sich in spezialisierten Klassen:
    /// - MachineOrderBuilder
    /// - MachineOrderCopier
    /// - ScheduleRecalculator
    /// Diese Klasse stellt eine zentrale Schnittstelle bereit, damit
    /// bestehender Code nicht angepasst werden muss.
    /// </summary>
    public static class ScheduleOrderHelper
    {
        //Diese Methode Erstellt für jede Maschine die Reihenfolge der Operationen basierend auf den aktuellen Startzeiten.
        public static Dictionary<int, List<Operation>> BuildMachineOrders(Instance instance)
        {
            return MachineOrderBuilder.Build(instance);
        }
        //Erstellt eine Kopie der Maschinenreihenfolge, damit Nachbarschaftsbewegungen die ursprüngliche Lösung nicht verändern.
        public static Dictionary<int, List<Operation>> CopyMachineOrders(
    Dictionary<int, List<Operation>> originalOrders)
        {
            return MachineOrderCopier.Copy(originalOrders);
        }
        //Berechnet die Start- und Endzeiten aller Operationen für eine gegebene Maschinenreihenfolge neu und liefert
        //den resultierenden Makespan zurück.
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