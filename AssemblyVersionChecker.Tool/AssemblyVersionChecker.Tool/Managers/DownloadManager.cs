using Alienlab.NetExtensions;
using Sitecore.Diagnostics.InfoService.Client.Model;
using System;
using System.IO;
using System.Linq;

namespace AssemblyVersionChecker.Tool.Managers
{
    public class DownloadManager
    {
        public event Action<DownloadContext.Block, long, int> OnProgressChanged;

        public event Action<Exception> OnErrorOccurred;

        public event Action<DownloadResult> OnDownloadCompleted;

        public void Download(IRelease release, string cookie, string tempDir)
        {
            if (release == null)
            {
                throw new ArgumentNullException(nameof(release));
            }

            if (string.IsNullOrWhiteSpace(cookie))
            {
                throw new ArgumentNullException(nameof(cookie));
            }

            if (release.DefaultDistribution == null)
            {
                throw new InvalidOperationException("Release contains no valid default distribution");
            }

            var url = new Uri(release.DefaultDistribution.Downloads.First(x => x.StartsWith("http")));
            var filename = release.DefaultDistribution.FileNames.First(x => x.EndsWith("zip"));
            var filepath = Path.Combine(tempDir, filename);

            if (File.Exists(filepath))
            {
                // Release has already been downloaded previously; just report back with the filepath
                OnDownloadCompleted?.Invoke(new DownloadResult(filepath, release));
                return;
            }

            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            var downloadOptions = new DownloadOptions
            {
                Uri = url,
                BlockOptions = new BlocksCount(1),
                Cookies = cookie,
                FilePath = filepath,
                RequestTimeout = new TimeSpan(0, 24, 0, 0)
            };

            var context = new DownloadContext(downloadOptions);

            if (OnProgressChanged != null)
            {
                context.OnProgressChanged += OnProgressChanged;
            }

            if (OnDownloadCompleted != null)
            {
                context.OnDownloadCompleted += () => { OnDownloadCompleted?.Invoke(new DownloadResult(filepath, release)); };
            }

            if (OnErrorOccurred != null)
            {
                context.OnErrorOccurred += OnErrorOccurred;
            }

            context.Download();
        }
    }

    public class DownloadResult
    {
        public DownloadResult(string file, IRelease release)
        {
            File = file;
            Release = release;
        }

        public string File { get; }

        public IRelease Release { get; }

        public string ReleaseVersion => Release != null ? Release.Version.ToString() : string.Empty;
    }
}