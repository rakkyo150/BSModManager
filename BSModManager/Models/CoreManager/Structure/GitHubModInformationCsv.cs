﻿using CsvHelper.Configuration.Attributes;

namespace BSModManager.Models.Structure
{
    public class GitHubModInformationCsv
    {
        [Index(0)]
        public string GithubMod { get; set; }
        [Index(1)]
        public string LocalVersion { get; set; }
        [Index(2)]
        public string GithubVersion { get; set; }
        [Index(3)]
        public bool OriginalMod { get; set; }
        [Index(4)]
        public string GithubUrl { get; set; }
    }
}