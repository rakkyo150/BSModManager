using BSModManager.Models;
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
            base.OnStartup(e);
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // たとえViewModelであったとしても、ViewModelをDIするときはシングルトンでないと新しいインスタンスになる

            containerRegistry.RegisterSingleton<PastMods>();
            containerRegistry.RegisterSingleton<RecommendMods>();
            containerRegistry.RegisterSingleton<LocalMods>();

            containerRegistry.RegisterSingleton<Refresher>();
            containerRegistry.RegisterSingleton<ModInstaller>();
            containerRegistry.RegisterSingleton<GitHubApi>();
            containerRegistry.RegisterSingleton<ConfigFileHandler>();
            containerRegistry.RegisterSingleton<MAMods>();
            containerRegistry.RegisterSingleton<ModCsvHandler>();
            containerRegistry.RegisterSingleton<MyselfUpdater>();
            containerRegistry.RegisterSingleton<ModUpdater>();
            containerRegistry.RegisterSingleton<InitialDirectorySetup>();
            containerRegistry.RegisterSingleton<ModDisposer>();
            containerRegistry.RegisterSingleton<SettingsVerifier>();
            containerRegistry.RegisterSingleton<PreviousLocalModsDataGetter>();
            containerRegistry.RegisterSingleton<ChangeModInfoModel>();

            containerRegistry.RegisterForNavigation<UpdateTab>();
            containerRegistry.RegisterForNavigation<InstallTab>();
            containerRegistry.RegisterForNavigation<SettingsTab>();

            containerRegistry.RegisterDialog<InitialSetting>();
            containerRegistry.RegisterDialog<ChangeModInfo>();
        }
    }
}
