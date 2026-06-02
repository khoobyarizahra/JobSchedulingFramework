using JobShopSchedulingFramework.Models;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    // Hilfsklasse zur Bestimmung der Vorgänger einer Operation basierend auf der Job- und Maschinenreihenfolge.
    public static class OperationPredecessorFinder
    {
        //Die Methode GetJobPredecessor bekommt eine Instanz und eine Operation und gibt die Vorgängeroperation zurück,
        //die in der Jobreihenfolge vor der gegebenen Operation liegt. Wenn die gegebene Operation die erste Operation im Job ist, wird null zurückgegeben.
        public static Operation GetJobPredecessor(
            Instance instance,
            Operation operation)
        {
            // Finde den Job, zu dem die gegebene Operation gehört.
            Job job =
                instance.Jobs.First(j => j.JobID == operation.JobID);
            // Finde den Index der gegebenen Operation in der Liste der Operationen des Jobs.
            //Wir brauchen den Index, um die Vorgängeroperation zu identifizieren, die sich direkt vor der gegebenen Operation in der Jobreihenfolge befindet.
            int index =
                job.Operations.FindIndex(op =>
                    op.OperationID == operation.OperationID);
            // Wenn der Index 0 oder kleiner ist, bedeutet dies, dass die gegebene Operation die erste Operation im Job ist, und es gibt keinen Vorgänger.
            if (index <= 0)
            {
                return null;
            }
            // Andernfalls geben wir die Operation zurück, die sich direkt vor der gegebenen Operation in der Jobreihenfolge befindet.
            return job.Operations[index - 1];
        }
        //Die Methode GetMachinePredecessor bekommt eine Dictionary, die die Reihenfolge der Operationen auf den Maschinen repräsentiert, und eine Operation.
        //Sie gibt die Vorgängeroperation zurück, die in der Maschinenreihenfolge vor der gegebenen Operation liegt.
        //Wenn die gegebene Operation die erste Operation auf der Maschine ist, wird null zurückgegeben.
        public static Operation GetMachinePredecessor(
            Dictionary<int, List<Operation>> machineOrders,
            Operation operation)
        {
            // Finde die Liste der Operationen, die auf der gleichen Maschine wie die gegebene Operation ausgeführt werden.
            List<Operation> operationsOnMachine =
                machineOrders[operation.Machine];
            // Finde den Index der gegebenen Operation in der Liste der Operationen auf der Maschine.
            int index =
                operationsOnMachine.IndexOf(operation);
            // Wenn der Index 0 oder kleiner ist, bedeutet dies, dass die gegebene Operation die erste Operation auf der Maschine ist, und es gibt keinen Vorgänger.
            if (index <= 0)
            {
                return null;
            }
            // Andernfalls geben wir die Operation zurück, die sich direkt vor der gegebenen Operation in der Maschinenreihenfolge befindet.
            return operationsOnMachine[index - 1];
        }
    }
}