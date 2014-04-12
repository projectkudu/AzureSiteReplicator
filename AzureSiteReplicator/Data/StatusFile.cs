using AzureSiteReplicator.Contracts;
using AzureSiteReplicator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace AzureSiteReplicator.Data
{
    public class StatusFile : IStatusFile, IDisposable
    {
        private string _siteName;
        private string _filePath;
        private DeployState _state;

        public DeployState State
        {
            get
            {
                return _state;
            }

            set
            {
                if (value == DeployState.Failed ||
                   value == DeployState.Succeeded)
                {
                    EndTime = DateTime.UtcNow;

                    Complete = true;
                }
                else
                {
                    if (value == DeployState.Deploying)
                    {
                        StartTime = DateTime.UtcNow;
                    }

                    Complete = false;
                }

                _state = value;
            }
        }

        public string Name
        {
            get { return _siteName; }
        }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool Complete { get; set; }
        public int ObjectsAdded { get; set; }
        public int ObjectsUpdated { get; set; }
        public int ObjectsDeleted { get; set; }
        public int ParametersChanged { get; set; }
        public long BytesCopied { get; set; }

        public StatusFile(string siteName)
        {
            _siteName = siteName;
            _filePath = Path.Combine(
                            Environment.Instance.SiteReplicatorPath,
                            siteName);

            _filePath = Path.Combine(_filePath, "status.xml");

            _state = DeployState.NotStarted;
            StartTime = DateTime.MinValue;
            EndTime = DateTime.MinValue;
        }

        public void LoadOrCreate()
        {
            XmlDocument doc = new XmlDocument();
            XPathNavigator nav = null;
            bool hasMore;
            int num;

            if (!FileHelper.FileSystem.File.Exists(_filePath))
            {
                Save();
                return;
            }

            using (Stream stream =
                FileHelper.FileSystem.File.OpenRead(_filePath))
            {
                doc = new XmlDocument();
                doc.Load(stream);
            }

            nav = doc.CreateNavigator();
            nav.MoveToFirstChild();
            hasMore = nav.MoveToFirstChild();

            while (hasMore)
            {
                if (string.Equals(nav.Name, "state", StringComparison.OrdinalIgnoreCase))
                {
                    DeployState status = DeployState.NotStarted;
                    if (Enum.TryParse<DeployState>(nav.Value, true, out status))
                    {
                        State = status;
                    }
                }
                else if (string.Equals(nav.Name, "startTime", StringComparison.OrdinalIgnoreCase))
                {
                    DateTime startTime = DateTime.MinValue;
                    if (DateTime.TryParse(nav.Value, out startTime))
                    {
                        StartTime = startTime;
                    }
                }
                else if (string.Equals(nav.Name, "endTime", StringComparison.OrdinalIgnoreCase))
                {
                    DateTime endTime = DateTime.MinValue;
                    if (DateTime.TryParse(nav.Value, out endTime))
                    {
                        EndTime = endTime;
                    }
                }
                else if (string.Equals(nav.Name, "complete", StringComparison.OrdinalIgnoreCase))
                {
                    bool complete;
                    if (bool.TryParse(nav.Value, out complete))
                    {
                        Complete = complete;
                    }
                }
                else if (string.Equals(nav.Name, "objectsAdded", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(nav.Value, out num))
                    {
                        ObjectsAdded = num;
                    }
                }
                else if (string.Equals(nav.Name, "objectsUpdated", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(nav.Value, out num))
                    {
                        ObjectsUpdated = num;
                    }
                }
                else if (string.Equals(nav.Name, "objectsDeleted", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(nav.Value, out num))
                    {
                        ObjectsDeleted = num;
                    }
                }
                else if (string.Equals(nav.Name, "parametersChanged", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(nav.Value, out num))
                    {
                        ParametersChanged = num;
                    }
                }
                else if (string.Equals(nav.Name, "bytesCopied", StringComparison.OrdinalIgnoreCase))
                {
                    long longNum;
                    if (long.TryParse(nav.Value, out longNum))
                    {
                        BytesCopied = longNum;
                    }
                }

                hasMore = nav.MoveToNext();
            }
        }

        public void Save()
        {
            XDocument doc = new XDocument(
                new XElement("status",
                    new XElement("state", State.ToString()),
                    new XElement("startTime", StartTime.ToString()),
                    new XElement("endTime", EndTime.ToString()),
                    new XElement("complete", Complete),
                    new XElement("objectsAdded", ObjectsAdded),
                    new XElement("objectsUpdated", ObjectsUpdated),
                    new XElement("objectsDeleted", ObjectsDeleted),
                    new XElement("parametersChanged", ParametersChanged),
                    new XElement("bytesCopied", BytesCopied)));

            string profileDir = Path.Combine(Environment.Instance.SiteReplicatorPath, _siteName);
            if (!FileHelper.FileSystem.Directory.Exists(_siteName))
            {
                FileHelper.FileSystem.Directory.CreateDirectory(profileDir);
            }

            using (Stream stream = FileHelper.FileSystem.File.Open(
                                        _filePath,
                                        FileMode.Create,
                                        FileAccess.ReadWrite,
                                        FileShare.Read))
            {
                doc.Save(stream);
            }
        }

        public void Dispose()
        {
            Save();
        }
    }
}