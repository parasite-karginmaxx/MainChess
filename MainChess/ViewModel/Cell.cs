using MainChess.ViewModel;
using MainChess.ViewModel.Base;

namespace MainChess.ViewModel
{
    public class Cell : BaseViewModel
    {
        private State _state;
        private bool _active;

        public Cell(int horizontal, int vertical)
        {
            Position = new Position(horizontal, vertical);
        }

        /// <summary>
        /// Позиция клетки на игровой доске описывается двумя натуральными числами (по горизонтали, по вертикали)
        /// </summary>
        public Position Position { get; set; }

        public State State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged();
            }
        }
        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                OnPropertyChanged();
            }
        }
    }
}