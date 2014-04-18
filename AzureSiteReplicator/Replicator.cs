using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Web.Deployment;
using AzureSiteReplicator.Contracts;
using AzureSiteReplicator.Data;

namespace AzureSiteReplicator
{
    public class Replicator
    {
        public static Replicator Instance = new Replicator();

        private int _inUseCount;
        private DateTime _lastChangeTime;
        private DateTime _publishStartTime;
        private IConfigRepository _repository;

        public Replicator()
        {
            var fileSystemWatcher = new FileSystemWatcher(Environment.Instance.ContentPath);
            fileSystemWatcher.Created += OnChanged;
            fileSystemWatcher.Changed += OnChanged;
            fileSystemWatcher.Deleted += OnChanged;
            fileSystemWatcher.Renamed += OnChanged;
            fileSystemWatcher.Error += OnError;
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.EnableRaisingEvents = true;
            _repository = new ConfigRepository();
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Trace.TraceInformation("{0} OnChanged {1} {2}", DateTime.Now, e.FullPath, e.ChangeType);
            TriggerDeployment();
        }

        public void TriggerDeployment()
        {
            // Start the publishing async, but don't wait for it. Just fire and forget.
            Task task = PublishAsync();
        }

        public async Task PublishAsync()
        {
            // This prevents running into null ref issue
            // http://stackoverflow.com/questions/16056016/nullreferenceexception-in-system-threading-tasks-calling-httpclient-getasyncurl
            await Task.Delay(1).ConfigureAwait(false);

            _lastChangeTime = DateTime.Now;

            if (Interlocked.Increment(ref _inUseCount) == 1)
            {
                _publishStartTime = DateTime.MinValue;

                // Keep publishing as long as some changes happened after we started the previous publish
                while (_publishStartTime < _lastChangeTime)
                {
                    // Wait till there are no change notifications for a while, so we don't start deploying while
                    // files are still being copied to the source folder
                    while (DateTime.Now - _lastChangeTime < TimeSpan.FromMilliseconds(250))
                    {
                        await Task.Delay(100);
                    }

                    _publishStartTime = DateTime.Now;
                    try
                    {
                        await PublishContentToAllSites(
                            Environment.Instance.ContentPath,
                            Environment.Instance.SiteReplicatorPath);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError("Publishing failed: {0}", e.ToString());
                    }
                }
            }
            Interlocked.Decrement(ref _inUseCount);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Trace.TraceError(e.GetException().ToString());
        }

        private async Task PublishContentToAllSites(
            string contentPath,
            string siteReplicatorPath)
        {
            // Publish to all the target sites in parallel
            var allChanges = await Task.WhenAll(Instance.Repository.Sites.Select(async site =>
            {
                return await PublishContentToSingleSite(site);
            }));

            // Trace all the results
            for (int i = 0; i < allChanges.Length; i++)
            {
                DeploymentChangeSummary changeSummary = allChanges[i];
                if (changeSummary == null) continue;

                Trace.TraceInformation("Processed sites: {0}", Instance.Repository.Sites.Count());
                Trace.TraceInformation("BytesCopied: {0}", changeSummary.BytesCopied);
                Trace.TraceInformation("Added: {0}", changeSummary.ObjectsAdded);
                Trace.TraceInformation("Updated: {0}", changeSummary.ObjectsUpdated);
                Trace.TraceInformation("Deleted: {0}", changeSummary.ObjectsDeleted);
                Trace.TraceInformation("Errors: {0}", changeSummary.Errors);
                Trace.TraceInformation("Warnings: {0}", changeSummary.Warnings);
                Trace.TraceInformation("Total changes: {0}", changeSummary.TotalChanges);
            }
        }

        public async Task<DeploymentChangeSummary> PublishContentToSingleSite(Site site)
        {
            string lockPath = Path.Combine(site.FilePath, "deployment.lock");
            LockFile lockFile = null;

            Trace.TraceInformation("Sync to single site: {0}", site.Name);

            try
            {
                if (LockFile.TryGetLockFile(lockPath, out lockFile))
                {
                    WebDeployHelper webDeployHelper = new WebDeployHelper();
                    return await Task<DeploymentChangeSummary>.Run(() =>
                        webDeployHelper.DeployContentToOneSite(
                            Repository,
                            Environment.Instance.ContentPath,
                            site.Settings.FilePath));
                }

                return null;
            }
            catch (Exception e)
            {
                Trace.TraceError("Error processing {0}: {1}", Path.GetFileName(site.Settings.FilePath), e.ToString());
                return null;
            }
            finally
            {
                if (lockFile != null)
                {
                    lockFile.Dispose();
                    lockFile = null;
                }
            }

        }

        public IConfigRepository Repository
        {
            get
            {
                return _repository;
            }
        }
    }
}
