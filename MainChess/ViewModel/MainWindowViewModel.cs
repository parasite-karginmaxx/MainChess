using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MainChess.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        public BoardViewModel? Board { get; set; }

    }
}
