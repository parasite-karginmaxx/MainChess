﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainChess.Model
{
    public enum RookKind
    {
        Royal,
        Queen
    }
    public class Rook : IPiece
    {
        /// <summary>
        /// обозначения вражеских фигур
        /// </summary>
        private string pieces;
        /// <summary>
        /// Обозначения своих фигур
        /// </summary>
        private string myPieces;

        public RookKind RookKind { get; }
        public bool IsMoved { get; set; }
        public PieceColor Color { get; set; }
        public (int, int) Position { get; set; }
        public bool IsDead { get; set; }

        /*направление поиска фигуры 
         *         (будет прибавлять первую координату к первой координате позиции фигуры
         *          и вторую ко второй по двумерному массиву (игрового поля))
         */
        private readonly (int, int) North = (0, 1);
        private readonly (int, int) South = (0, -1);
        private readonly (int, int) West = (1, 0);
        private readonly (int, int) East = (-1, 0);

        //условия для поиска фигуры в массиве(игровом поле)
        private readonly Func<int, int, bool> NorthCondition = (int i, int j) => j < 8;
        private readonly Func<int, int, bool> SouthCondition = (int i, int j) => j > -1;
        private readonly Func<int, int, bool> WestCondition = (int i, int j) => i < 8;
        private readonly Func<int, int, bool> EastCondition = (int i, int j) => i > -1;

        /// <summary>
        /// Проверяет доступные для ладьи ходы в 4 направлениях
        /// </summary>
        /// <param name="GameField"></param>
        /// <returns>Список координат доступных для хода клеток</returns>
        public List<(int, int)> AvailableMoves(string[,] GameField)
        {

            var AvailableMovesList = new List<(int, int)>();
            //Север
            AvailableMovesInDirection(North, GameField, AvailableMovesList, NorthCondition);
            //Юг
            AvailableMovesInDirection(South, GameField, AvailableMovesList, SouthCondition);
            //Запад
            AvailableMovesInDirection(West, GameField, AvailableMovesList, WestCondition);
            //восток
            AvailableMovesInDirection(East, GameField, AvailableMovesList, EastCondition);
            return AvailableMovesList;
        }
        /// <summary>
        /// Ищет доступные клетки для хода
        /// </summary>
        /// <param name="Direction">Направление поиска</param>
        /// <param name="GameField">Игровое поле</param>
        /// <param name="AvailableMovesList">Список доступных ходов (сюда добавляются вновь найденные доступные клетки для хода(координаты))</param>
        /// <param name="Condition">условие для цикла (зависит от направления)</param>
        private void AvailableMovesInDirection((int, int) Direction, string[,] GameField, List<(int, int)> AvailableMovesList, Func<int, int, bool> Condition)
        {
            for (int j = Position.Item2 + Direction.Item2, i = Position.Item1 + Direction.Item1; Condition(i, j); j += Direction.Item2, i += Direction.Item1)
            {
                //если клетка не пустая, то пойти на нее нельзя
                if (GameField[i, j] != " ")
                {
                    break;
                }
                AvailableMovesList.Add((i, j));
            }
        }

        public override string ToString()
        {
            return Color == PieceColor.White ? "R" : "r";
        }
        /// <summary>
        /// Ищет вражеские фигуры для атаки
        /// </summary>
        /// <param name="direction">Направление поиска (пара чисел прибавляется к координатам фигуры)</param>
        /// <param name="GameField">Игровое поле</param>
        /// <param name="AvailableKillsList">Список вражеских фигур для атаки, куда добавляются вновь найденные вражеские фигуры</param>
        /// <param name="Condition">Условие для цикла for (разное в зависимости от направления поиска)</param>
        private void AvailableKillsInDirection((int, int) direction, string[,] GameField, List<(int, int)> AvailableKillsList, Func<int, int, bool> Condition)
        {
            for (int i = Position.Item1 + direction.Item1, j = Position.Item2 + direction.Item2; Condition(i, j); i += direction.Item1, j += direction.Item2)
            {
                ///если уперлись в свою фигуру, то съесть никого нельзя
                if (myPieces.Contains(GameField[i, j]))
                {
                    break;
                }
                else if (pieces.Contains(GameField[i, j]))
                {
                    AvailableKillsList.Add((i, j));
                    break;
                }
            }
        }
        /// <summary>
        /// Поиск вражеских фигур для атаки
        /// </summary>
        /// <param name="GameField">игровое поле</param>
        /// <returns>Список координат вражеских фигур для атаки</returns>
        public List<(int, int)> AvailableKills(string[,] GameField)
        {
            var AvailableKillsList = new List<(int, int)>();

            SetOppositeAndFreindlyPieces();

            //Север
            AvailableKillsInDirection(North, GameField, AvailableKillsList, NorthCondition);
            //Юг
            AvailableKillsInDirection(South, GameField, AvailableKillsList, SouthCondition);
            //Запад
            AvailableKillsInDirection(West, GameField, AvailableKillsList, WestCondition);
            //восток
            AvailableKillsInDirection(East, GameField, AvailableKillsList, EastCondition);
            return AvailableKillsList;
        }
        /// <summary>
        /// Устанавливает вражеские и свои фигуры
        /// </summary>
        private void SetOppositeAndFreindlyPieces()
        {
            if (Color == PieceColor.White)
            {
                pieces = "kbnpqr";
                myPieces = "KBNPQR";
            }
            else
            {
                pieces = "KBNPQR";
                myPieces = "kbnpqr";
            }
        }
        public void ChangePosition((int, int) NewPosition)
        {
            Position = NewPosition;
        }

        public object Clone()
        {
            return new Rook(Position, Color);
        }

        public Rook((int, int) Position, PieceColor color)
        {
            this.Position = Position;
            Color = color;
            IsDead = false;
            IsMoved = false;

            if (Position.Item1 == 0)
            {
                RookKind = RookKind.Queen;
            }
            else
            {
                RookKind = RookKind.Royal;
            }
        }
    }
}
