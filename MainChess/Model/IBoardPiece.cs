namespace MainChess.Model;

public interface IBoardPiece : IPiece
{
    Position Position { get; }
    void MoveTo(Position position);
    void Promote(Type type);
}