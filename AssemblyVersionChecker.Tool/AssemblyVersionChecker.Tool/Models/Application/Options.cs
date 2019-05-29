using CommandLine;

namespace AssemblyVersionChecker.Tool.Models.Application
{
    public class Options
    {
        [Option('m', "manifest", Required = true, HelpText = "Set path to manifest file.")]
        public string Manifest { get; set; }

        // Manual scan options
        [Option('v', "version", Required = false, HelpText = "Set version of Sitecore to perform a manual scan in conjunction with the directory.")]
        public string Version { get; set; }

        [Option('d', "dir", Required = false, HelpText = "Set scan directory to perform a manual scan in conjunction with the version number.")]
        public string Directory { get; set; }

        // Automatic scan options
        [Option('u', "username", Required = true, HelpText = "Set username for downloading Sitecore.")]
        public string Username { get; set; }

        [Option('p', "password", Required = true, HelpText = "Set password for downloading Sitecore.")]
        public string Password { get; set; }

        [Option('t', "tempdir", Required = true, HelpText = "Set temp directory for downloaded assets.")]
        public string TempDir { get; set; }
    }
}
