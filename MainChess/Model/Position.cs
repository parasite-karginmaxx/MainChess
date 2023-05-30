using System;
using System.IO;

namespace MainChess.Model;
public readonly struct Position
{
    public int X { get; init; }
    public int Y { get; init; }
    public Position(int x, int y)
    {
        if (x < 0 || x > 7 || y < 0 || y > 7) throw new ArgumentOutOfRangeException();
        X = x; Y = y;
    }
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (obj is not Position) return false;
        var position = (Position)obj;
        return X == position.X && Y == position.Y;
    }
    public override string ToString() => XToChar(X) + Y.ToString();
    public override int GetHashCode() => X + (Y * 8);
    public static char XToChar(int x) => x switch
    {
        0 => 'a',
        1 => 'b',
        2 => 'c',
        3 => 'd',
        4 => 'e',
        5 => 'f',
        6 => 'g',
        7 => 'h',
        _ => throw new ArgumentOutOfRangeException($"x is not in rang 0 - 7  x = {x}"),
    };
    public static bool operator ==(Position left, Position right) => left.Equals(right);
    public static bool operator !=(Position left, Position right) => !(left == right);

    public static Position? Create(int x, int y) 
    {
        if (x < 0 || x > 7 || y < 0 || y > 7) return null;
        return new(x,y);
    }
}