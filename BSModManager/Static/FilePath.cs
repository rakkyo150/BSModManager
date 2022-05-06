using System;
using System.IO;
using System.Windows.Forms;

namespace BSModManager.Static
{
    public static class FilePath
    {
        public readonly static string configFilePath = Path.Combine(Environment.CurrentDirectory, "config.json");
        public readonly static string mAModCsvPath = Path.Combine(FolderManager.dataFolder, "ModAssistantModData.csv");

		public static string SelectFile(string previouPath)
		{
			// ダイアログのインスタンスを生成
			var dialog = new OpenFileDialog();

			// ファイルの種類を設定
			dialog.Filter = "exeファイル (*.exe)|*.exe";

			// ダイアログを表示する
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				// 選択されたファイル名 (ファイルパス) をメッセージボックスに表示
				Console.WriteLine(dialog.FileName);
				return dialog.FileName;
			}

			return previouPath;
		}
	}
}
