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
        private CompositeDisposable Disposables { get; } = new CompositeDisposable();

        public ReadOnlyReactivePropertySlim<string> ModNameAndProgress { get; }
        public ReactiveProperty<string> Url { get; }
        public ReadOnlyReactivePropertySlim<bool> ExistInMA { get; }
        public ReactiveProperty<bool> Original { get; }
        public ReactiveProperty<bool> IsBackButtonEnable { get; }
        public ReadOnlyReactivePropertySlim<string> NextOrFinish { get; }

        public ReactiveCommand SearchMod { get; } = new ReactiveCommand();
        public ReactiveCommand ExitCommand { get; } = new ReactiveCommand();
        public ReactiveCommand BackCommand { get; } = new ReactiveCommand();
        public ReactiveCommand NextOrFinishCommand { get; } = new ReactiveCommand();

        private bool canCloseDialog = false;


        public ModDataChanger changeModInfoPropertyModel;

        public ChangeModInfoViewModel(ModDataChanger cmipm)
        {
            changeModInfoPropertyModel = cmipm;

            ModNameAndProgress = changeModInfoPropertyModel.ObserveProperty(x => x.ModNameAndProgress).ToReadOnlyReactivePropertySlim().AddTo(Disposables);

            // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
            // 双方向のバインドにはToReactivePropertyAsSynchronizedを使う(ToReactivePropertyでは片方向になってしまう)

            // SetterでModsDataにデータセットされます
            Url = changeModInfoPropertyModel.ToReactivePropertyAsSynchronized(x => x.Url).AddTo(Disposables);
            ExistInMA = changeModInfoPropertyModel.ObserveProperty(x => x.ExistInMA).ToReadOnlyReactivePropertySlim().AddTo(Disposables);
            // SetterでModsDataにデータセットされます
            Original = changeModInfoPropertyModel.ToReactivePropertyAsSynchronized(x => x.Original).AddTo(Disposables);

            IsBackButtonEnable = changeModInfoPropertyModel.ToReactivePropertyAsSynchronized(x => x.IsBackButtonEnable).AddTo(Disposables);

            NextOrFinish = changeModInfoPropertyModel.ObserveProperty(x => x.NextOrFinishButtonText).ToReadOnlyReactivePropertySlim().AddTo(Disposables);

            SearchMod.Subscribe(() =>
            {
                changeModInfoPropertyModel.Search();
            }).AddTo(Disposables);

            ExitCommand.Subscribe(() =>
            {
                changeModInfoPropertyModel.SetInfoToMods();
                canCloseDialog = true;
                RequestClose.Invoke(new DialogResult(ButtonResult.OK));
            }).AddTo(Disposables);

            BackCommand.Subscribe(() =>
            {
                changeModInfoPropertyModel.SetInfoToMods();
                canCloseDialog = true;
                RequestClose.Invoke(new DialogResult(ButtonResult.OK));
                changeModInfoPropertyModel.ShowChangeModInfoPreviousDialog();
            }).AddTo(Disposables);

            NextOrFinishCommand.Subscribe(() =>
            {
                changeModInfoPropertyModel.SetInfoToMods();
                canCloseDialog = true;
                RequestClose.Invoke(new DialogResult(ButtonResult.OK));

                if (NextOrFinish.Value == "Next")
                {
                    changeModInfoPropertyModel.ShowChangeModInfoNextDialog();
                }
            }).AddTo(Disposables);

            changeModInfoPropertyModel.ChangeIsUrlTextBoxReadOnly();
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
            Disposables.Dispose();
        }
    }
}
