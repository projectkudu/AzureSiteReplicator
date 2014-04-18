using AzureSiteReplicator.Contracts;
using AzureSiteReplicator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AzureSiteReplicator.Data
{
    public class ConfigRepository : IConfigRepository
    {
        private ConfigFile _config;
        private volatile HashSet<Site> _sites;

        public ConfigRepository()
        {
            _config = new ConfigFile();
            _config.LoadOrCreate();

            var profileFilePaths =
                FileHelper.FileSystem.Directory.GetFiles(
                    Environment.Instance.SiteReplicatorPath, "*.publishSettings");

            _sites = new HashSet<Site>();
            foreach (var profilePaths in profileFilePaths)
            {
                _sites.Add(new Site(profilePaths));
            }

        }

        public ConfigFile Config
        {
            get
            {
                return _config;
            }
        }

        public IEnumerable<Site> Sites
        {
            get
            {
                HashSet<Site> sites = _sites;
                return sites;
            }
        }

        public void AddSite(string profilePath)
        {
            HashSet<Site> sites = new HashSet<Site>(_sites);
            Site newSite = new Site(profilePath);
            if (!sites.Contains(newSite))
            {
                sites.Add(newSite);
            }

            _sites = sites;
        }

        public void RemoveSite(string siteName)
        {
            HashSet<Site> sites = _sites;

            Site siteToRemove = sites.FirstOrDefault((m) =>
            {
                return string.Equals(siteName, m.Name, StringComparison.OrdinalIgnoreCase);
            });

            if (siteToRemove != null)
            {
                siteToRemove.Delete();
                sites = new HashSet<Site>(sites);
                sites.Remove(siteToRemove);
            }

            _sites = sites;
        }
    }
}