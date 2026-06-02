using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    // Hilfsklasse zur Erstellung einer tiefen Kopie der Maschinenreihenfolge, um sicherzustellen, dass Änderungen an der Kopie die Originalreihenfolge nicht beeinflussen.
    public static class MachineOrderCopier
    {
        //in dictionary copy ist der Schlüssel der Maschinentyp und der Wert ist eine Liste von Operationen, die auf dieser Maschine ausgeführt werden sollen.
        public static Dictionary<int, List<Operation>> Copy(
            Dictionary<int, List<Operation>> originalOrders)
        {
            Dictionary<int, List<Operation>> copy =
                new Dictionary<int, List<Operation>>();
            // Für jede Maschine (Schlüssel) und ihre zugehörige Liste von Operationen (Wert) in der originalOrders-Dictionary wird eine neue Liste von Operationen
            // erstellt und der Schlüssel-Wert-Paar in die copy-Dictionary eingefügt.
            foreach (var pair in originalOrders)
            {
                copy[pair.Key] =
                    new List<Operation>(pair.Value);
            }

            return copy;
        }
    }
}