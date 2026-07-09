using JobShopSchedulingFramework.Models;
using System.Collections.Generic;

namespace JobShopSchedulingFramework.Heuristics.Metaheuristic.TabuSearch.Core
{
    /*
    Wendet einen Move auf eine Maschinenreihenfolge an.

    Die Tabu Search verändert nicht direkt die Start- und Endzeiten der Operationen.
    Stattdessen wird zuerst nur die Reihenfolge der Operationen auf einer Maschine
    geändert. Aus dieser neuen Maschinenreihenfolge wird danach ein vollständiger
    Schedule neu berechnet.

    Dadurch bleibt die Suchstruktur einfach:
    - Swap-Move: Zwei Operationen auf derselben Maschine werden vertauscht.
    - Insert-Move: Eine Operation wird aus ihrer Position entfernt und an anderer
      Stelle auf derselben Maschine eingefügt.
    */
    public static class MoveApplier
    {
        public static void Apply(
            Dictionary<int, List<Operation>> machineOrders,
            Move move)
        {
            List<Operation> operationsOnMachine =
                machineOrders[move.Machine];

            /*
            Insert-Moves und Swap-Moves verändern die Maschinenreihenfolge
            unterschiedlich. Deshalb werden sie getrennt behandelt.
            */
            if (move.IsInsertMove)
            {
                ApplyInsertMove(
                    operationsOnMachine,
                    move);

                return;
            }

            ApplySwapMove(
                operationsOnMachine,
                move);
        }

        private static void ApplySwapMove(
            List<Operation> operationsOnMachine,
            Move move)
        {
            /*
            Beim Swap-Move werden zwei Positionen auf derselben Maschine vertauscht.
            Die Operationen selbst bleiben unverändert, nur ihre Reihenfolge auf der
            Maschine ändert sich.
            */
            Operation temp =
                operationsOnMachine[move.MachineIndex1];

            operationsOnMachine[move.MachineIndex1] =
                operationsOnMachine[move.MachineIndex2];

            operationsOnMachine[move.MachineIndex2] =
                temp;
        }

        private static void ApplyInsertMove(
            List<Operation> operationsOnMachine,
            Move move)
        {
            /*
            Beim Insert-Move wird eine Operation aus der Maschinenreihenfolge entfernt
            und an einer anderen Position wieder eingefügt. Dadurch können größere
            Änderungen als bei einem einfachen Swap entstehen.
            */
            Operation movedOperation =
                operationsOnMachine[move.MachineIndex1];

            operationsOnMachine.RemoveAt(
                move.MachineIndex1);

            int targetIndex =
                move.MachineIndex2;

            /*
            Wenn die Operation nach rechts verschoben wird, verändert das Entfernen
            der Operation die Indizes der nachfolgenden Elemente.

            Beispiel:
            Liste: [A, B, C, D]
            B wird von Index 1 nach Index 3 verschoben.
            Nach RemoveAt(1): [A, C, D]
            Der ursprüngliche Zielindex 3 wäre jetzt eine Position zu weit rechts.
            Deshalb wird targetIndex um 1 reduziert.
            */
            if (move.MachineIndex1 < move.MachineIndex2)
            {
                targetIndex--;
            }

            /*
            Sicherheitsprüfung:
            Der Zielindex muss innerhalb der aktuellen Liste liegen.
            Dadurch vermeiden wir ungültige Insert-Positionen.
            */
            if (targetIndex < 0)
            {
                targetIndex = 0;
            }

            if (targetIndex > operationsOnMachine.Count)
            {
                targetIndex = operationsOnMachine.Count;
            }

            operationsOnMachine.Insert(
                targetIndex,
                movedOperation);
        }
    }
}