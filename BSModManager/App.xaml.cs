using BSModManager.Models;
using BSModManager.Static;
using BSModManager.ViewModels;
using BSModManager.Views;
using Prism.Ioc;
using System.Windows;

namespace BSModManager
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
            containerRegistry.RegisterSingleton<MainWindowPropertyModel>();
            containerRegistry.RegisterSingleton<MainTabPropertyModel>();
            containerRegistry.RegisterSingleton<SettingsTabViewModel>();

            containerRegistry.RegisterSingleton<ConfigFileManager>();
            containerRegistry.RegisterSingleton<VersionManager>();

            containerRegistry.RegisterForNavigation<MainTab>();
            containerRegistry.RegisterForNavigation<SettingsTab>();
        }
    }
}
