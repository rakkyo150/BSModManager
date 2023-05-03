using BSModManager.Interfaces;
using BSModManager.Models;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace BSModManager.ViewModels
{
    public class UpdateTabViewModel : BindableBase, IDestructible
    {
        private CompositeDisposable disposables { get; } = new CompositeDisposable();
        private readonly ModsContainerAgent modsDataContainerAgent;

        public ReactiveProperty<string> SearchWords { get; } = new ReactiveProperty<string>("");

        public ReactiveCommand<string> ColorCommand { get; }


        public ObservableCollection<IMod> LocalModsContainer { get; }

        public UpdateTabViewModel(ModsContainerAgent mdca)
        {
            modsDataContainerAgent = mdca;

            SearchWords = modsDataContainerAgent.LocalModsContainer.ToReactivePropertyAsSynchronized(x => x.SearchWords).AddTo(disposables);
            LocalModsContainer = modsDataContainerAgent.LocalModsContainer.ShowedLocalModsData;

            ColorCommand = new ReactiveCommand<string>()
                .WithSubscribe(x => modsDataContainerAgent.LocalModsContainer.AddOrRemoveColorWord2SearchWords(x))
                .AddTo(disposables);

            // https://alfort.online/689
            BindingOperations.EnableCollectionSynchronization(LocalModsContainer, new object());
        }

        public void Destroy()
        {
            disposables.Dispose();
        }
    }
}
