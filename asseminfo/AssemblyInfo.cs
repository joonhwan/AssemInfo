using System;

namespace AssemInfo
{
    public class AssemblyInfo
    {
        public string Path { get; set; }
        public string Version { get; set; }
        public string MinimumServerVersion { get; set; }
        public string FullName { get; set; }

        public override string ToString()
        {
            return String.Format("Path: {0}, Version: {1}, MinimumServerVersion: {2}, FullName: {3}", Path, Version,
                MinimumServerVersion, FullName);
        }
    }
}