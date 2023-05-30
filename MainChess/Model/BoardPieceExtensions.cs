using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MainChess.Model;

public static class BoardPieceExtensions
{
    public static IEnumerable<Position> GetDestinations(this BoardPiece source, IBoard board) => source.Type switch
    {
        Type.Pawn => GetPownDestinations(source, board),
        Type.Knight => GetKnightDestinations(source, board),
        Type.Bishop => GetBishopDestinations(source, board),
        Type.Rook => GetRookDestinations(source, board),
        Type.Queen => GetQueenDestinations(source, board),
        Type.King => GetKingDestinations(source, board),
        _ => throw new InvalidDataException(),
    };

    private static IEnumerable<Position> GetKingDestinations(BoardPiece source, IBoard board)
    {
        var x = source.Position.X; var y = source.Position.Y;
        List<Position?> all = new List<Position?>
        {
            Position.Create(x + 1, y),
            Position.Create(x - 1, y),
            Position.Create(x , y + 1),
            Position.Create(x , y - 1),
            Position.Create(x + 1, y + 1),
            Position.Create(x + 1, y - 1),
            Position.Create(x - 1, y + 1),
            Position.Create(x - 1, y - 1),
        };
        foreach (var item in all)
            if (item.HasValue && (board.IsEmpty(item.Value) || board.GetPieceColor(item.Value) != source.Color))
                yield return item.Value;
    }
    private static IEnumerable<Position> GetQueenDestinations(BoardPiece source, IBoard board)
    {
        foreach (var item in GetRookDestinations(source, board))
            yield return item;
        foreach (var item in GetBishopDestinations(source, board))
            yield return item;
    }
    private static IEnumerable<Position> GetRookDestinations(BoardPiece source, IBoard board)
    {
        foreach (var item in GoUp(source.Color, source.Position, board))
            yield return item;
        foreach (var item in GoDown(source.Color, source.Position, board))
            yield return item;
        foreach (var item in GoRight(source.Color, source.Position, board))
            yield return item;
        foreach (var item in GoLeft(source.Color, source.Position, board))
            yield return item;
    }
    private static IEnumerable<Position> GetBishopDestinations(BoardPiece source, IBoard board)
    {
        foreach (var item in GoUpRight(source.Color, source.Position, board))
            yield return item;
        foreach (var item in GoUpLeft(source.Color, source.Position, board))
            yield return item;
        foreach (var item in GoDownRight(source.Color, source.Position, board))
            yield return item;
        foreach (var item in GoDownLeft(source.Color, source.Position, board))
            yield return item;
    }
    private static IEnumerable<Position> GetKnightDestinations(BoardPiece source, IBoard board)
    {
        var x = source.Position.X; var y = source.Position.Y;
        var all = new List<Position?>
        {
            Position.Create(x+ 1, y + 2),
            Position.Create(x - 1, y + 2),
            Position.Create(x + 2, y + 1),
            Position.Create(x - 2, y + 1),
            Position.Create(x + 1, y - 2),
            Position.Create(x - 1, y - 2),
            Position.Create(x + 2, y - 1),
            Position.Create(x - 2, y - 1)
        };
        foreach (var item in all)
            if (item.HasValue && (board.IsEmpty(item.Value) || board.GetPieceColor(item.Value) != source.Color))
                yield return item.Value;
    }
    private static IEnumerable<Position> GetPownDestinations(BoardPiece source, IBoard board)
    {
        var x = source.Position.X; var y = source.Position.Y + (source.Color.IsWhite() ? 1 : -1);

        var forward = Position.Create(x, y) ?? throw new InvalidDataException();
        if (board.IsEmpty(forward))
        {
            yield return forward;
            if (source.Position.Y == (source.Color.IsWhite() ? 1 : 6))
            {
                var forwardPlus = Position.Create(x, source.Color.IsWhite() ? 3 : 4);
                if (forwardPlus.HasValue && board.IsEmpty(forwardPlus.Value))
                    yield return forwardPlus.Value;
            }
        }
        foreach (var item in new List<Position?> { Position.Create(x + 1, y), Position.Create(x - 1, y) })
        {
            if (item.HasValue && !board.IsEmpty(item.Value) && board.GetPieceColor(item.Value) != source.Color)
                yield return item.Value;
        }
    }

    private static IEnumerable<Position> GoUp(Color color, Position position, IBoard board)
    {
        var y = position.Y + 1;
        while (y <= 7)
        {
            Position up = new(position.X, y);
            if (board.IsEmpty(up))
            {
                y++;
                yield return up;
            }
            else if (board.GetPieceColor(up) != color)
            {
                yield return up;
                break;
            }
            else break;
        }
    }
    private static IEnumerable<Position> GoDown(Color color, Position position, IBoard board)
    {
        var y = position.Y - 1;
        while (y >= 0)
        {
            Position down = new(position.X, y);
            if (board.IsEmpty(down))
            {
                y--;
                yield return down;
            }
            else if (board.GetPieceColor(down) != color)
            {
                yield return down;
                break;
            }
            else break;
        }
    }
    private static IEnumerable<Position> GoRight(Color color, Position position, IBoard board)
    {
        var x = position.X + 1;
        while (x <= 7)
        {
            Position right = new(x, position.Y);
            if (board.IsEmpty(right))
            {
                x++;
                yield return right;
            }
            else if (board.GetPieceColor(right) != color)
            {
                yield return right;
                break;
            }
            else break;
        }
    }
    private static IEnumerable<Position> GoLeft(Color color, Position position, IBoard board)
    {
        var x = position.X - 1;
        while (x >= 0)
        {
            Position left = new(x, position.Y);
            if (board.IsEmpty(left))
            {
                x--;
                yield return left;
            }
            else if (board.GetPieceColor(left) != color)
            {
                yield return left;
                break;
            }
            else break;
        }
    }
    private static IEnumerable<Position> GoUpRight(Color color, Position position, IBoard board)
    {
        var y = position.Y + 1;
        var x = position.X + 1;
        while (y <= 7 && x <= 7)
        {
            Position upRight = new(x, y);
            if (board.IsEmpty(upRight))
            {
                x++; y++;
                yield return upRight;
            }
            else if (board.GetPieceColor(upRight) != color)
            {
                yield return upRight;
                break;
            }
            else break;
        }
    }
    private static IEnumerable<Position> GoUpLeft(Color color, Position position, IBoard board)
    {
        var y = position.Y + 1;
        var x = position.X - 1;
        while (y <= 7 && x >= 0)
        {
            Position upLeft = new(x, y);
            if (board.IsEmpty(upLeft))
            {
                y++; x--;
                yield return upLeft;
            }
            else if (board.GetPieceColor(upLeft) != color)
            {
                yield return upLeft;
                break;
            }
            else break;
        }
    }
    private static IEnumerable<Position> GoDownRight(Color color, Position position, IBoard board)
    {
        var y = position.Y - 1;
        var x = position.X + 1;
        while (y >= 0 && x <= 7)
        {
            Position downRight = new(x, y);
            if (board.IsEmpty(downRight))
            {
                x++; y--;
                yield return downRight;
            }
            else if (board.GetPieceColor(downRight) != color)
            {
                yield return downRight;
                break;
            }
            else break;
        }
    }
    private static IEnumerable<Position> GoDownLeft(Color color, Position position, IBoard board)
    {
        var Y = position.Y - 1;
        var X = position.X - 1;
        while (Y >= 0 && X >= 0)
        {
            Position downLeft = new(X, Y);
            if (board.IsEmpty(downLeft))
            {
                X--; Y--;
                yield return downLeft;
            }
            else if (board.GetPieceColor(downLeft) != color)
            {
                yield return downLeft;
                break;
            }
            else break;
        }
    }
}
