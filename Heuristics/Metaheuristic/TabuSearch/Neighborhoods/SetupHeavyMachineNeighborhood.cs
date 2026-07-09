using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core;
using JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Criticality;
using JobShopSchedulingFramework.Models;
using System.Collections.Generic;
using System.Linq;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Neighborhoods
{
    /*
    Diese Nachbarschaft erzeugt Moves auf Basis hoher Setup-Zeiten.

    Idee:
    Wenn zwei direkt aufeinanderfolgende Operationen auf einer Maschine hohe
    reihenfolgeabhängige Rüstzeiten verursachen, kann ein Tausch in diesem Bereich
    den Makespan verbessern.

    Zusätzlich werden Moves innerhalb kritischer Blöcke ergänzt. Dadurch kombiniert
    diese Nachbarschaft zwei Informationen:
    - hohe Setup-Zeiten auf Maschinen,
    - kritische Operationen, die für den aktuellen Cmax relevant sind.

    Diese Nachbarschaft ist experimentell und kann verwendet werden, um zu prüfen,
    ob setup-orientierte Moves gegenüber rein kritischen Block-Moves Vorteile bringen.
    */
    public class SetupHeavyMachineNeighborhood : INeighborhoodDefinition
    {
        private readonly int maxMovesPerMachine;

        public SetupHeavyMachineNeighborhood()
        {
            /*
            Begrenzung der Anzahl setup-basierter Moves pro Maschine.

            Ohne diese Begrenzung könnten auf großen Instanzen sehr viele Moves
            entstehen. Das würde die Laufzeit erhöhen, bevor überhaupt die exakte
            Move-Bewertung beginnt.
            */
            maxMovesPerMachine = 5;
        }

        public List<Move> GenerateMoves(
            Instance instance,
            Dictionary<int, List<Operation>> machineOrders,
            List<CriticalBlock> criticalBlocks)
        {
            List<Move> moves =
                new List<Move>();

            /*
            Zuerst werden alle Maschinen betrachtet. Für jede Maschine werden die
            Positionen mit den höchsten Setup-Zeiten gesucht. In der Umgebung dieser
            Positionen werden lokale Swap-Moves erzeugt.
            */
            foreach (var pair in machineOrders)
            {
                int machine =
                    pair.Key;

                List<Operation> operations =
                    pair.Value;

                List<(int Index, int SetupCost)> setupPositions =
                    FindHighSetupPositions(
                        instance,
                        operations);

                int addedMoves =
                    0;

                foreach (var setupPosition in setupPositions)
                {
                    if (addedMoves >= maxMovesPerMachine)
                    {
                        break;
                    }

                    int index =
                        setupPosition.Index;

                    /*
                    Die problematische Setup-Kante liegt zwischen index und index + 1.
                    Deshalb werden Moves in der direkten Umgebung dieser Kante erzeugt.
                    */
                    AddAdjacentMove(
                        machine,
                        operations,
                        index,
                        moves);

                    AddMoveWithPrevious(
                        machine,
                        operations,
                        index,
                        moves);

                    AddMoveWithNext(
                        machine,
                        operations,
                        index + 1,
                        moves);

                    addedMoves++;
                }
            }

            /*
            Zusätzlich werden Moves innerhalb kritischer Blöcke erzeugt.
            Diese Moves sind besonders relevant, weil kritische Operationen direkt
            mit dem aktuellen Cmax zusammenhängen.
            */
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

            /*
            Für jede direkte Nachbarschaft auf der Maschine wird die Setup-Zeit
            zwischen den zugehörigen Jobs bestimmt.
            */
            for (int i = 0; i < operations.Count - 1; i++)
            {
                Operation before =
                    operations[i];

                Operation after =
                    operations[i + 1];

                int setupCost =
                    instance.SetupTimes[
                        before.JobID - 1,
                        after.JobID - 1];

                setupPositions.Add(
                    (i, setupCost));
            }

            /*
            Die höchsten Setup-Zeiten werden zuerst betrachtet, weil sie am ehesten
            Verbesserungspotenzial bieten.
            */
            return setupPositions
                .OrderByDescending(item => item.SetupCost)
                .ToList();
        }

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
            /*
            Innerhalb eines kritischen Blocks werden paarweise Swaps erzeugt.
            Die lokalen Blockindizes müssen auf die Indizes der vollständigen
            Maschinenreihenfolge umgerechnet werden.
            */
            foreach (CriticalBlock block in criticalBlocks)
            {
                for (int i = 0; i < block.Operations.Count - 1; i++)
                {
                    for (int j = i + 1; j < block.Operations.Count; j++)
                    {
                        AddMove(
                            block.Machine,
                            block.Operations,
                            block.StartIndexInMachine,
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
            /*
            Doppelte Moves und direkte Rückmoves werden vermieden, damit dieselbe
            Nachbarschaft nicht mehrfach bewertet wird.
            */
            foreach (Move existingMove in moves)
            {
                if (existingMove.GetKey() == newMove.GetKey() ||
                    existingMove.GetReverseKey() == newMove.GetKey())
                {
                    return;
                }
            }

            moves.Add(
                newMove);
        }
    }
}