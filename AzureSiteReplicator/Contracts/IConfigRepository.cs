using AzureSiteReplicator.Data;
using AzureSiteReplicator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSiteReplicator.Contracts
{
    public interface IConfigRepository
    {
        ConfigFile Config { get; }
        IEnumerable<Site> Sites { get; }
        void AddSite(string site);
        void RemoveSite(string name);
    }
}
