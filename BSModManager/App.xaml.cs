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
            containerRegistry.RegisterSingleton<PastMods>();
            containerRegistry.RegisterSingleton<RecommendMods>();
            containerRegistry.RegisterSingleton<LocalMods>();
            containerRegistry.RegisterSingleton<MA>();
            containerRegistry.RegisterSingleton<GitHubApi>();
            containerRegistry.RegisterSingleton<MainModsSetter>();
            containerRegistry.RegisterSingleton<ChangeModInfoModel>();
            containerRegistry.RegisterSingleton<MyselfUpdater>();

            containerRegistry.Register<Refresher>();
            containerRegistry.Register<ModInstaller>();
            containerRegistry.Register<ModCsvHandler>();
            containerRegistry.Register<ModUpdater>();
            containerRegistry.Register<InitialDirectorySetup>();
            containerRegistry.Register<ModDisposer>();
            containerRegistry.Register<PreviousLocalModsDataGetter>();

            containerRegistry.RegisterForNavigation<UpdateTab>();
            containerRegistry.RegisterForNavigation<InstallTab>();
            containerRegistry.RegisterForNavigation<SettingsTab>();

            containerRegistry.RegisterDialog<InitialSetting>();
            containerRegistry.RegisterDialog<ChangeModInfo>();
        }
    }
}
