using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureSiteReplicator.Contracts
{
    public interface IEnvironment
    {
        string ContentPath { get; set; }
        string SiteReplicatorPath { get; set; }
    }
}