using AssemblyVersionChecker.Tool.Models;
using CommandLine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AssemblyVersionChecker.Tool
{
    class Program
    {
        public class Options
        {
            [Option('p', "path", Required = true, HelpText = "Set path to Sitecore bin directory.")]
            public string Path { get; set; }

            [Option('v', "version", Required = true, HelpText = "Set version of Sitecore.")]
            public string Version { get; set; }

            [Option('m', "manifest", Required = true, HelpText = "Set path to manifest file.")]
            public string Manifest { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(RunOptions);
        }

        private static void RunOptions(Options opts)
        {
            var manifestInformation = new ManifestInformation();

            if (File.Exists(opts.Manifest))
            {
                manifestInformation = JsonConvert.DeserializeObject<ManifestInformation>(File.ReadAllText(opts.Manifest));
            }

            var version = manifestInformation.Manifests.FirstOrDefault(x => x.Version == opts.Version);
            if (version == null)
            {
                version = new Manifest {Assemblies = new List<Assembly>(), Version = opts.Version};
                manifestInformation.Manifests.Add(version);
            }
            else
            {
                version.Assemblies = new List<Assembly>();
            }

            if (!Directory.Exists(opts.Path))
            {
                throw new IOException($"Directory '{opts.Path}' does not exist!");
            }

            foreach (var file in new DirectoryInfo(opts.Path).GetFiles("*.dll"))
            {
                version.Assemblies.Add(new Assembly
                {
                    Name = file.Name,
                    Version = FileVersionInfo.GetVersionInfo(file.FullName).FileVersion
                });
            }

            File.WriteAllText(opts.Manifest, JsonConvert.SerializeObject(manifestInformation));
        }
    }
}