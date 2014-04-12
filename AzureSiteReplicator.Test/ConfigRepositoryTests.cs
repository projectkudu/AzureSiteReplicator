using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using AzureSiteReplicator.Contracts;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;
using AzureSiteReplicator.Data;
using AzureSiteReplicator.Models;
using System.IO.Abstractions;

namespace AzureSiteReplicator.Test
{
    [TestClass]
    public class ConfigRepositoryTests
    {
        private Mock<IEnvironment> _mockEnv = null;
        private const string ProfileTemplate =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
        "<publishData>" +
        "  <publishProfile " +
        "      publishMethod=\"MSDeploy\"" +
        "      msdeploySite=\"{0}\">" +
        "  </publishProfile>" +
        "</publishData>";


        [TestInitialize]
        public void Setup()
        {
            _mockEnv = new Mock<IEnvironment>();
            _mockEnv.Setup(m => m.SiteReplicatorPath).Returns(@"c:\");
            Environment.Instance = _mockEnv.Object;

            FileHelper.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        }

        [TestMethod]
        public void ConfigRepositoryGetSitesTest()
        {
            FileHelper.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                { 
                    @"c:\site1.publishsettings",
                    new MockFileData(string.Format(ProfileTemplate, "site1"))
                },
                { 
                    @"c:\site2.foo.publishsettings",
                    new MockFileData(string.Format(ProfileTemplate, "site2"))
                },
                { @"c:\foo.txt", new MockFileData("bar")}
            });

            HashSet<Site> expected = new HashSet<Site> 
            { 
                new Site(@"c:\site1.publishsettings"),
                new Site(@"c:\site2.foo.publishsettings") 
            };

            IConfigRepository repository = new ConfigRepository();
            TestHelpers.VerifyEnumerable(expected, repository.Sites);
        }

        [TestMethod]
        public void ConfigRepositoryRemoveSiteTest()
        {
            FileHelper.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                { 
                    @"c:\site1.publishsettings",
                    new MockFileData(string.Format(ProfileTemplate, "site1"))
                },
                { 
                    @"c:\site2.foo.publishsettings",
                    new MockFileData(string.Format(ProfileTemplate, "site2"))
                },
                { @"c:\foo.txt", new MockFileData("bar")},
                { @"c:\site1\", new MockDirectoryData() }
            });

            IConfigRepository repository = new ConfigRepository();
            repository.RemoveSite("site1");
            repository.RemoveSite("site2");

            FileBase fileBase = FileHelper.FileSystem.File;
            DirectoryBase dirBase = FileHelper.FileSystem.Directory;

            Assert.IsFalse(fileBase.Exists(@"c:\site1.publishSettings"), "site1 publishSettings still exists");
            Assert.IsFalse(dirBase.Exists(@"c:\site1"), "site1 folder still exists");
            Assert.IsFalse(fileBase.Exists(@"c:\site2.foo.publishsettings"), "sit2 publishSettings still exists");
        }
    }
}
