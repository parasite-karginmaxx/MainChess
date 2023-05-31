﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainChess.Model
{
    public enum PieceColor
    {
        White,
        Black
    }
    public interface IPiece : ICloneable
    {
        /// <summary>
        /// Является ли фигура убитой
        /// </summary>
        public bool IsDead { get; set; }
        /// <summary>
        /// Цвет фигуры
        /// </summary>
        public PieceColor Color { get; set; }
        /// <summary>
        /// Позиция на доске
        /// </summary>
        public (int, int) Position { get; set; }
        /// <summary>
        /// Доступные клетки для хода
        /// </summary>
        /// <param name="GameField"></param>
        /// <returns></returns>
        public List<(int, int)> AvailableMoves(string[,] GameField);
        /// <summary>
        /// Меняет позицию фигуры
        /// </summary>
        /// <param name="Position">Позиция, на которую меняем</param>
        public void ChangePosition((int, int) Position);
        /// <summary>
        /// Проверяет какие фигуры можно съесть
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public List<(int, int)> AvailableKills(string[,] GameField);
    }
}
