using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace AssemblyVersionChecker.Tool.Models
{
    public class ManifestInformation
    {
        public ManifestInformation()
        {
            Manifests = new List<Manifest>();
        }
        
        public List<Manifest> Manifests { get; set; }

        public static ManifestInformation LoadFromFileOrCreateNew(string file)
        {
            if (!File.Exists(file))
            {
                return new ManifestInformation();
            }

            return JsonConvert.DeserializeObject<ManifestInformation>(File.ReadAllText(file));
        }
    }
}
