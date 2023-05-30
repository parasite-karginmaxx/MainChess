using MainChess.ViewModel;
using MainChess.ViewModel.Base;

namespace MainChess.ViewModel
{
    public class Cell : BaseViewModel
    {
        private State _state;
        private bool _active;

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