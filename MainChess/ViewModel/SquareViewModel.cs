using MainChess.Model;
using System;
using System.Windows.Input;

namespace MainChess.ViewModel;

public class SquareViewModel : BaseViewModel
{
    public int X { get; init; }
    public int Y { get; init; }
    public IBoardPiece? Piece { get; private set; }
    public void Promote(Model.Type type) => Piece.Promote(type);
    public ICommand ClickedCommand { get; }

    private Model.Color? Color = null;
    private readonly BoardViewModel _boardViewModel;
    private void Clicked() => _boardViewModel.HandleClick(this);
    public SquareViewModel(BoardViewModel boardViewModel)
    {
        _boardViewModel = boardViewModel;
        ClickedCommand = new CommandHandler(Clicked);
    }
    public SquareViewModel(int x, int y, BoardViewModel boardViewModel) : this(boardViewModel)
    {
        if (x < 0 || x > 7 || y < 0 || y > 7) throw new ArgumentOutOfRangeException();
        Piece = null; X = x; Y = y;
        Color = (X + Y) % 2 == 0 ? Model.Color.White : Model.Color.Black;
    }
    public SquareViewModel(IBoardPiece boardPiece, BoardViewModel boardViewModel) : this(boardViewModel)
    {
        Piece = boardPiece;
        X = boardPiece.Position.X;
        Y = boardPiece.Position.Y;
        Color = (X + Y) % 2 == 0 ? Model.Color.White : Model.Color.Black;
    }

    public void RemovePiece()
    {
        Piece = null;
        OnPropertyChanged(nameof(PieceImage));
    }
    public void AddPiece(IPiece piece)
    {
        Piece = new BoardPiece(piece, new Position(X, Y));
        OnPropertyChanged(nameof(PieceImage));
    }
    public void RemoveColor()
    {
        Color = null;
        OnPropertyChanged(nameof(ColorImage));
    }
    public void AddColor()
    {
        Color = (X + Y) % 2 == 0 ? Model.Color.White : Model.Color.Black;
        OnPropertyChanged(nameof(ColorImage));
    }
    public string ColorImage => Color.HasValue ? Color?.ToString().ToLower() + ".png" : "empty.png";
    public string PieceImage => Piece is not null ? Piece?.ToString().ToLower().Replace(' ', '_')  + ".png" : "empty.png";
    public string DestinationImage => _isDestination ? ((Piece is null ? "empty" : "full") + "_destination.png") : "empty.png";
    public bool IsSelected
    {
        get { return _isSelected; }
        set
        {
            if (value != _isSelected)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }
    private bool _isSelected;
    public bool IsDestination
    {
        get { return _isDestination; }
        set
        {
            if (value != _isDestination)
            {
                _isDestination = value;
                OnPropertyChanged(nameof(DestinationImage));
            }
        }
    }
    private bool _isDestination;
}