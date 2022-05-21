using Prism.Mvvm;

namespace BSModManager.Static
{
    internal class MainWindowLog : BindableBase
    {
        internal static MainWindowLog Instance { get; set; } = new MainWindowLog();

        private string debug = "";
        internal string Debug
        {
            get { return debug; }
            set { SetProperty(ref debug, value); }
        }
    }
}
