﻿using MainChess.Model;
using MainChess.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace MainChess.ViewModel
{
    class MainWindowViewModel : BaseViewModel
    {
        private const int longCastleVerticalPosition = 2;
        private const int shortCastleVerticalPosition = 6;
        private const string pathOfFenFile = "fen.txt";

        private readonly List<string> colors = new List<string>() { "Белые", "Черные" };

        private Board board;
        private ICommand cellCommand;
        private int currentPlayer;

        private Game game;
        private ObservableCollection<string> moves;
        private ICommand newGameCommand;
        private List<IPiece> pieces;
        private ObservableCollection<string> playerMoves;
        private List<Player> players;

        public MainWindowViewModel()
        {
            Board = new Board();
        }

        public Board Board
        {
            get => board;
            set
            {
                board = value;
                OnPropertyChanged();
            }
        }

        public ICommand CellCommand => cellCommand ??= new RelayCommand(parameter =>
        {

            Cell CurrentCell = (Cell)parameter;

            Cell PreviousActiveCell = Board.FirstOrDefault(x => x.Active);

            List<(int, int)> ValidMoves = new();

            List<(int, int)> ValidAttacks = new();


            if (IsCheck())
            {
                UpdateModel();

                ChosePiece(CurrentCell, PreviousActiveCell);

                //атака королем под шахом
                KingAttackCheck(CurrentCell, ValidAttacks, PreviousActiveCell);

                //ход королем под шахом
                KingMoveCheck(CurrentCell, ValidMoves, PreviousActiveCell);

                MoveInCheck(CurrentCell, ValidMoves, PreviousActiveCell);

                AttackInCheck(CurrentCell, ValidMoves, PreviousActiveCell);

                UpdateModel();
            }
            else
            {
                UpdateModel();

                ChosePiece(CurrentCell, PreviousActiveCell);

                //Игрок хочет атаковать
                ValidAttacks = Attack(CurrentCell, ValidAttacks, PreviousActiveCell);

                //Игрок хочет сделать ход
                ValidMoves = Move(CurrentCell, ValidMoves, PreviousActiveCell);

                UpdateModel();

                CheckMessage();

                File.AppendAllText(pathOfFenFile, $"{Fen.GetFenFromTheGameField()}\n");
            }

        }, parameter => parameter is Cell cell && (Board.Any(x => x.Active) || cell.State != State.Empty));

        public ObservableCollection<string> PlayersMoves
        {
            get => playerMoves;
            set
            {
                playerMoves = value;
                OnPropertyChanged();
            }

        }

        public IEnumerable<char> Numbers => "87654321";

        public IEnumerable<char> Letters => "ABCDEFGH";

        public ICommand NewGameCommand => newGameCommand ??= new RelayCommand(parameter =>
        {
            game = new Game();
            playerMoves = new ObservableCollection<string>();
            moves = new ObservableCollection<string>();
            File.WriteAllText(pathOfFenFile, $"{DateTime.Now.ToString()}\n");
            Fen.GameField = game.GameField;

            SetupBoard();
        });

        private void CheckMessage()
        {
            if (IsCheck())
            {
                MessageBox.Show("Шах!");
            }
        }

        private bool IsCheck()
        {
            return game.GameField.IsCheck();
        }

        /// <summary>
        /// Убирает убитые фигуры, обновяет доску
        /// </summary>
        private void UpdateModel()
        {
            game.RemoveDeadPieces(pieces);
            game.GameField.Update(pieces, GetGameFieldString(), players[currentPlayer % 2].Color);
        }

        /// <summary>
        /// Атака королем под шахом
        /// </summary>
        /// <param name="CurrentCell">Текущая клетка</param>
        /// <param name="ValidAttacks">Доступные клетки для атаки</param>
        /// <param name="PreviousActiveCell">Предыдущая клетка</param>
        /// <returns></returns>
        private void KingAttackCheck(Cell CurrentCell, List<(int, int)> ValidAttacks, Cell PreviousActiveCell)
        {
            if (CurrentCell.State != State.Empty && PreviousActiveCell != null && (PreviousActiveCell.State == State.BlackKing || PreviousActiveCell.State == State.WhiteKing))
            {

                CurrentCell.Active = true;
                PreviousActiveCell.Active = false;
                King king = (King)game.GameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;

                ValidAttacks = king.AvailableKills(game.GetGameField(pieces));

                //Проверяем атакована ли клетка, на которую собирается пойти король
                var AvailableAttacksForKingInCheck = ValidAttacks.FindAll(x => game.GameField.GetAtackStatus(pieces.Where(x => x.Color != king.Color).ToList(), x, GetGameFieldString()));
                foreach (var removedMoves in AvailableAttacksForKingInCheck)
                {
                    ValidAttacks.Remove(removedMoves);
                }
                if (ValidAttacks.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                {

                    king.IsMoved = true;

                    AttackView(CurrentCell, PreviousActiveCell);

                    AttackModel(CurrentCell, PreviousActiveCell);

                    ChangePlayer();

                    MakeEnPassentUnavailableForAllPawns();


                    //MainWindow.AddNewMove(CurrentCell.Position.ToString());
                    moves.Add($"{players[currentPlayer].Color} {CurrentCell.Position}");
                    PlayersMoves = moves;

                }
            }
        }

        private void AttackInCheck(Cell CurrentCell, List<(int, int)> ValidMoves, Cell PreviousActiveCell)
        {
            if (CurrentCell.State != State.Empty && PreviousActiveCell != null && PreviousActiveCell?.State != State.Empty && !(PreviousActiveCell.State == State.BlackKing || PreviousActiveCell.State == State.WhiteKing))
            {
                IPiece chosenPiese = game.GameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;

                ValidMoves = chosenPiese.AvailableKills(game.GetGameField(pieces));

                var AvailableKillsIfKingInCheck = ValidMoves.FindAll(x => !game.GameField.GetCheckStatusAfterMove(pieces, chosenPiese, x, players[currentPlayer]));

                foreach (var removedMoves in AvailableKillsIfKingInCheck)
                {
                    ValidMoves.Remove(removedMoves);
                }
                if (ValidMoves.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                {

                    CurrentCell.Active = true;
                    PreviousActiveCell.Active = false;

                    AttackView(CurrentCell, PreviousActiveCell);

                    AttackModel(CurrentCell, PreviousActiveCell);

                    ChangePlayer();

                    MakeEnPassentUnavailableForAllPawns();


                    //MainWindow.AddNewMove(CurrentCell.Position.ToString());
                    moves.Add($"{players[currentPlayer].Color} {CurrentCell.Position}");
                    PlayersMoves = moves;
                }
            }
        }

        private void MoveInCheck(Cell CurrentCell, List<(int, int)> ValidMoves, Cell PreviousActiveCell)
        {
            if (CurrentCell.State == State.Empty && PreviousActiveCell != null && !(PreviousActiveCell.State == State.BlackKing || PreviousActiveCell.State == State.WhiteKing))
            {

                IPiece chosenPiese = game.GameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;

                ValidMoves = chosenPiese.AvailableMoves(game.GetGameField(pieces));

                var AvailableMovesIfKingInCheck = ValidMoves.FindAll(x => game.GameField.GetCheckStatusAfterMove(pieces, chosenPiese, x, players[currentPlayer]));
                foreach (var removedMoves in AvailableMovesIfKingInCheck)
                {
                    ValidMoves.Remove(removedMoves);
                }
                if (ValidMoves.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                {

                    CurrentCell.Active = true;
                    PreviousActiveCell.Active = false;

                    MoveView(CurrentCell, PreviousActiveCell);

                    MoveModel(CurrentCell, PreviousActiveCell);

                    ChangePlayer();

                    MakeEnPassentUnavailableForAllPawns();


                    //MainWindow.AddNewMove(CurrentCell.Position.ToString());
                    moves.Add($"{players[currentPlayer].Color} {CurrentCell.Position}");
                    PlayersMoves = moves;
                }
            }
        }

        /// <summary>
        /// Ход королем под шахом
        /// </summary>
        /// <param name="CurrentCell">Текущая клетка</param>
        /// <param name="ValidMoves">Доступные клетки для хода</param>
        /// <param name="PreviousActiveCell">Предыдущая клетка</param>
        /// <returns></returns>
        private void KingMoveCheck(Cell CurrentCell, List<(int, int)> ValidMoves, Cell PreviousActiveCell)
        {
            if (CurrentCell.State == State.Empty && PreviousActiveCell != null && (PreviousActiveCell.State == State.WhiteKing || PreviousActiveCell.State == State.BlackKing))
            {

                CurrentCell.Active = true;
                PreviousActiveCell.Active = false;
                King king = (King)game.GameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;

                ValidMoves = king.AvailableMoves(game.GetGameField(pieces));
                var EnemyPieces = pieces.Where(x => x.Color != king.Color).ToList();
                var AvailableMovesForKingInCheck = ValidMoves.FindAll(x => game.GameField.GetAtackStatus(EnemyPieces, x, GetGameFieldString()));
                foreach (var removedMoves in AvailableMovesForKingInCheck)
                {
                    ValidMoves.Remove(removedMoves);
                }

                if (ValidMoves.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical)))
                {

                    king.IsMoved = true;

                    MoveView(CurrentCell, PreviousActiveCell);

                    MoveModel(CurrentCell, PreviousActiveCell);

                    MakeEnPassentUnavailableForAllPawns();

                    AddCurrentMoveToListView(CurrentCell);

                    ChangePlayer();

                }
            }
        }
        /// <summary>
        ///  После любого хода, все вражеские пешки нельзя взять на проходе по правилам.
        /// Для этого, после текущего хода устанавливаем у всех вражеских пешек соответсвующее значение
        /// </summary>
        private void MakeEnPassentUnavailableForAllPawns()
        {
            var enemyPawns = pieces.Where(p => p is Pawn);

            foreach (var EnemyPawn in enemyPawns)
            {
                ((Pawn)EnemyPawn).EnPassantAvailable = false;
            }
        }

        private void AddCurrentMoveToListView(Cell CurrentCell)
        {
            moves.Add($"{colors[currentPlayer]} {CurrentCell.Position}");
            PlayersMoves = moves;
        }

        /// <summary>
        /// проверет выбрал ли игрок правильную фигуру
        /// </summary>
        /// <param name="CurrentCell">Текущая клетка</param>
        /// <param name="PreviousActiveCell">Предыдущая клетка</param>
        private void ChosePiece(Cell CurrentCell, Cell PreviousActiveCell)
        {
            if (CurrentCell.State != State.Empty && PreviousActiveCell == null)
            {
                if (game.GameField[CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical].Piece.Color == players[currentPlayer % 2].Color)
                {
                    CurrentCell.Active = !CurrentCell.Active;
                }
            }
        }
        /// <summary>
        /// Ход, проверяет можно ли сделать ход и делает его, либо сообщает об ошибке
        /// </summary>
        /// <param name="CurrentCell">Текущая клетка</param>
        /// <param name="ValidMoves">Доступные клетки для хода</param>
        /// <param name="PreviousActiveCell">Предыдущая клетка</param>
        /// <returns></returns>
        private List<(int, int)> Move(Cell CurrentCell, List<(int, int)> ValidMoves, Cell PreviousActiveCell)
        {
            if (IsPlayerGoingToEmptyCell(CurrentCell, PreviousActiveCell))
            {

                IPiece ChosenPiece = game.GameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;

                ValidMoves = ChosenPiece.AvailableMoves(game.GetGameField(pieces));

                bool ShortCastleAvailble = false;

                bool LongCastleAvailable = false;

                var EnemyPieces = pieces.Where(x => x.Color != ChosenPiece.Color).ToList();

                var MyPieces = pieces.Where(x => x.Color == ChosenPiece.Color).ToList();

                var MyRooks = new List<Rook>();

                IfPieceIsKingRemoveAttackedCells(ValidMoves, ChosenPiece, ref ShortCastleAvailble, ref LongCastleAvailable, EnemyPieces, MyPieces, ref MyRooks);

                ValidMoves = IfChosenPieceIsPawnAddValidEnPassentMoves(ValidMoves, ChosenPiece, EnemyPieces);

                RemoveIncorrectMove(CurrentCell, ValidMoves, PreviousActiveCell, ChosenPiece);

                if (IsMoveValid(CurrentCell, ValidMoves))
                {
                    if (ShortCastlingIntention(CurrentCell, ChosenPiece, ShortCastleAvailble))
                    {
                        ShortCastleModel(ChosenPiece, MyRooks);

                        ShortCastleView(PreviousActiveCell, ChosenPiece);

                        moves.Add($"{ChosenPiece} 0-0");
                        PlayersMoves = moves;
                    }
                    else if (LongCastlingIntention(CurrentCell, ChosenPiece, LongCastleAvailable))
                    {
                        LongCastleModel(ChosenPiece, MyRooks);

                        LongCastleView(PreviousActiveCell, ChosenPiece);

                        //Добавляем сделанный ход на listview в главном окне
                        //MainWindow.AddNewMove($"Длинная рокировка {ChosenPiece.Color}");
                        moves.Add($"{ChosenPiece} 0-0-0");
                        PlayersMoves = moves;
                    }
                    else if (EnemyPieces.Where(x => x.Position.Item2 == ChosenPiece.Position.Item2).Where(x => x is Pawn).ToList().Count > 0 && ChosenPiece is Pawn)
                    {
                        EnPassentView(CurrentCell, PreviousActiveCell, ChosenPiece);

                        EnPassentModel(CurrentCell, PreviousActiveCell);

                        AddCurrentMoveToListView(CurrentCell);
                    }
                    else
                    {
                        MoveModel(CurrentCell, PreviousActiveCell);

                        MoveView(CurrentCell, PreviousActiveCell);

                        AddCurrentMoveToListView(CurrentCell);
                    }
                    ChangePieceProperties(CurrentCell, ChosenPiece);

                    MakeEnPassentUnavailableForAllPawns();

                    ChangePlayer();
                }

            }

            return ValidMoves;
        }

        private static bool IsPlayerGoingToEmptyCell(Cell CurrentCell, Cell PreviousActiveCell)
        {
            return CurrentCell.State == State.Empty && PreviousActiveCell != null;
        }

        private void RemoveIncorrectMove(Cell CurrentCell, List<(int, int)> ValidMoves, Cell PreviousActiveCell, IPiece ChosenPiece)
        {
            if (IsCurrentMoveMakeCheckToOurKing(CurrentCell, ChosenPiece))
            {
                ValidMoves.RemoveAll(move => game.GameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece.AvailableMoves(GetGameFieldString()).Contains(move));
            }
        }

        private void IfPieceIsKingRemoveAttackedCells(List<(int, int)> ValidMoves, IPiece ChosenPiece, ref bool ShortCastleAvailble, ref bool LongCastleAvailable, List<IPiece> EnemyPieces, List<IPiece> MyPieces, ref List<Rook> MyRooks)
        {
            if (ChosenPiece is King)
            {
                var InvalidMovesForKing = FindsAttackedCells(ValidMoves, EnemyPieces);

                RemoveCheckMoves(ValidMoves, InvalidMovesForKing);

                MyRooks = GetMyRooks(MyPieces);

                IsCastlingAvailable(ValidMoves, ChosenPiece, out ShortCastleAvailble, out LongCastleAvailable, EnemyPieces, MyRooks);

            }
        }

        private List<(int, int)> IfChosenPieceIsPawnAddValidEnPassentMoves(List<(int, int)> ValidMoves, IPiece ChosenPiece, List<IPiece> EnemyPieces)
        {
            if (ChosenPiece is Pawn)
            {
                var EnemyPawn = EnemyPieces.Where(x => x.Position.Item2 == ChosenPiece.Position.Item2).Where(x => x is Pawn).Cast<Pawn>().ToList();

                if (EnemyPawn != null)
                {
                    ValidMoves = AddAvailableEnPassentMoves(ValidMoves, ChosenPiece, EnemyPawn);

                }

            }

            return ValidMoves;
        }

        private List<(int, int)> FindsAttackedCells(List<(int, int)> ValidMoves, List<IPiece> EnemyPieces)
        {
            return ValidMoves.FindAll(cell => game.GameField.GetAtackStatus(EnemyPieces, cell, GetGameFieldString()));
        }

        private static List<Rook> GetMyRooks(List<IPiece> MyPieces)
        {
            return MyPieces.Where(myPiece => myPiece is Rook).Cast<Rook>().ToList();
        }

        private bool IsCurrentMoveMakeCheckToOurKing(Cell CurrentCell, IPiece ChosenPiece)
        {
            return game.GameField.GetCheckStatusAfterMove(pieces, ChosenPiece, (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical), players[currentPlayer]);
        }

        /// <summary>
        /// Проверяет можно ли атаковать и атакует, либо выводит сообщение об ошибке
        /// </summary>
        /// <param name="CurrentCell">Текущая клетка</param>
        /// <param name="ValidAttacks">Доступные клетки для атаки</param>>
        /// <param name="PreviousActiveCell">Предыдущая клетка</param>
        /// <returns></returns>
        private List<(int, int)> Attack(Cell CurrentCell, List<(int, int)> ValidAttacks, Cell PreviousActiveCell)
        {
            if (CurrentCell.State != State.Empty && PreviousActiveCell != null)
            {

                //если игрок захотел поменять выбранную фигуру
                if (IsPlayerWantChoseOtherPiece(CurrentCell))
                {
                    ChoseOtherPiece(CurrentCell, PreviousActiveCell);

                }
                else//если игрок хочет съесть фигуру
                {

                    IPiece ChosenPiece = game.GameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece;
                    ValidAttacks = ChosenPiece.AvailableKills(game.GetGameField(pieces));
                    if (ChosenPiece is King)
                    {
                        var InvalidMoves = ValidAttacks.FindAll(x => game.GameField.GetAtackStatus(pieces.Where(x => x.Color != ChosenPiece.Color).ToList(), x, GetGameFieldString()));
                        RemoveInvalidMoves(ValidAttacks, InvalidMoves);
                    }

                    //Атаковать короля нельзя, поэтому атака короля - недоступный ход, который мы убираем из доступных ходов
                    var InvalidAttacks = ValidAttacks.FindAll(x => game.GameField[x.Item1, x.Item2].Piece is King);
                    RermoveIvalidAttacks(ValidAttacks, InvalidAttacks);

                    bool IsCurrentMoveInvalid = game.GameField.GetCheckStatusAfterMove(pieces, ChosenPiece, (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical), players[currentPlayer]);

                    if (IsCurrentMoveInvalid)
                    {
                        ValidAttacks.Remove((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical));
                    }

                    if (IsAttackValid(CurrentCell, ValidAttacks))
                    {
                        AttackView(CurrentCell, PreviousActiveCell);
                        AttackModel(CurrentCell, PreviousActiveCell);
                        ChangePlayer();


                        moves.Add($"{ChosenPiece} {CurrentCell.Position}");
                        PlayersMoves = moves;
                    }
                    ChangePieceProperties(CurrentCell, ChosenPiece);

                    MakeEnPassentUnavailableForAllPawns();
                }
            }
            return ValidAttacks;
        }

        private void ChangePlayer()
        {
            currentPlayer++;
            if (currentPlayer >= 2)
            {
                currentPlayer -= 2;
            }
            Fen.CurrentPlayer = currentPlayer;
        }

        private static void ChangePieceProperties(Cell CurrentCell, IPiece ChosenPiece)
        {
            if (IfPawnMoved2StepsForward(CurrentCell, ChosenPiece))
            {
                ((Pawn)ChosenPiece).EnPassantAvailable = true;
            }
            if (ChosenPiece is King king)
            {
                king.IsMoved = true;
            }
            if (ChosenPiece is Rook Rook)
            {
                Rook.IsMoved = true;
            }
        }

        private static bool IsMoveValid(Cell CurrentCell, List<(int, int)> ValidMoves)
        {
            return ValidMoves.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical));
        }

        private List<(int, int)> AddAvailableEnPassentMoves(List<(int, int)> ValidMoves, IPiece ChosenPiece, List<Pawn> EnemyPawn)
        {
            foreach (var pawn in EnemyPawn)
            {
                var validPawnMoves = ((Pawn)ChosenPiece).AvailableKills(game.GetGameField(pieces), pawn);
                ValidMoves = ValidMoves.Union(validPawnMoves)?.ToList();
            }

            return ValidMoves;
        }

        private void MoveModel(Cell CurrentCell, Cell PreviousActiveCell)
        {
            game.GameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece.ChangePosition((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical));
        }

        private static void MoveView(Cell CurrentCell, Cell PreviousActiveCell)
        {
            PreviousActiveCell.Active = false;
            CurrentCell.State = PreviousActiveCell.State;
            PreviousActiveCell.State = State.Empty;
        }

        private static bool IfPawnMoved2StepsForward(Cell CurrentCell, IPiece ChosenPiece)
        {
            return ChosenPiece is Pawn &&
                (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical) == (((Pawn)ChosenPiece).StartPos.Item1 + ((Pawn)ChosenPiece).MoveDir[1].Item1, ((Pawn)ChosenPiece).StartPos.Item2 + ((Pawn)ChosenPiece).MoveDir[1].Item2);
        }

        private void EnPassentModel(Cell CurrentCell, Cell PreviousActiveCell)
        {
            game.GameField[PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical].Piece.ChangePosition((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical));
        }

        private void EnPassentView(Cell CurrentCell, Cell PreviousActiveCell, IPiece ChosenPiece)
        {
            PreviousActiveCell.Active = false;
            CurrentCell.State = PreviousActiveCell.State;
            PreviousActiveCell.State = State.Empty;

            Board[7 - ChosenPiece.Position.Item2, CurrentCell.Position.Horizontal] = State.Empty;
        }

        private static void RemoveCheckMoves(List<(int, int)> ValidMoves, List<(int, int)> InvalidMovesCheck)
        {
            foreach (var move in InvalidMovesCheck)
            {
                ValidMoves.Remove(move);
            }
        }

        private void IsCastlingAvailable(List<(int, int)> ValidMoves, IPiece ChosenPiece, out bool ShortCastleAvailble, out bool LongCastleAvailable, List<IPiece> EnemyPieces, List<Rook> MyRooks)
        {
            ShortCastleAvailble = ((King)ChosenPiece).ShortCastling(MyRooks.Where(rook => rook.RookKind == RookKind.Royal).ToList()[0], game.GameField, EnemyPieces, GetGameFieldString());
            LongCastleAvailable = ((King)ChosenPiece).LongCastling(MyRooks.Where(rook => rook.RookKind == RookKind.Queen).ToList()[0], game.GameField, EnemyPieces, GetGameFieldString());
            //Если рокировки доступны, то добавляем их в список доступных ходов
            if (ShortCastleAvailble)
            {
                ValidMoves.Add((shortCastleVerticalPosition, ChosenPiece.Position.Item2));
            }

            if (LongCastleAvailable)
            {
                ValidMoves.Add((longCastleVerticalPosition, ChosenPiece.Position.Item2));
            }
        }

        private static void LongCastleModel(IPiece ChosenPiece, List<Rook> MyRooks)
        {
            ((King)ChosenPiece).ChangePosition((2, ChosenPiece.Position.Item2));

            Rook MyQueenRook = MyRooks.Where(MyRook => MyRook.RookKind == RookKind.Queen).ToList()[0];

            MyQueenRook.ChangePosition((3, MyQueenRook.Position.Item2));
        }

        private static void ShortCastleModel(IPiece ChosenPiece, List<Rook> MyRooks)
        {
            ((King)ChosenPiece).ChangePosition((6, ChosenPiece.Position.Item2));

            Rook MyRoyalRook = MyRooks.Where(MyRook => MyRook.RookKind == RookKind.Royal).ToList()[0];

            MyRoyalRook.ChangePosition((5, MyRoyalRook.Position.Item2));
        }

        private static bool LongCastlingIntention(Cell CurrentCell, IPiece piece, bool LongCastleAvailable)
        {
            return LongCastleAvailable && piece is King && (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical) == (longCastleVerticalPosition, piece.Position.Item2);
        }

        private static bool ShortCastlingIntention(Cell CurrentCell, IPiece piece, bool ShortCastleAvailable)
        {
            return ShortCastleAvailable && piece is King && (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical) == (shortCastleVerticalPosition, piece.Position.Item2);
        }

        /// <summary>
        /// Отрисовка длинной рокировки 
        /// </summary>
        /// <param name="PreviousActiveCell">Предыдущая выделенная клетка</param>
        /// <param name="piece">Выбранная фигура</param>
        private void LongCastleView(Cell PreviousActiveCell, IPiece piece)
        {
            /*
            При обращении к Board через индексы важно помнить, что позиция по горизонтали у Board - вторая координата, позиция по вертикали - первая.
            Порядок по вертикали сверху вниз, то есть белый король находится на 7 строке у Board
            */
            PreviousActiveCell.Active = false;
            Board[7 - piece.Position.Item2, 2] = PreviousActiveCell.State;
            PreviousActiveCell.State = State.Empty;
            Board[7 - piece.Position.Item2, 3] = Board[7 - piece.Position.Item2, 0];
            Board[7 - piece.Position.Item2, 0] = State.Empty;
        }

        /// <summary>
        /// Отрисовка короткой рокировки
        /// </summary>
        /// <param name="PreviousActiveCell">предыдущая выделенная клетка</param>
        /// <param name="piece">Выбранная фигура</param>
        private void ShortCastleView(Cell PreviousActiveCell, IPiece piece)
        {
            /*
             При обращении к Board через индексы важно помнить, что позиция по горизонтали у Board - вторая координата, позиция по вертикали - первая.
             Порядок по вертикали сверху вниз, то есть белый король находится на 7 строке у Board
            */
            PreviousActiveCell.Active = false;
            Board[7 - piece.Position.Item2, 6] = PreviousActiveCell.State;
            PreviousActiveCell.State = State.Empty;
            Board[7 - piece.Position.Item2, 5] = Board[7 - piece.Position.Item2, 7];
            Board[7 - piece.Position.Item2, 7] = State.Empty;
        }

        private void AttackModel(Cell CurrentCell, Cell PreviousActiveCell)
        {
            game.CheckIfPieceWasKilled((PreviousActiveCell.Position.Horizontal, PreviousActiveCell.Position.Vertical), (CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical), GetGameFieldString(), pieces);
            game.RemoveDeadPieces(pieces);
        }

        private static void AttackView(Cell CurrentCell, Cell PreviousActiveCell)
        {
            CurrentCell.State = PreviousActiveCell.State;
            PreviousActiveCell.Active = false;
            PreviousActiveCell.State = State.Empty;
        }

        private static bool IsAttackValid(Cell CurrentCell, List<(int, int)> ValidAttacks)
        {
            return ValidAttacks.Contains((CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical));
        }

        private static void RermoveIvalidAttacks(List<(int, int)> ValidAttacks, List<(int, int)> InvalidAttacks)
        {
            foreach (var move in InvalidAttacks)
            {
                ValidAttacks.Remove(move);
            }
        }

        private static void RemoveInvalidMoves(List<(int, int)> ValidAttacks, List<(int, int)> InvalidMoves)
        {
            foreach (var move in InvalidMoves)
            {
                ValidAttacks.Remove(move);
            }
        }

        private static void ChoseOtherPiece(Cell CurrentCell, Cell PreviousActiveCell)
        {
            PreviousActiveCell.Active = !PreviousActiveCell.Active;
            CurrentCell.Active = true;
        }

        private bool IsPlayerWantChoseOtherPiece(Cell CurrentCell)
        {
            return game.GameField[CurrentCell.Position.Horizontal, CurrentCell.Position.Vertical].Piece.Color == players[currentPlayer % 2].Color;
        }

        /// <summary>
        /// Утсанавливает начальные позиции фигурам при старте игры, для WPF
        /// </summary>
        private void SetupBoard()
        {
            currentPlayer = 0;
            pieces = game.GetPiecesStartPosition();
            players = new List<Player>()
        {
            new Player(PieceColor.White,pieces.Where(x=> x.Color == PieceColor.White).ToList(),"user1"),
            new Player(PieceColor.Black,pieces.Where(x=> x.Color == PieceColor.Black).ToList(),"user2")
        };
            Board board = new Board();
            board[0, 0] = State.BlackRook;
            board[0, 1] = State.BlackKnight;
            board[0, 2] = State.BlackBishop;
            board[0, 3] = State.BlackQueen;
            board[0, 4] = State.BlackKing;
            board[0, 5] = State.BlackBishop;
            board[0, 6] = State.BlackKnight;
            board[0, 7] = State.BlackRook;
            for (int i = 0; i < 8; i++)
            {
                board[1, i] = State.BlackPawn;
                board[6, i] = State.WhitePawn;
            }
            board[7, 0] = State.WhiteRook;
            board[7, 1] = State.WhiteKnight;
            board[7, 2] = State.WhiteBishop;
            board[7, 3] = State.WhiteQueen;
            board[7, 4] = State.WhiteKing;
            board[7, 5] = State.WhiteBishop;
            board[7, 6] = State.WhiteKnight;
            board[7, 7] = State.WhiteRook;
            Board = board;
            playerMoves = new ObservableCollection<string>();
        }

        private string[,] GetGameFieldString() => game.GetGameField(pieces);
    }
}
