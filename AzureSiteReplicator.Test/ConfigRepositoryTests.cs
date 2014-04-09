using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using AzureSiteReplicator.Contracts;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;
using AzureSiteReplicator.Data;
using AzureSiteReplicator.Models;

namespace AzureSiteReplicator.Test
{
    [TestClass]
    public class ConfigRepositoryTests
    {
        private Mock<IEnvironment> _mockEnv = null;

        [TestInitialize]
        public void Setup()
        {
            _mockEnv = new Mock<IEnvironment>();
            _mockEnv.Setup(m => m.SiteReplicatorPath).Returns(@"c:\");
            Environment.Instance = _mockEnv.Object;

            FileHelper.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        }


        [TestMethod]
        public void GetSiteStatusesTest()
        {
            FileHelper.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                { @"c:\site1.publishsettings", new MockFileData("<status />") },
                { @"c:\site2.foo.publishsettings", new MockFileData("<status />") },
                { @"c:\config.xml", new MockFileData("<config />")}
            });

            IConfigRepository repository = new ConfigRepository();

            List<SiteStatusModel> siteStatuses = new List<SiteStatusModel>(repository.SiteStatuses);

            Assert.AreEqual(2, siteStatuses.Count, "statuses found");
            Assert.AreEqual("site1", siteStatuses[0].Name, "site1");
            Assert.AreEqual("site2", siteStatuses[1].Name, "site2");
        }
    }
}
