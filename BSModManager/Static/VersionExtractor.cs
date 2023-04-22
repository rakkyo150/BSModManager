using System;
using System.IO;
using System.Text;

namespace BSModManager.Models
{
    public static class VersionExtractor
    {
        public static string DisplayedGameVersion => ("GameVersion\n" + GetGameVersion());
        public static string GameVersion => DetectVersionFromRawVersion(GetGameVersion()).ToString();

        public static string GetGameVersion()
        {
            string filename = Path.Combine(Config.Instance.BSFolderPath, "Beat Saber_Data", "globalgamemanagers");

            if (!File.Exists(filename)) return "---";

            using (FileStream stream = File.OpenRead(filename))
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8))
            {
                const string key = "public.app-category.games";
                int pos = 0;

                while (stream.Position < stream.Length && pos < key.Length)
                {
                    if (reader.ReadByte() == key[pos]) pos++;
                    else pos = 0;
                }

                if (stream.Position == stream.Length) // we went through the entire stream without finding the key
                    return null;

                while (stream.Position < stream.Length)
                {
                    char current = (char)reader.ReadByte();
                    if (char.IsDigit(current))
                        break;
                }

                int rewind = -sizeof(int) - sizeof(byte);
                stream.Seek(rewind, SeekOrigin.Current); // rewind to the string length

                int strlen = reader.ReadInt32();
                byte[] strbytes = reader.ReadBytes(strlen);

                return Encoding.UTF8.GetString(strbytes);
            }
        }

        public static Version DetectVersionFromRawVersion(string rawVersion)
        {
            Version version = null;

            if (rawVersion == null) return version;

            int versionInfoStartPosition = 0;
            foreach (char item in rawVersion)
            {
                if (item >= '0' && item <= '9')
                {
                    break;
                }
                versionInfoStartPosition++;
            }

            for (int versionInfoFinishPosition = 0; versionInfoFinishPosition <= rawVersion.Length - versionInfoStartPosition - 1; versionInfoFinishPosition++)
            {
                char versionDetector = rawVersion[versionInfoStartPosition + versionInfoFinishPosition];
                if (!(versionDetector >= '0' && versionDetector <= '9') && versionDetector != '.')
                {
                    version = new Version(rawVersion.Substring(versionInfoStartPosition, versionInfoFinishPosition));
                    return version;
                }
            }

            version = new Version(rawVersion.Substring(versionInfoStartPosition));
            return version;
        }
    }
}
