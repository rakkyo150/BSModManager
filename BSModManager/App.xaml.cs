using BSModManager.Models;
using BSModManager.Models.CoreManager;
using BSModManager.Models.ViewModelCommonProperty;
using BSModManager.Static;
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
            containerRegistry.RegisterSingleton<UpdateTabPropertyModel>();
            containerRegistry.RegisterSingleton<SettingsTabPropertyModel>();
            containerRegistry.RegisterSingleton<UpdateMyselfConfirmPropertyModel>();
            containerRegistry.RegisterSingleton<ChangeModInfoPropertyModel>();
            
            containerRegistry.RegisterSingleton<ConfigFileManager>();
            containerRegistry.RegisterSingleton<VersionManager>();
            containerRegistry.RegisterSingleton<PastModsDataModel>();
            containerRegistry.RegisterSingleton<RecommendModsDataModel>();
            containerRegistry.RegisterSingleton<ModsDataModel>();

            containerRegistry.RegisterSingleton<InnerData>();
            containerRegistry.RegisterSingleton<DataManager>();
            containerRegistry.RegisterSingleton<GitHubManager>();

            containerRegistry.RegisterForNavigation<UpdateTab>();
            containerRegistry.RegisterForNavigation<InstallTab>();
            containerRegistry.RegisterForNavigation<SettingsTab>();

            containerRegistry.RegisterDialog<InitialSetting>();
            containerRegistry.RegisterDialog<ChangeModInfo>();
        }
    }
}
