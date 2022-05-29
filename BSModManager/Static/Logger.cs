using Prism.Mvvm;
using System;
using System.IO;
using System.Text;
using System.Windows;

namespace BSModManager.Static
{
    internal class Logger : BindableBase
    {
        internal static Logger Instance { get; set; } = new Logger();

        private string allLog = string.Empty;

        private string LogFilePath
        {
            get { return Path.Combine(Folder.Instance.logFolder, $"{DateTime.Now:yyyy.MM.dd.HH.mm.ss}.log"); }
        }

        private string infoLog = string.Empty;
        public string InfoLog
        {
            get { return infoLog; }
            set
            {
                SetProperty(ref infoLog, value);
                allLog += $"[Info @ {DateTime.Now:HH:mm:ss}] " + value + "\n";
            }
        }

        internal void Info(string info)
        {
            InfoLog = info;
        }

        internal void Debug(string debug)
        {
            allLog += $"[Debug @ {DateTime.Now:HH:mm:ss}] " + debug + "\n";
        }

        internal void Error(string error)
        {
            allLog += $"[Error @ {DateTime.Now:HH:mm:ss}] " + error + "\n";
            MessageBox.Show($"{error}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        internal void GenerateLogFile()
        {
            if (!Directory.Exists(Folder.Instance.logFolder))
            {
                Directory.CreateDirectory(Folder.Instance.logFolder);
            }

            try
            {
                Encoding enc = Encoding.GetEncoding("Shift_JIS");
                using (StreamWriter streamWriter = new StreamWriter(LogFilePath, false, enc))
                {
                    streamWriter.WriteLine(allLog);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}",
                        "ログファイル作成失敗", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
