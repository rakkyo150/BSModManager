using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSModManager.Models
{
    internal class LocalModFile
    {
        private readonly string _modName;
        private readonly Version _version;
        private readonly string _fileHash;

        internal LocalModFile(string modName,Version version,string fileHash)
        {
            _modName = modName;
            _version = version;
            _fileHash = fileHash;
        }

        internal string ModName => _modName;
        internal Version Version => _version;
        internal string FileHash => _fileHash;


    }
}
