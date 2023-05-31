using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainChess.ViewModel
{
    public class Position
    {
        public int Vertical { get; set; }
        public int Horizontal { get; set; }
        public Position(int Horizontal, int Vertical)
        {
            this.Horizontal = Horizontal;
            this.Vertical = Vertical;
        }
    }
}
