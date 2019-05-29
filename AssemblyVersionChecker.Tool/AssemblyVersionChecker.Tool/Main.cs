using AssemblyVersionChecker.Tool.Managers;
using AssemblyVersionChecker.Tool.Models;
using AssemblyVersionChecker.Tool.Models.Application;
using Newtonsoft.Json;
using Sitecore.Diagnostics.InfoService.Client.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace AssemblyVersionChecker.Tool
{
    public class Main
    {
        private readonly ReleaseManager _releaseManager = new ReleaseManager();

        public void Run(Options opts)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Trying to load the manifest file '{opts.Manifest}' or create a new one if it does not exist");

            var manifestInformation = ManifestInformation.LoadFromFileOrCreateNew(opts.Manifest);

            bool result;
            if (!string.IsNullOrWhiteSpace(opts.Version) && !string.IsNullOrWhiteSpace(opts.Directory))
            {
                Console.WriteLine($"Performing manual scan using version {opts.Version} and scan directory '{opts.Directory}' ...");

                result = RunManualScan(opts, manifestInformation);
            }
            else
            {
                Console.WriteLine("Performing automatic scan ...");

                result = RunAutomaticScan(opts, manifestInformation);
            }

            if (result)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Saving the manifest file at '{opts.Manifest}'");

                File.WriteAllText(opts.Manifest, JsonConvert.SerializeObject(manifestInformation));
            }
        }

        private bool RunManualScan(Options opts, ManifestInformation manifestInformation)
        {
            Console.WriteLine($"Getting release information for {opts.Version}...");

            var release = _releaseManager.GetRelease(opts.Version);
            if (release == null || release.DefaultDistribution == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No release information available");

                return false;
            }

            var sitecoreVersionIdentifier = Path.GetFileNameWithoutExtension(release.DefaultDistribution.FileNames.First(x => x.EndsWith("zip")));

            ScanAssembliesAndUpdateManifest(manifestInformation, opts.TempDir, sitecoreVersionIdentifier, opts.Version);

            return true;
        }

        private bool RunAutomaticScan(Options opts, ManifestInformation manifestInformation)
        {
            Console.WriteLine("Getting release information...");

            var releases = _releaseManager.GetReleases();
            if (!releases.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No release information available");

                return false;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Authenticating...");

            var cookie = new CookieManager().GetMarketplaceCookie(opts.Username, opts.Password);

            foreach (var release in releases)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Downloading {release.ProductName} version {release.Version} (rev. {release.Revision})");

                DownloadRelease(release, cookie, manifestInformation, opts.TempDir);
            }

            return true;
        }

        private void DownloadRelease(IRelease release, string cookie, ManifestInformation manifestInformation, string tempDir)
        {
            var downloadManager = new DownloadManager();
            var progress = new ProgressBar();

            downloadManager.OnProgressChanged += (block, l, totalPercentage) =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                progress.Report((double)totalPercentage / 100);
            };

            downloadManager.OnDownloadCompleted += result =>
            {
                if (progress != null)
                {
                    progress.Dispose();
                    progress = null;
                }

                if (!File.Exists(result.File))
                {
                    throw new IOException($"File '{result.File}' does not exist");
                }

                var tempFile = $"{Path.GetFileNameWithoutExtension(result.File)} (copy).zip";
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                File.Copy(result.File, tempFile);

                var sitecoreVersionIdentifier = Path.GetFileNameWithoutExtension(result.File);

                if (ExtractAssemblies(tempDir, tempFile, sitecoreVersionIdentifier))
                {
                    ScanAssembliesAndUpdateManifest(manifestInformation, tempDir, sitecoreVersionIdentifier, result.ReleaseVersion);
                }

                File.Delete(tempFile);
            };

            downloadManager.Download(release, cookie, tempDir);
        }

        private bool ExtractAssemblies(string tempDirectory, string file, string sitecoreVersionIdentifier)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Extracting assemblies to temporary directory");

            try
            {
                using (var zip = ZipFile.OpenRead(file))
                {
                    foreach (var entry in zip.Entries.Where(e => e.FullName.StartsWith($"{sitecoreVersionIdentifier}/Website/bin/") && e.FullName.EndsWith("dll")))
                    {
                        var fileName = Path.Combine(tempDirectory, entry.FullName);
                        var directory = Path.GetDirectoryName(fileName);

                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        if (!File.Exists(fileName)) // Skip file if it already exists
                        {
                            entry.ExtractToFile(fileName);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error occurred extracting this zipfile, zipfile might be corrupt. Extract the files manually and perform a scan on this specific directory.");
                return false;
            }
        }

        private IEnumerable<Assembly> ScanAssemblies(string tempDirectory, string sitecoreVersionIdentifier)
        {
            var directoryToScan = Path.Combine(tempDirectory, $"{sitecoreVersionIdentifier}/Website/bin/");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Scanning the assemblies in directory '{directoryToScan}'");

            return new AssemblyManager().ScanDirectory(directoryToScan);
        }

        private void ScanAssembliesAndUpdateManifest(ManifestInformation manifestInformation, string tempDir, string sitecoreVersionIdentifier, string version)
        {
            var scannedAssemblies = ScanAssemblies(tempDir, sitecoreVersionIdentifier);

            var manifest = manifestInformation.Manifests.FirstOrDefault(x => x.Version == version);
            if (manifest == null)
            {
                manifest = new Manifest(version);
                manifestInformation.Manifests.Add(manifest);
            }
            else
            {
                manifest.Assemblies = new List<Assembly>();
            }

            manifest.Assemblies.AddRange(scannedAssemblies);
        }
    }
}
