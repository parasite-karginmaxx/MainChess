using MainChess.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Type = MainChess.Model.Type;

namespace MainChess.ViewModel;

public class BoardViewModel : BaseViewModel
{
    public ObservableCollection<SquareViewModel> Squares { get; init; }
    private IBoard Board { get; init; }
    private Color TurnColor { get; set; }
    private IBoardPiece? SourcePiece { get; set; } = null;
    private List<Position> Destinations { get; set; } = new();

    private Move? PromotionMove = null;
    private List<IBoardPiece> RemovedPieces { get; set; } = new();
    public BoardViewModel() : this(new Board()) { }
    public BoardViewModel(IBoard board)
    {
        Board = board;
        TurnColor = board.Turn;
        Squares = new();
        InitializeChessBoard();
    }
    public void HandleClick(SquareViewModel square)
    {
        if (PromotionMove.HasValue)
            SelectPromotion(square);
        else if (SourcePiece is not null && square.IsSelected)
            UnSelectPiece(square);
        else if (SourcePiece is not null && square.Piece?.Color == SourcePiece.Color)
        {
            var sourceSquare = GetSquare(SourcePiece.Position);
            UnSelectPiece(sourceSquare);
            SelectPiece(square);
        }
        else if (SourcePiece is null && square.Piece?.Color == TurnColor)
            SelectPiece(square);
        else if (square.IsDestination)
        {
            Move move = new(SourcePiece.Position, new(square.X, square.Y), null);
            var isPromotion = SourcePiece.Type == Type.Pawn && square.Y == (SourcePiece.Color.IsWhite() ? 7 : 0);
            if (isPromotion)
                ShowPromotions(move);
            else
                AddMove(move);
        }
    }

    private void SelectPiece(SquareViewModel square)
    {
        SourcePiece = square.Piece;
        square.IsSelected = true;
        AddDestinations();
    }
    private void UnSelectPiece(SquareViewModel square)
    {
        ClearDestinatios();
        square.IsSelected = false;
        SourcePiece = null;
    }
    private void SelectPromotion(SquareViewModel square)
    {
        var selectedType = GetType(square);
        Move? move = selectedType is null ? null:
            new(PromotionMove.Value.Source, PromotionMove.Value.Destination, selectedType);
        HidePromotions();
        if (move.HasValue)
            AddMove(move.Value);
    }

    private Type? GetType(SquareViewModel square)
    {
        if (square.Piece is null) return null;
        var promotions = GetPromations(square.Piece.Color, PromotionMove.Value.Destination.X);
        return promotions.FirstOrDefault(p => p.Position == square.Piece.Position)?.Type;
    }

    private void InitializeChessBoard()
    {
        for (int i = 0; i < 64; i++)
        {
            (var x, var y) = GetXY(i);
            var piece = Board.GetPiece(x, y);
            if (piece is null)
                Squares.Add(new SquareViewModel(x, y, this));
            else
                Squares.Add(new SquareViewModel(piece, this));
        }
    }
    private void AddMove(Move move)
    {
        if (Board.AddMove(move))
        {
            var sourceSquare = GetSquare(move.Source);
            sourceSquare.IsSelected = false;
            SourcePiece = null;
            TurnColor = TurnColor == Color.White ? Color.Black : Color.White;
            ClearDestinatios();
            UpdateChessBoard(move);
        }
    }
    private void UpdateChessBoard(Move move)
    {
        var sourceSquare = GetSquare(move.Source);
        var sourcePice = sourceSquare.Piece;
        var destinationSquare = GetSquare(move.Destination);
        if (sourcePice.Type == Type.Pawn && move.Source.X != move.Destination.X && destinationSquare.Piece is null)
        {
            int index = GetIndex(move.Destination.X, move.Source.Y);
            Squares[index].RemovePiece();
        }
        if (move.PromotionType.HasValue)
            sourcePice.Promote(move.PromotionType.Value);

        destinationSquare.AddPiece(sourcePice);
        sourceSquare.RemovePiece();

        if (sourcePice.Type == Type.King && Math.Abs(move.Source.X - move.Destination.X) == 2)
        {
            var rookSquare = GetSquare(new(move.Destination.X == 2 ? 0 : 7, move.Destination.Y));
            var rookDesSquare = GetSquare(new(move.Destination.X == 2 ? 3 : 5, move.Destination.Y));
            var rook = rookSquare.Piece;
            rookDesSquare.AddPiece(rook);
            rookSquare.RemovePiece();
        }
    }
    private static int GetIndex(int x, int y) => x + (7 - y) * 8;
    private static (int x, int y) GetXY(int index) => (index % 8, 7 - (index / 8));
    private SquareViewModel GetSquare(Position position) => Squares[GetIndex(position.X, position.Y)];
    private void AddDestinations()
    {
        foreach (var item in Board.GetDestinationsOf(SourcePiece.Position))
        {
            Destinations.Add(item);
            GetSquare(item).IsDestination = true;
        }
    }
    private void ClearDestinatios()
    {
        foreach (var item in Destinations)
            GetSquare(item).IsDestination = false;
        Destinations.Clear();
    }
    private void ShowPromotions(Move move)
    {
        PromotionMove = move;
        var sourceSquare = GetSquare(move.Source);
        sourceSquare.IsSelected = false;
        var promotions = GetPromations(sourceSquare.Piece.Color, move.Destination.X);
        foreach (var promotion in promotions)
        {
            var square = GetSquare(promotion.Position);
            if (square.Piece is not null)
                RemovedPieces.Add(square.Piece);
            square.AddPiece(promotion);
            square.RemoveColor();
        }
    }
    private void HidePromotions()
    {
        var sourceSquare = GetSquare(PromotionMove.Value.Source);
        sourceSquare.IsSelected = true;
        var promotions = GetPromations(sourceSquare.Piece.Color, PromotionMove.Value.Destination.X);
        foreach (var item in promotions)
        {
            var square = GetSquare(item.Position);
            square.RemovePiece();
            square.AddColor();
        }
        foreach (var item in RemovedPieces)
            GetSquare(item.Position).AddPiece(item);
        RemovedPieces.Clear();
        PromotionMove = null;
    }
    private static IEnumerable<BoardPiece> GetPromations(Color color, int x)
    {
        yield return new(color, Type.Queen, x, color.IsWhite() ? 7 : 0);
        yield return new(color, Type.Knight, x, color.IsWhite() ? 6 : 1);
        yield return new(color, Type.Rook, x, color.IsWhite() ? 5 : 2);
        yield return new(color, Type.Bishop, x, color.IsWhite() ? 4 : 3);
    }
}
