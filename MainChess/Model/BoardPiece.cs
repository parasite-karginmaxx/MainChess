namespace MainChess.Model;

public class BoardPiece : Piece, IBoardPiece
{
    public Position Position { get; set; }
    public BoardPiece(IPiece piece, Position position) : base(piece.Color, piece.Type) => Position = position;
    public BoardPiece(Color color, Type type, int x, int y) : base(color, type) => Position = new(x, y);

    public void MoveTo(Position position) => Position = position;
    public void Promote(Type type) => Type = type;

}