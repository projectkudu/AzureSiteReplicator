using AzureSiteReplicator.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureSiteReplicator.Models
{
    public enum DeployState{
        NotStarted,
        Deploying,
        Failed,
        Succeeded
    }

    public class SiteStatusModel
    {
        private IStatusFile _statusFile;

        public SiteStatusModel(IStatusFile statusFile)
        {
            _statusFile = statusFile;
        }

        public string Name
        {
            get
            {
                return _statusFile.Name;
            }
        }

        public string State 
        {
            get
            {
                return _statusFile.State.ToString();
            }
            set
            {
                _statusFile.State = (DeployState)Enum.Parse(typeof(DeployState), value);
            }
        }

        public DateTime StartTime 
        {
            get
            {
                return _statusFile.StartTime;
            }
        }

        public DateTime EndTime
        {
            get
            {
                return _statusFile.EndTime;
            }
        }

        public bool Complete
        {
            get
            {
                return _statusFile.Complete;
            }
        }

        public int ChangesAdded
        {
            get
            {
                return _statusFile.ObjectsAdded;
            }

            set
            {
                _statusFile.ObjectsAdded = value;
            }
        }

        public int ChangesUpdated
        {
            get
            {
                return _statusFile.ObjectsUpdated;
            }
            set
            {
                _statusFile.ObjectsUpdated = value;
            }
        }

        public int ChangesDeleted
        {
            get
            {
                return _statusFile.ObjectsDeleted;
            }
            set
            {
                _statusFile.ObjectsDeleted = value;
            }
        }

        public long BytesCopied
        {
            get
            {
                return _statusFile.BytesCopied;
            }
            set
            {
                _statusFile.BytesCopied = value;
            }
        }

        internal void Save()
        {
            _statusFile.Save();
        }
    }
}