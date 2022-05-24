using BSModManager.Static;
using System.IO;
using System.Text;

namespace BSModManager.Models
{
    public static class GameVersion
    {
        public static string DisplayedVersion
        {
            get { return "GameVersion\n" + Get(); }
        }

        public static string Version
        {
            get { return Get(); }
        }

        public static string Get()
        {
            string filename = Path.Combine(Folder.Instance.BSFolderPath, "Beat Saber_Data", "globalgamemanagers");

            if (!File.Exists(filename)) return "---";

            using (var stream = File.OpenRead(filename))
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
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
                    var current = (char)reader.ReadByte();
                    if (char.IsDigit(current))
                        break;
                }

                var rewind = -sizeof(int) - sizeof(byte);
                stream.Seek(rewind, SeekOrigin.Current); // rewind to the string length

                var strlen = reader.ReadInt32();
                var strbytes = reader.ReadBytes(strlen);

                return Encoding.UTF8.GetString(strbytes);
            }
        }
    }
}
