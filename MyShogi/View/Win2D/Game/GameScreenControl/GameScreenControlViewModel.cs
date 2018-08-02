using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.View.Win2D
{
    public partial class GameScreenControl
    {
        /// <summary>
        /// このクラスで用いるViewModel
        /// </summary>
        public class GameScreenControlViewModel : NotifyObject
        {
        }

        public GameScreenControlViewModel ViewModel = new GameScreenControlViewModel();
    }
}
