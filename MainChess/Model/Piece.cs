namespace MainChess.Model;

public class Piece : IPiece
{
    public Color Color { get; init; }
    public Type Type { get; protected set; }

    public Piece(Color color, Type type)
    {
        Type = type;
        Color = color;
    }
    public override string ToString() => Color + " " + Type;
}