using System;
using System.IO;
using System.Windows.Forms;

namespace BSModManager.Static
{
    public class FilePath
    {
        public static FilePath Instance { get; set; } = new FilePath();

        public readonly string configFilePath = Path.Combine(Environment.CurrentDirectory, "config.json");
        public readonly string mAModCsvPath = Path.Combine(Folder.Instance.dataFolder, "ModAssistantModData.csv");

        public string SelectFile(string previouPath)
        {
            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog
            {
                // ファイルの種類を設定
                Filter = "exeファイル (*.exe)|*.exe"
            };

            // ダイアログを表示する
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // 選択されたファイル名 (ファイルパス) をメッセージボックスに表示
                return dialog.FileName;
            }

            return previouPath;
        }
    }
}
