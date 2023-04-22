using BSModManager.Models;
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
            base.OnStartup(e);
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<PastModsContainer>();
            containerRegistry.RegisterSingleton<RecommendModsContainer>();
            containerRegistry.RegisterSingleton<LocalModsContainer>();
            containerRegistry.RegisterSingleton<MA>();
            containerRegistry.RegisterSingleton<GitHubApi>();
            containerRegistry.RegisterSingleton<ModsContainerAgent>();
            containerRegistry.RegisterSingleton<ChangeModInfoModel>();
            containerRegistry.RegisterSingleton<MyselfUpdater>();

            containerRegistry.Register<Refresher>();
            containerRegistry.Register<ModInstaller>();
            containerRegistry.Register<ModsDataCsv>();
            containerRegistry.Register<ModUpdater>();
            containerRegistry.Register<InitialSetup>();
            containerRegistry.Register<ModDisposer>();

            containerRegistry.RegisterForNavigation<UpdateTab>();
            containerRegistry.RegisterForNavigation<InstallTab>();
            containerRegistry.RegisterForNavigation<SettingsTab>();
            containerRegistry.RegisterForNavigation<LicenseTab>();

            containerRegistry.RegisterDialog<InitialSetting>();
            containerRegistry.RegisterDialog<ChangeModInfo>();
        }
    }
}
