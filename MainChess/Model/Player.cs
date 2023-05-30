﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainChess.Model
{
    public class Player
    {
        /// <summary>
        /// Имя игрока
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Цвет фигур
        /// </summary>
        public PieceColor Color { get; set; }
        /// <summary>
        /// Фигуры
        /// </summary>
        public List<IPiece> Pieces { get; set; }
        /// <summary>
        /// Убитые фигуры
        /// </summary>
        public List<IPiece> KilledPieces { get; set; }
        /// <summary>
        /// Добавить убитую фигуру
        /// </summary>
        /// <param name="piece"></param>
        public void AddKilledPiece(IPiece piece)
        {
            KilledPieces.Add(piece);
        }
        /// <summary>
        /// Убить фигуру
        /// </summary>
        /// <param name="piece"></param>
        public void KillPiece(IPiece piece)
        {
            Pieces.Remove(piece);
        }

        public Player(PieceColor pieceColor, List<IPiece> myPieces, string Name = "user")
        {
            Color = pieceColor;
            Pieces = myPieces;
            this.Name = Name;
            KilledPieces = new List<IPiece>();
        }

    }
}
