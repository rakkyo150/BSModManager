using BSModManager.Models;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;

namespace BSModManager.ViewModels
{
    public class ChangeModInfoViewModel : BindableBase, IDialogAware, IDestructible
    {
        private CompositeDisposable disposables { get; } = new CompositeDisposable();

        public ReadOnlyReactivePropertySlim<string> ModNameAndProgress { get; }
        public ReactiveProperty<string> Url { get; }
        public ReadOnlyReactivePropertySlim<bool> ExistInMA { get; }
        public ReactiveProperty<bool> Original { get; }
        public ReadOnlyReactivePropertySlim<string> NextOrFinish { get; }

        public ReactiveCommand SearchMod { get; } = new ReactiveCommand();
        public ReactiveCommand ExitCommand { get; } = new ReactiveCommand();
        public ReactiveCommand NextOrFinishCommand { get; } = new ReactiveCommand();

        private bool canCloseDialog = false;


        public ChangeModInfoModel changeModInfoPropertyModel;

        public ChangeModInfoViewModel(ChangeModInfoModel cmipm)
        {
            changeModInfoPropertyModel = cmipm;

            ModNameAndProgress = changeModInfoPropertyModel.ObserveProperty(x => x.ModNameAndProgress).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
            // 双方向のバインドにはToReactivePropertyAsSynchronizedを使う(ToReactivePropertyでは片方向になってしまう)

            // SetterでModsDataにデータセットされます
            Url = changeModInfoPropertyModel.ToReactivePropertyAsSynchronized(x => x.Url).AddTo(disposables);
            ExistInMA = changeModInfoPropertyModel.ObserveProperty(x => x.ExistInMA).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            // SetterでModsDataにデータセットされます
            Original = changeModInfoPropertyModel.ToReactivePropertyAsSynchronized(x => x.Original).AddTo(disposables);

            NextOrFinish = changeModInfoPropertyModel.ObserveProperty(x => x.NextOrFinish).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            SearchMod.Subscribe(() =>
            {
                changeModInfoPropertyModel.Search();
            }).AddTo(disposables);
            ExitCommand.Subscribe(() =>
            {
                // Exitするので
                changeModInfoPropertyModel.Position = 1;
                canCloseDialog = true;
                changeModInfoPropertyModel.GetInfo();
                RequestClose.Invoke(new DialogResult(ButtonResult.OK));
                canCloseDialog = false;
                if (changeModInfoPropertyModel.NextOrFinish == "Finish")
                {
                    changeModInfoPropertyModel.NextOrFinish = "Next";
                }
            }).AddTo(disposables);
            NextOrFinishCommand.Subscribe(() =>
            {
                canCloseDialog = true;
                changeModInfoPropertyModel.GetInfo();
                RequestClose.Invoke(new DialogResult(ButtonResult.OK));
                canCloseDialog = false;
                changeModInfoPropertyModel.ChangeModInfo();
            }).AddTo(disposables);
        }

        public string Title => "Change Mod Info";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => canCloseDialog;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        public void Destroy()
        {
            disposables.Dispose();
        }
    }
}
