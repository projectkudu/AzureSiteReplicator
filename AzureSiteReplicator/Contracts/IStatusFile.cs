using AzureSiteReplicator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSiteReplicator.Contracts
{
    public interface IStatusFile
    {
        DeployState State { get; set; }
        string Name { get; }
        DateTime StartTime { get; }
        DateTime EndTime { get;  }
        bool Complete { get;  }
        int ObjectsAdded { get; set; }
        int ObjectsUpdated { get; set; }
        int ObjectsDeleted { get; set; }
        int ParametersChanged { get; set; }
        long BytesCopied { get; set; }
        void Save();
    }
}
