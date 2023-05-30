using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MainChess.Model;
public class Board : IBoard
{
    public Color Turn { get; private set; }
    private List<BoardPiece> Pieces;
    private bool WhiteKingside = false;
    private bool WhiteQueenside = false;
    private bool BlackKingside = false;
    private bool BlackQueenside = false;
    private Position? EnPassant = null;

    public Board() => InitializeBoard();
    protected Board(IEnumerable<IBoardPiece> pieces, Color turn, Position? enPassant
        , bool whiteKingside, bool whiteQueenside, bool blackKingside, bool blackQueenside)
    {
        Turn = turn;
        WhiteKingside = whiteKingside;
        WhiteQueenside = whiteQueenside;
        BlackKingside = blackKingside;
        BlackQueenside = blackQueenside;
        EnPassant = enPassant;
        Pieces = new();
        foreach (var item in pieces)
            Pieces.Add(new(item.Color, item.Type, item.Position.X, item.Position.Y));
    }

    public IBoardPiece? GetPiece(int x, int y) => GetPiece(new(x, y));
    private BoardPiece? GetPiece(Position position) => Pieces.FirstOrDefault(x => x.Position.Equals(position));
    public Color? GetPieceColor(Position position) => GetPiece(position)?.Color;
    public bool IsEmpty(Position position) => GetPiece(position) == null;
    public bool IsCheck(Color color)
    {
        var king = GetKing(color);
        return Check(color, king.Position);
    }
    public IEnumerable<Position> GetDestinationsOf(Position source)
    {
        var sourcePiece = GetPiece(source) ?? throw new InvalidOperationException($"{nameof(source)} dosen't have any piece");
        foreach (var item in sourcePiece.GetDestinations(this))
            if (!IsCheckAfterMove(new(sourcePiece.Position, item)))
                yield return item;
        if (sourcePiece.Type == Type.King)
        {
            if (CanKingsideCastle(sourcePiece.Color))
                yield return new(6, sourcePiece.Color.IsWhite() ? 0 : 7);
            if (CanQueensideCastle(sourcePiece.Color))
                yield return new(2, sourcePiece.Color.IsWhite() ? 0 : 7);
        }
        if (sourcePiece.Type == Type.Pawn && EnPassant.HasValue
            && Math.Abs(EnPassant.Value.Y - sourcePiece.Position.Y) == 1
            && Math.Abs(EnPassant.Value.X - sourcePiece.Position.X) == 1)
            yield return EnPassant.Value;
    }
    public bool AddMove(Move move)
    {
        try
        {
            if (!GetDestinationsOf(move.Source).Contains(move.Destination)) 
                return false;
            DoMove(move);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void InitializeBoard()
    {
        Turn = Color.White;
        Pieces = new()
        {
            new(Color.White, Type.King, 4, 0) ,
            new(Color.Black, Type.King, 4, 7),
            new(Color.White, Type.Queen, 3, 0),
            new(Color.Black, Type.Queen, 3, 7),
            new(Color.White, Type.Rook, 0, 0),
            new(Color.White, Type.Rook, 7, 0),
            new(Color.Black, Type.Rook, 0, 7),
            new(Color.Black, Type.Rook, 7, 7),
            new(Color.White, Type.Knight, 1, 0),
            new(Color.White, Type.Knight, 6, 0),
            new(Color.Black, Type.Knight, 1, 7),
            new(Color.Black, Type.Knight, 6, 7),
            new(Color.White, Type.Bishop, 2, 0),
            new(Color.White, Type.Bishop, 5, 0),
            new(Color.Black, Type.Bishop, 2, 7),
            new(Color.Black, Type.Bishop, 5, 7)
        };
        for (int i = 0; i < 8; i++) Pieces.Add(new(Color.White, Type.Pawn, i, 1));
        for (int i = 0; i < 8; i++) Pieces.Add(new(Color.Black, Type.Pawn, i, 6));
    }
    private bool IsCheckAfterMove(Move move)
    {
        var source = GetPiece(move.Source)!;
        var boardClone = new Board(Pieces, Turn, EnPassant, WhiteKingside, WhiteQueenside, BlackKingside, BlackQueenside);
        boardClone.DoMove(move);
        return boardClone.IsCheck(source.Color);
    }
    private void DoMove(Move move)
    {
        var sourcePiece = GetPiece(move.Source)!;
        var destinationPiece = GetPiece(move.Destination);
        if (destinationPiece is not null)
            Pieces.Remove(destinationPiece);
        if (sourcePiece.Type == Type.King && move.Source.X == 4)
        {
            var y = sourcePiece.Color.IsWhite() ? 0 : 7;
            if (move.Destination.X == 6)
                GetPiece(new(7, y)).MoveTo(new(5, y));
            if (move.Destination.X == 2)
                GetPiece(new(0, y)).MoveTo(new(3, y));
        }
        if (sourcePiece.Type == Type.Pawn && move.Destination == EnPassant)
        {
            var opponentPawn = GetPiece(new(EnPassant.Value.X, move.Source.Y));
            Pieces.Remove(opponentPawn);
        }
        UpdateCastleMovedStates(sourcePiece);
        EnPassant = sourcePiece.Type == Type.Pawn && Math.Abs(move.Source.Y - move.Destination.Y) == 2
            ? new(move.Source.X, (move.Source.Y + move.Destination.Y) / 2) : null;

        sourcePiece.MoveTo(move.Destination);
        if (move.PromotionType.HasValue)
            sourcePiece.Promote(move.PromotionType.Value);
        Turn = Turn.IsWhite() ? Color.Black : Color.White;
    }
    private bool Check(Color color, Position position)
    {
        var opponentColor = color.IsWhite() ? Color.Black : Color.White;
        var opponentPices = GetAllPice(opponentColor);
        foreach (var piece in opponentPices)
        {
            var destinations = piece.GetDestinations(this);
            if (destinations.Contains(position))
                return true;
        }
        return false;
    }
    private BoardPiece GetKing(Color color) => Pieces.First(x => x.Color == color && x.Type == Type.King);
    private IEnumerable<BoardPiece> GetAllPice(Color color) => Pieces.Where(x => x.Color == color);
    private void UpdateCastleMovedStates(BoardPiece sourcePiece)
    {
        if (sourcePiece.Type == Type.Rook)
        {
            if (sourcePiece.Position.Y == 0)
            {
                if (sourcePiece.Position.X == 7)
                    WhiteKingside = true;
                if (sourcePiece.Position.X == 0)
                    WhiteQueenside = true;
            }
            if (sourcePiece.Position.Y == 7)
            {
                if (sourcePiece.Position.X == 7)
                    BlackKingside = true;
                if (sourcePiece.Position.X == 0)
                    BlackQueenside = true;
            }
        }
        if (sourcePiece.Type == Type.King)
        {
            if (sourcePiece.Color.IsWhite())
            {
                WhiteKingside = true;
                WhiteQueenside = true;
            }
            else
            {
                BlackKingside = true;
                BlackQueenside = true;
            }
        }
    }
    private bool CanKingsideCastle(Color color)
    {
        if (IsCheck(color))
            return false;
        if (color.IsWhite() && WhiteKingside)
            return false;
        if (color.IsBlack() && BlackKingside)
            return false;
        return IsKingsidePathOpen(color);
    }
    private bool CanQueensideCastle(Color color)
    {
        if (IsCheck(color))
            return false;
        if (color.IsWhite() && WhiteQueenside)
            return false;
        if (color.IsBlack() && BlackQueenside)
            return false;
        return IsQueensidePathOpen(color);
    }
    private bool IsKingsidePathOpen(Color color)
    {
        var y = color.IsWhite() ? 0 : 7;
        return IsEmpty(new(5, y)) && IsEmpty(new(6, y))
            && !Check(color, new(5, y)) && !Check(color, new(6, y));
    }
    private bool IsQueensidePathOpen(Color color)
    {
        var y = color.IsWhite() ? 0 : 7;
        return IsEmpty(new(1, y)) && IsEmpty(new(2, y)) && IsEmpty(new(3, y))
            && !Check(color, new(3, y)) && !Check(color, new(2, y));
    }
}