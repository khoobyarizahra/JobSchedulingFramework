using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Models;
using System.Reflection.PortableExecutable;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods
{

    // Diese Nachbarschaft erzeugt Moves basierend auf hohen Setupkosten zwischen Jobs auf Maschinen
    // Vertauscht sowohl direkte als auch indirekte Nachbarn
    public class SetupHeavyMachineNeighborhood : INeighborhoodDefinition
    {
        
        private readonly int maxMovesPerMachine;

        public SetupHeavyMachineNeighborhood()
        {
            // Begrenzung zur Laufzeitreduktion
            maxMovesPerMachine = 5;
        }

        public List<Move> GenerateMoves(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders,
            List<CriticalBlock> criticalBlocks)
        {

            // Liste aller möglichen Nachbarschafts-Moves
            List<Move> moves =
                new List<Move>();

            //Maschinenanalyse
            foreach (var pair in machineOrders)
            {
                // Maschinen - ID
                int machine =           
                    pair.Key;
                // Reihenfolge der Operationen
                List<Operation> operations =        

                    pair.Value;
                // Finde Positionen mit hohen Setupkosten
                List<(int Index, int SetupCost)> setupPositions =
                    FindHighSetupPositions(
                        instance,
                        operations);

                int addedMoves =
                    0;
                // Bearbeite nur die längsten Setup-Zeiten (Top 5)
                foreach (var setupPosition in setupPositions)
                {
                    if (addedMoves >= maxMovesPerMachine)
                    {
                        break;
                    }

                    int index =
                        setupPosition.Index;
                    // Lokale Nachbarschaft um Problemstelle
                    // Swap mit nächstem Element
                    AddAdjacentMove(
                        machine,
                        operations,
                        index,
                        moves);

                    // Swap mit vorherigem Element
                    AddMoveWithPrevious(
                        machine,
                        operations,
                        index,
                        moves);
                    // Swap mit nächstem Element (leicht verschoben)
                    AddMoveWithNext(
                        machine,
                        operations,
                        index + 1,
                        moves);

                    addedMoves++;
                }
            }

            //Critical Blockerweiterung
            AddCriticalBlockMoves(
                criticalBlocks,
                moves);

            return moves;
        }

        private List<(int Index, int SetupCost)> FindHighSetupPositions(
            Instance instance,
            List<Operation> operations)
        {
            List<(int Index, int SetupCost)> setupPositions =
                new List<(int Index, int SetupCost)>();

            // Prüfe alle aufeinanderfolgenden Operationen
            for (int i = 0; i < operations.Count - 1; i++)
            {
                Operation before =
                    operations[i];

                Operation after =
                    operations[i + 1];

                // Setupkosten zwischen zwei Jobs
                int setupCost =
                    instance.SetupTimes[
                        before.JobID - 1,
                        after.JobID - 1];

                setupPositions.Add(
                    (i, setupCost));
            }
            // Sortiere nach höchsten Setupkosten zuerst
            setupPositions =
                setupPositions
                    .OrderByDescending(item => item.SetupCost)
                    .ToList();

            return setupPositions;
        }
        // Swap mit Nachfolger
        private void AddAdjacentMove(
            int machine,
            List<Operation> operations,
            int index,
            List<Move> moves)
        {
            if (index < 0 ||
                index >= operations.Count - 1)
            {
                return;
            }

            AddMove(
                machine,
                operations,
                index,
                index + 1,
                moves);
        }
        // Swap mit Vorgänger
        private void AddMoveWithPrevious(
            int machine,
            List<Operation> operations,
            int index,
            List<Move> moves)
        {
            int previousIndex =
                index - 1;

            if (previousIndex < 0)
            {
                return;
            }

            AddMove(
                machine,
                operations,
                previousIndex,
                index,
                moves);
        }
        // Swap mit nächsten Element
        private void AddMoveWithNext(
            int machine,
            List<Operation> operations,
            int index,
            List<Move> moves)
        {
            int nextIndex =
                index + 1;

            if (nextIndex >= operations.Count)
            {
                return;
            }

            AddMove(
                machine,
                operations,
                index,
                nextIndex,
                moves);
        }

        private void AddCriticalBlockMoves(
            List<CriticalBlock> criticalBlocks,
            List<Move> moves)
        {
            foreach (CriticalBlock block in criticalBlocks)
            {
                for (int i = 0; i < block.operations.Count - 1; i++)
                {
                    for (int j = i + 1; j < block.operations.Count; j++)
                    {
                        AddMove(
                            block.machine,
                            block.operations,
                            block.startIndexInMachine,
                            i,
                            j,
                            moves);
                    }
                }
            }
        }

        private void AddMove(
            int machine,
            List<Operation> operations,
            int index1,
            int index2,
            List<Move> moves)
        {
            Operation first =
                operations[index1];

            Operation second =
                operations[index2];

            Move move =
                new Move(
                    machine,
                    index1,
                    index2,
                    first.JobID,
                    first.OperationID,
                    second.JobID,
                    second.OperationID);

            AddIfNotDuplicate(
                moves,
                move);
        }

        private void AddMove(
            int machine,
            List<Operation> blockOperations,
            int blockStartIndex,
            int blockIndex1,
            int blockIndex2,
            List<Move> moves)
        {
            Operation first =
                blockOperations[blockIndex1];

            Operation second =
                blockOperations[blockIndex2];

            Move move =
                new Move(
                    machine,
                    blockStartIndex + blockIndex1,
                    blockStartIndex + blockIndex2,
                    first.JobID,
                    first.OperationID,
                    second.JobID,
                    second.OperationID);

            AddIfNotDuplicate(
                moves,
                move);
        }

        private void AddIfNotDuplicate(
            List<Move> moves,
            Move newMove)
        {
            foreach (Move existingMove in moves)
            {
                if (existingMove.GetKey() == newMove.GetKey() ||
                    existingMove.GetReverseKey() == newMove.GetKey())
                {
                    return;
                }
            }

            moves.Add(newMove);
        }
    }
}