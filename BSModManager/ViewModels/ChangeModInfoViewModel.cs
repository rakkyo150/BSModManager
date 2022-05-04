using BSModManager.Models.ViewModelCommonProperty;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;

namespace BSModManager.ViewModels
{
    public class ChangeModInfoViewModel : BindableBase, IDialogAware
    {
        public ReadOnlyReactivePropertySlim<string> ModNameAndProgress { get; }
        public ReactiveProperty<string> Url { get; }
        public ReactiveProperty<bool> Original { get; }
        public ReadOnlyReactivePropertySlim<string> NextOrFinish { get; }

        public ReactiveCommand ExitCommand { get; } = new ReactiveCommand();
        public ReactiveCommand NextOrFinishCommand { get; } = new ReactiveCommand();

        private bool canCloseDialog = false;


        public ChangeModInfoPropertyModel changeModInfoPropertyModel;

        public ChangeModInfoViewModel(ChangeModInfoPropertyModel cmipm)
        {
            changeModInfoPropertyModel = cmipm;

            ModNameAndProgress = changeModInfoPropertyModel.ObserveProperty(x => x.ModNameAndProgress).ToReadOnlyReactivePropertySlim();

            // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
            // 双方向のバインドにはToReactivePropertyAsSynchronizedを使う(ToReactivePropertyでは片方向になってしまう)

            // SetterでModsDataにデータセットされます
            Url = changeModInfoPropertyModel.ToReactivePropertyAsSynchronized(x => x.Url);
            // SetterでModsDataにデータセットされます
            Original = changeModInfoPropertyModel.ToReactivePropertyAsSynchronized(x => x.Original);

            NextOrFinish = changeModInfoPropertyModel.ObserveProperty(x => x.NextOrFinish).ToReadOnlyReactivePropertySlim();

            ExitCommand.Subscribe(() =>
            {
                // Exitするので
                changeModInfoPropertyModel.Position = 1;
                canCloseDialog = true;
                RequestClose.Invoke(new DialogResult(ButtonResult.OK));
                canCloseDialog = false;
                if (changeModInfoPropertyModel.NextOrFinish == "Finish")
                {
                    changeModInfoPropertyModel.NextOrFinish = "Next";
                }
            });
            NextOrFinishCommand.Subscribe(() =>
            {
                canCloseDialog = true;
                RequestClose.Invoke(new DialogResult(ButtonResult.OK));
                canCloseDialog = false;
                changeModInfoPropertyModel.ChangeModInfo();
            });
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
    }
}
