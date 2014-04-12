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
    public class SkipRule
    {
        private string _expression = string.Empty;
        public string Expression
        {
            get { return _expression; }
            set { _expression = value; }
        }
        public bool IsDirectory { get; set; }
    }

    public class ConfigFile
    {
        private string _filePath;
        private volatile List<SkipRule> _skipRules = new List<SkipRule>();

        public ConfigFile()
        {
            _filePath = Path.Combine(Environment.Instance.SiteReplicatorPath, "config.xml");
        }

        public void LoadOrCreate()
        {
            XmlDocument doc = new XmlDocument();
            XPathNavigator nav = null;
            bool hasMore;
            List<SkipRule> newSkips = new List<SkipRule>();

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
                if (string.Equals(nav.Name, "skipRules", StringComparison.OrdinalIgnoreCase))
                {
                    bool hasMoreSkips = nav.MoveToFirstChild();
                    if (hasMoreSkips)
                    {
                        while (hasMoreSkips)
                        {
                            SkipRule rule = new SkipRule();
                            rule.Expression = nav.Value;

                            if (nav.MoveToFirstAttribute())
                            {
                                if (string.Equals(nav.Name, "isDirectory", StringComparison.OrdinalIgnoreCase))
                                {
                                    bool isDir = false;
                                    bool.TryParse(nav.Value, out isDir);
                                    rule.IsDirectory = isDir;
                                }

                                nav.MoveToParent();
                            }
                            
                            newSkips.Add(rule);
                            hasMoreSkips = nav.MoveToNext();
                        }

                        _skipRules = newSkips;
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

            IEnumerable<SkipRule> skips = SkipRules;
            XElement skipFiles = new XElement("skipRules");
            foreach (SkipRule skip in skips)
            {
                XElement skipRule =
                    new XElement("skipRule", new XAttribute("IsDirectory", skip.IsDirectory), skip.Expression);
                skipFiles.Add(skipRule);
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

        public IReadOnlyCollection<SkipRule> SkipRules
        {
            get
            {
                var skipFiles = _skipRules;
                return skipFiles.AsReadOnly();
            }
        }

        public void AddSkip(SkipRule skip)
        {
            List<SkipRule> skips = new List<SkipRule>(_skipRules);
            skips.Add(skip);
            _skipRules = skips;
        }

        public void SetSkips(List<SkipRule> skips)
        {
            List<SkipRule> newList = new List<SkipRule>();
            if (skips != null)
            {
                newList.AddRange(skips);
            }
            _skipRules = newList;
        }

        public void ClearSkips()
        {
            List<SkipRule> skips = new List<SkipRule>();
            _skipRules = skips;
        }

        public void RemoveSkip(SkipRule skip)
        {
            List<SkipRule> skips = new List<SkipRule>(_skipRules);
            int index = skips.FindIndex
            (
                m => string.Equals(m.Expression, skip.Expression, StringComparison.OrdinalIgnoreCase)
            );

            if (index > -1)
            {
                skips.RemoveAt(index);
            }

            _skipRules = skips;
        }
    }
}