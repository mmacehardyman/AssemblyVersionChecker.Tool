using Sitecore.Diagnostics.InfoService.Client;
using Sitecore.Diagnostics.InfoService.Client.Model;
using System.Collections.Generic;
using System.Linq;

namespace AssemblyVersionChecker.Tool.Managers
{
    public class ReleaseManager
    {
        private readonly IServiceClient _serviceClient;

        public ReleaseManager()
        {
            _serviceClient = ServiceClient.Create();
        }

        public List<IRelease> GetReleases()
        {
            return _serviceClient.GetVersions("Sitecore CMS").Where(x => x.Version.Major >= 8).ToList();
        }

        public IRelease GetRelease(string version)
        {
            return _serviceClient.GetVersions("Sitecore CMS").FirstOrDefault(x => x.Version.MajorMinorUpdate == version);
        }
    }
}
