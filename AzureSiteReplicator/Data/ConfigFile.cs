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
    public class ConfigFile
    {
        private string _filePath;
        private volatile List<string> _skipFiles = new List<string>();

        public ConfigFile()
        {
            _filePath = Path.Combine(Environment.Instance.SiteReplicatorPath, "config.xml");
        }

        public void LoadOrCreate()
        {
            XmlDocument doc = new XmlDocument();
            XPathNavigator nav = null;
            bool hasMore;
            List<string> newSkips = new List<string>();

            ClearSkips();

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
                if (string.Equals(nav.Name, "skipFiles", StringComparison.OrdinalIgnoreCase))
                {
                    bool hasMoreSkips = nav.MoveToFirstChild();
                    if (hasMoreSkips)
                    {
                        while (hasMoreSkips)
                        {
                            newSkips.Add(nav.Value);
                            hasMoreSkips = nav.MoveToNext();
                        }

                        _skipFiles = newSkips;
                        nav.MoveToParent();
                    }
                }

                hasMore = nav.MoveToNext();
            }
        }

        public void Save()
        {
            XDocument doc = new XDocument();
            XElement root = new XElement("config");

            IEnumerable<string> skips = SkipFiles;
            XElement skipFiles = new XElement("skipFiles");
            foreach (string skip in skips)
            {
                skipFiles.Add(new XElement("skip", skip));
            }

            if (skipFiles.HasElements)
            {
                root.Add(skipFiles);
            }

            doc.Add(root);

            using (Stream stream = 
                FileHelper.FileSystem.File.Open(
                    _filePath,
                    FileMode.Create,
                    FileAccess.ReadWrite,
                    FileShare.Read))
            {
                doc.Save(stream);
            }
        }

        public IEnumerable<string> SkipFiles
        {
            get
            {
                var skipFiles = _skipFiles;
                return skipFiles.AsEnumerable();
            }
        }

        public void AddSkip(string skip)
        {
            List<string> skips = new List<string>(_skipFiles);
            skips.Add(skip);
            _skipFiles = skips;
        }

        public void SetSkips(List<string> skips)
        {
            List<string> newList = new List<string>();
            if (skips != null)
            {
                newList.AddRange(skips);
            }
            _skipFiles = newList;
        }

        public void ClearSkips()
        {
            List<string> skips = new List<string>();
            _skipFiles = skips;
        }

        public void RemoveSkip(string skip)
        {
            List<string> skips = new List<string>(_skipFiles);
            skips.Remove(skip);
            _skipFiles = skips;
        }
    }
}