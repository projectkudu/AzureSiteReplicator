using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace AzureSiteReplicator.Data
{
    public class Site
    {
        private string _profilePath;
        private string _sitePath;
        private PublishSettings _settings;

        public Site(string profilePath)
        {
            _profilePath = profilePath;
            _settings = new PublishSettings(_profilePath);

            string rootPath = Path.GetDirectoryName(_profilePath);
            _sitePath = Path.Combine(rootPath, _settings.SiteName);

            if (!FileHelper.FileSystem.Directory.Exists(_sitePath))
            {
                FileHelper.FileSystem.Directory.CreateDirectory(_sitePath);
            }
        }

        public string Name
        {
            get { return Settings.SiteName; }
        }

        public StatusFile Status
        {
            get
            {
                StatusFile status = new StatusFile(Settings.SiteName);
                status.LoadOrCreate();
                return status;
            }
        }

        public LogFile GetLogFile(bool readOnly)
        {
            return new LogFile(Settings.SiteName, readOnly);
        }

        public PublishSettings Settings
        {
            get
            {
                return _settings;
            }
        }

        public void Delete()
        {
            if (FileHelper.FileSystem.File.Exists(_profilePath))
            {
                FileHelper.FileSystem.File.Delete(_profilePath);
            }

            // We only really need to delete the profile.  If for
            // some reason we fail to delete the sites directory
            // that should be okay.

            try
            {
                if (FileHelper.FileSystem.Directory.Exists(_sitePath))
                {
                    FileHelper.FileSystem.Directory.Delete(_sitePath, true);
                }
            }
            catch (IOException e)
            {
                Trace.TraceError("Failed to delete sites directory: {0}", e.ToString());
            }
        }

        public string FilePath
        {
            get { return _sitePath; }
        }

        public override bool Equals(object obj)
        {
            Site site = obj as Site;

            return site != null &&
                string.Equals(Name, site.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}