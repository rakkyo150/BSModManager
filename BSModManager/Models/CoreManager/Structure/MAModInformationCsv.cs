using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSModManager.Models.Structure
{
    public class MAModInformationCsv
    {
        [Index(0)]
        public string ModAssistantMod { get; set; }
        [Index(1)]
        public string LocalVersion { get; set; }
        [Index(2)]
        public string ModAssistantVersion { get; set; }
    }
}
