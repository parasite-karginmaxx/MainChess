﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainChess.Model
{
    public class Bishop : IPiece
    {
        public PieceColor Color { get; set; }
        public (int, int) Position { get; set; }
        public bool IsDead { get; set; }

        //направление поиска фигуры (будет прибавлять первую координату к первой координате позиции фигуры и вторую ко второй) по двумерному массиву (игрового поля)
        private readonly (int, int)[] Directions = new (int, int)[] { (1, 1), (-1, 1), (1, -1), (-1, -1) };

        //условия для поиска фигуры в массиве(игровом поле)
        private readonly Func<int, int, bool>[] Conditions = new Func<int, int, bool>[]
        {
            (int i, int j) => i < 8 & j < 8,//NorthEast
            (int i, int j) => i > -1 & j < 8,//NorthWest
            (int i, int j) => i < 8 & j > -1,//SouthEast
            (int i, int j) => i > -1 & j > -1//SouthWest
        };

        /// <summary>
        /// Проверка доступных ходов для слона в четырех направлениях
        /// </summary>
        /// <param name="GameField"></param>
        /// <returns>Список координат свободных для хода клеток</returns>
        public List<(int, int)> AvailableMoves(string[,] GameField)
        {
            var AvailableMovesList = new List<(int, int)>();

            for (int i = 0; i < 4; i++)
            {
                AvailableMovesInDirection(Directions[i], GameField, AvailableMovesList, Conditions[i]);
            }

            return AvailableMovesList;
        }
        /// <summary>
        /// Проверяет доступные клетки для хода в определенном направлении
        /// </summary>
        /// <param name="Direction">Заданное направление (пара чисел, которая будет прибавляться к координатам фигуры)</param>
        /// <param name="GameField">Игровое поле</param>
        /// <param name="AvailableMovesList">Список доступных для хода клеток</param>
        /// <param name="Condition">условие для цикла for (разное в зависимости от выбранного направления)</param>
        private void AvailableMovesInDirection((int, int) Direction, string[,] GameField, List<(int, int)> AvailableMovesList, Func<int, int, bool> Condition)
        {
            for (int i = Position.Item1 + Direction.Item1, j = Position.Item2 + Direction.Item2; Condition(i, j); i += Direction.Item1, j += Direction.Item2)
            {
                //если клетка не пустая, то сделать на нее ход нельзя
                if (GameField[i, j] != " ")
                {
                    break;
                }
                AvailableMovesList.Add((i, j));
            }
        }

        public override string ToString()
        {
            return Color == PieceColor.White ? "B" : "b";
        }

        /// <summary>
        /// Вражеские фигуры
        /// </summary>
        string pieces;
        /// <summary>
        /// Свои фигуры
        /// </summary>
        string myPieces;

        /// <summary>
        /// Поиск вражеских фигур, которые можно съесть в одном из четырех направлений
        /// </summary>
        /// <param name="direction">направление</param>
        /// <param name="GameField">игровое поле</param>
        /// <param name="AvailableKillsList">список фигур, которые можно съесть (сюда добавляем вновь найденную фигуру, которую можно съесть)</param>
        /// <param name="func">условия для направления поиска фигур в массиве(игровом поле)</param>
        private void AvailableKillsInDirection((int, int) direction, string[,] GameField, List<(int, int)> AvailableKillsList, Func<int, int, bool> func)
        {
            for (int i = Position.Item1 + direction.Item1, j = Position.Item2 + direction.Item2; func(i, j); i += direction.Item1, j += direction.Item2)
            {
                ///если уперлись в свою фигуру, то съесть никого нельзя
                if (myPieces.Contains(GameField[i, j]))
                {
                    break;
                }
                else//если уперлись во вражескую фигуру, то можем ее съесть
                      if (pieces.Contains(GameField[i, j]))
                {

                    AvailableKillsList.Add((i, j));
                    break;
                }
            }
        }
        /// <summary>
        /// Поиск вражеских фигур, которые можно съесть
        /// </summary>
        /// <param name="GameField">Игровое поле</param>
        /// <returns>Список координат вражеских фигур для атаки</returns>
        public List<(int, int)> AvailableKills(string[,] GameField)
        {
            List<(int, int)> AvailableKillsList = new();
            SetOppositeAndFreindPieces();
            for (int i = 0; i < 4; i++)
            {
                AvailableKillsInDirection(Directions[i], GameField, AvailableKillsList, Conditions[i]);
            }

            return AvailableKillsList;
        }

        private void SetOppositeAndFreindPieces()
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
        public void ChangePosition((int, int) Position)
        {
            this.Position = Position;
        }

        public object Clone()
        {
            return new Bishop(this.Position, this.Color);
        }

        public Bishop((int, int) startPosition, PieceColor Color)
        {
            this.Color = Color;
            Position = startPosition;
            IsDead = false;
        }
    }
}
