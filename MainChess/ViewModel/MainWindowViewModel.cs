using MainChess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MainChess.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {

        #region Заголовок окна
        private string _title = "Шахматы";
        /// <summary> Заголовок окна </summary>
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        #endregion

        public BoardViewModel? Board { get; set; }

        #region Команды

        public ICommand CloseAppCommand { get; }
        private bool CanCloseAppCommandExecute(object p) => true;
        private void OnCloseAppCommandExecuted(object p)
        {
            Application.Current.Shutdown();
        }

        #endregion

        public MainWindowViewModel() 
        {
            CloseAppCommand = new LambdaCommand(OnCloseAppCommandExecuted, CanCloseAppCommandExecute);
        }
    }
}
