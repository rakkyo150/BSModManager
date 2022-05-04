using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Linq;

namespace BSModManager.Models.ViewModelCommonProperty
{
    public class ChangeModInfoPropertyModel : BindableBase
    {
        IDialogService dialogService;
        MainTabPropertyModel mainTabPropertyModel;

        public ChangeModInfoPropertyModel(IDialogService ds, MainTabPropertyModel mtpm)
        {
            dialogService = ds;
            mainTabPropertyModel = mtpm;
        }

        private string modName;

        private string modNameAndProgress;
        public string ModNameAndProgress
        {
            get { return modNameAndProgress; }
            set { SetProperty(ref modNameAndProgress, value); }
        }

        private string url;
        // SetterでModsDataにデータセットされます
        public string Url
        {
            get { return url; }
            set
            {
                SetProperty(ref url, value);
                mainTabPropertyModel.ModsData.First(x => x.Mod == modName).Url = Url;
            }
        }

        private bool original;
        // SetterでModsDataにデータセットされます
        public bool Original
        {
            get { return original; }
            set
            {
                SetProperty(ref original, value);
                if (Original)
                {
                    mainTabPropertyModel.ModsData.First(x => x.Mod == modName).Original = "〇";
                }
                else
                {
                    mainTabPropertyModel.ModsData.First(x => x.Mod == modName).Original = "×";
                }
            }
        }

        private string nextOrFinish = "Next";
        public string NextOrFinish
        {
            get { return nextOrFinish; }
            set { SetProperty(ref nextOrFinish, value); }
        }



        private int position = 1;
        // ModsDataのうち何個目のCheckedのデータを変更するか
        // Exit時や全情報更新終了時に1に戻す
        public int Position
        {
            get { return position; }
            set { SetProperty(ref position, value); }
        }
        public void ChangeModInfo()
        {
            // 何個目のCheckedか
            int count = 0;
            int AllCheckedMod = mainTabPropertyModel.ModsData.Count(x => x.Checked == true);

            foreach (var a in mainTabPropertyModel.ModsData)
            {
                // Finishボタン押したとき
                if (Position > AllCheckedMod)
                {
                    break;
                }

                if (a.Checked)
                {
                    count++;

                    if (count == AllCheckedMod)
                    {
                        NextOrFinish = "Finish";
                    }

                    if (count == Position)
                    {
                        modName = a.Mod;

                        ModNameAndProgress = a.Mod + "(" + Position.ToString()
                        + "/" + AllCheckedMod.ToString() + ")";
                        Url = a.Url;
                        if (a.Original == "?" || a.Original == "〇")
                        {
                            Original = true;
                        }
                        else
                        {
                            Original = false;
                        }

                        Position++;

                        // ここで表示されるViewでNext/Finishボタンを押すとChangeModInfoが再帰的に呼び出される
                        // Exitの場合は再帰的な呼び出しはない
                        dialogService.ShowDialog("ChangeModInfo");
                        break;
                    }
                }
            }

            // 全情報更新終了したので
            Position = 1;
            NextOrFinish = "Next";
        }
    }
}
