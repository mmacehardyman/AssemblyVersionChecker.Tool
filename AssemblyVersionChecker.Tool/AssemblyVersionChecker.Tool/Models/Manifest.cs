using System.Collections.Generic;

namespace AssemblyVersionChecker.Tool.Models
{
    public class Manifest
    {
        public Manifest(string version)
        {
            Version = version;
            Assemblies = new List<Assembly>();
        }

        public string Version { get; set; }

        public List<Assembly> Assemblies { get; set; }
    }
}
