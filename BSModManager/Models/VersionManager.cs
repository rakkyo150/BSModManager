using System;
using System.IO;
using System.Text;

namespace BSModManager.Models
{
    public class VersionManager
    {
        public string GetGameVersion(string bSFolderPath)
        {
            try
            {
                string filename = Path.Combine(bSFolderPath, "Beat Saber_Data", "globalgamemanagers");
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

                    return "GameVersion\n" + Encoding.UTF8.GetString(strbytes);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "GameVersion\n---";
            }
        }

        public string GetMyselfVersion()
        {
            System.Reflection.Assembly assembly =System.Reflection.Assembly.GetExecutingAssembly();
            System.Version version = assembly.GetName().Version;
            return "Version\n"+version.ToString(3);
        }
    }
}
