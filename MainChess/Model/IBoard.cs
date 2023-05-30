using System.Collections.Generic;

namespace MainChess.Model;

public interface IBoard
{
    public Color Turn { get; }
    IBoardPiece? GetPiece(int x , int y);
    Color? GetPieceColor(Position position);
    bool IsEmpty(Position position);
    bool IsCheck(Color color);
    IEnumerable<Position> GetDestinationsOf(Position source);
    bool AddMove(Move move);

}