using System.Collections.Generic;

namespace AssemblyVersionChecker.Tool.Models
{
    public class ManifestInformation
    {
        public ManifestInformation()
        {
            Manifests = new List<Manifest>();
        }
        
        public List<Manifest> Manifests { get; set; }
    }
}
