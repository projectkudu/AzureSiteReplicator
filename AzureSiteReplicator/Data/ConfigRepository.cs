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
        private volatile ConfigFile _config;
        private volatile List<SiteStatusModel> _siteStatuses;

        public ConfigFile Config
        {
            get
            {
                ConfigFile config = _config;
                if (config == null)
                {
                    config = new ConfigFile();
                    config.LoadOrCreate();
                    _config = config;
                }

                return config;
            }
        }

        public IReadOnlyCollection<SiteStatusModel> SiteStatuses
        {
            get
            {
                List<SiteStatusModel> statuses = _siteStatuses;
                if (statuses == null)
                {
                    statuses = new List<SiteStatusModel>();
                    
                    var profileNames = FileHelper.FileSystem.Directory.GetFiles(Environment.Instance.SiteReplicatorPath, "*.publishSettings")
                            .Select(path => FileHelper.GetProfileNameFromFileName(path));

                    foreach (var profileName in profileNames)
                    {
                        StatusFile file = new StatusFile(profileName);

                        file.LoadOrCreate();
                        statuses.Add(new SiteStatusModel(file));
                    }

                    _siteStatuses = statuses;
                }

                return statuses.AsReadOnly();
            }
        }

        public void Reset()
        {
            _siteStatuses = null;
            _config = null;
        }
    }
}