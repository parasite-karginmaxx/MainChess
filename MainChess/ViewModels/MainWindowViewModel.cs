using MainChess.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MainChess.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        #region Заголовок окна
        private string _title = "NoteApp";
        /// <summary> Заголовок окна </summary>
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        #endregion

        
    }
}
