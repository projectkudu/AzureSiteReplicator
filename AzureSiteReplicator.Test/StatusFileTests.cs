using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using AzureSiteReplicator.Contracts;
using System.Collections.Generic;
using AzureSiteReplicator.Data;
using System.IO.Abstractions.TestingHelpers;
using System.IO;
using AzureSiteReplicator.Models;

namespace AzureSiteReplicator.Test
{
    [TestClass]
    public class StatusFileTests
    {
        private Mock<IEnvironment> _mockEnv = null;

        private class StatusFileTestData : IStatusFile
        {
            private string _name = string.Empty;

            public StatusFileTestData()
            {
                State = DeployState.NotStarted;
                StartTime = DateTime.MinValue;
                EndTime = DateTime.MinValue;
            }

            public DeployState State { get; set; }
            public string Name { get { return _name; } }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public bool Complete { get; set; }
            public int ObjectsAdded { get; set; }
            public int ObjectsUpdated { get; set; }
            public int ObjectsDeleted { get; set; }
            public int ParametersChanged { get; set; }
            public long BytesCopied { get; set; }

            public void Save()
            {
            }
        }

        [TestInitialize]
        public void Setup()
        {
            _mockEnv = new Mock<IEnvironment>();
            _mockEnv.Setup(m => m.SiteReplicatorPath).Returns(@"c:\");
            Environment.Instance = _mockEnv.Object;

            FileHelper.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        }


        [TestMethod]
        public void LoadStatusFileTest()
        {
            StatusFile statusFile = null;
            string profileName = "site";
    
            List<string> tests = new List<string>()
            {
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<status>" +
                "</status>",
                
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<status>" +
                "    <state>failed</state>" +
                "    <startTime>3/17/2014</startTime>" +
                "    <endTime>3/18/2014</endTime>" +
                "    <complete>true</complete>" +
                "    <objectsAdded>100</objectsAdded>" +
                "    <objectsUpdated>1000</objectsUpdated>" +
                "    <objectsDeleted>10000</objectsDeleted>" +
                "    <parametersChanged>100000</parametersChanged>" +
                "    <bytesCopied>1000000</bytesCopied>" +
                "</status>",

                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<status>" +
                "    <state>InvalidState</state>" +
                "    <startTime>InvalidTime</startTime>" +
                "    <endTime>InvalidTime</endTime>" +
                "    <complete>NotBool</complete>" +
                "    <objectsAdded>NotNum</objectsAdded>" +
                "    <objectsUpdated>NotNum</objectsUpdated>" +
                "    <objectsDeleted>NotNum</objectsDeleted>" +
                "    <parametersChanged>NotNum</parametersChanged>" +
                "    <bytesCopied>NotNum</bytesCopied>" +
                "</status>",
            };

            var expected = new[]{
                    new StatusFileTestData(),
                    new StatusFileTestData(){
                        State = DeployState.Failed,
                        StartTime = DateTime.Parse("3/17/2014"),
                        EndTime = DateTime.Parse("3/18/2014"),
                        Complete = true,
                        ObjectsAdded = 100,
                        ObjectsUpdated = 1000,
                        ObjectsDeleted = 10000,
                        ParametersChanged = 100000,
                        BytesCopied = 1000000
                    },
                    new StatusFileTestData()
                };


            for (int i = 0; i < tests.Count; i++)
            {
                FileHelper.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
                {
                    {
                        @"c:\site\status.xml", new MockFileData(tests[i])
                    }
                });

                statusFile = new StatusFile(profileName);
                statusFile.LoadOrCreate();

                VerifyStatusFile(expected[i], statusFile);
            }
        }

        [TestMethod]
        public void CreateStatusFileTest()
        {
            FileHelper.FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            StatusFile statusFile = new StatusFile("site1");
            string statusFilePath = @"c:\site1\status.xml";

            Assert.IsFalse(FileHelper.FileSystem.File.Exists(statusFilePath));
            statusFile.LoadOrCreate();
            Assert.IsTrue(FileHelper.FileSystem.File.Exists(statusFilePath));
        }

        [TestMethod]
        public void SaveStatusFileTest()
        {
            string profileName = "site";
            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now.AddHours(1);

            StatusFileTestData[] tests = new StatusFileTestData[]
            {
                new StatusFileTestData(){
                    State = DeployState.Succeeded,
                    ObjectsAdded = 10,
                    ObjectsUpdated = 20,
                    ObjectsDeleted = 30,
                    ParametersChanged = 40,
                    BytesCopied = 50,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    Complete = true
                }
            };

            System.IO.Abstractions.FileBase MockFile = FileHelper.FileSystem.File;

            for (int i = 0; i < tests.Length; i++)
            {
                StatusFile statusFile = new StatusFile(profileName);
                statusFile.State = tests[i].State;
                statusFile.ObjectsAdded = tests[i].ObjectsAdded;
                statusFile.ObjectsUpdated = tests[i].ObjectsUpdated;
                statusFile.ObjectsDeleted = tests[i].ObjectsDeleted;
                statusFile.ParametersChanged = tests[i].ParametersChanged;
                statusFile.BytesCopied = tests[i].BytesCopied;
                statusFile.StartTime = tests[i].StartTime;
                statusFile.EndTime = tests[i].EndTime;
                statusFile.Complete = tests[i].Complete;

                statusFile.Save();
                
                Assert.IsTrue(MockFile.Exists(@"c:\" + profileName + @"\status.xml"));

                statusFile = new StatusFile(profileName);
                statusFile.LoadOrCreate();

                VerifyStatusFile(tests[i], statusFile);
            }
        }

        private void VerifyStatusFile(StatusFileTestData expected, StatusFile result)
        {
            Assert.AreEqual(expected.BytesCopied, result.BytesCopied, "bytes copied");
            Assert.AreEqual(expected.Complete, result.Complete, "complete");

            VerifyDateTime(expected.EndTime, result.EndTime);
            VerifyDateTime(expected.StartTime, result.StartTime);

            Assert.AreEqual(expected.ObjectsAdded, result.ObjectsAdded, "objects added");
            Assert.AreEqual(expected.ObjectsDeleted, result.ObjectsDeleted, "objects deleted");
            Assert.AreEqual(expected.ObjectsUpdated, result.ObjectsUpdated, "objects updated");
            Assert.AreEqual(expected.ParametersChanged, result.ParametersChanged, "parameters changed");
            Assert.AreEqual(expected.State, result.State, "state");
        }

        private void VerifyDateTime(DateTime expected, DateTime result)
        {
            Assert.AreEqual(expected.Year, result.Year);
            Assert.AreEqual(expected.Month, result.Month);
            Assert.AreEqual(expected.Day, result.Day);
            Assert.AreEqual(expected.Hour, result.Hour);
            Assert.AreEqual(expected.Minute, result.Minute);
            Assert.AreEqual(expected.Second, result.Second);

        }
    }
}
