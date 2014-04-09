using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureSiteReplicator.Models
{
    public class ReplicationInfoModel
    {
        public IEnumerable<string> SkipFiles { get; set; }
        //public List<PublishSettingsModel> PublishSettings { get; set; }
        public IReadOnlyCollection<SiteStatusModel> SiteStatuses { get; set; }
    }
}