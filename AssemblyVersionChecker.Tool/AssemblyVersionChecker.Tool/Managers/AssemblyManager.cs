using AssemblyVersionChecker.Tool.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AssemblyVersionChecker.Tool.Managers
{
    public class AssemblyManager
    {
        public IEnumerable<Assembly> ScanDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                throw new IOException($"Directory '{directory}' does not exist!");
            }

            foreach (var file in new DirectoryInfo(directory).GetFiles("*.dll"))
            {
                var fileVersion = FileVersionInfo.GetVersionInfo(file.FullName).FileVersion;
                yield return new Assembly(file.Name, fileVersion);
            }
        }
    }
}