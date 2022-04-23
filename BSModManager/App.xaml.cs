using BSModManager.Static;
using ModManager.ViewModels;
using ModManager.Views;
using Prism.Ioc;
using System.Windows;

namespace ModManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            FolderManager.FolderInitialize();
            base.OnStartup(e);
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // たとえViewModelであったとしても、ViewModelをDIするときはシングルトンでないと新しいインスタンスになる
            containerRegistry.RegisterSingleton<MainWindowViewModel>();
            containerRegistry.RegisterSingleton<SettingsTabViewModel>();

            containerRegistry.RegisterForNavigation<MainTab>();
            containerRegistry.RegisterForNavigation<SettingsTab>();
        }
    }
}
