using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Web.Deployment;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using AzureSiteReplicator.Data;
using AzureSiteReplicator.Contracts;
using System.Collections.Generic;

namespace AzureSiteReplicator
{
    public class WebDeployHelper
    {
        private List<string> _skipRulesTestResults;
        private Object _lockObj = new Object();

        public DeploymentChangeSummary DeployContentToOneSite(
            IConfigRepository repository,
            string contentPath,
            string publishSettingsFile)
        {
            var sourceBaseOptions = new DeploymentBaseOptions();
            DeploymentBaseOptions destBaseOptions;
            string siteName = SetDestBaseOptions(publishSettingsFile, out destBaseOptions);
            bool success = true;
            DeploymentChangeSummary summary = null;

            AddSkips(repository.Config.SkipRules, sourceBaseOptions, destBaseOptions);

            Trace.TraceInformation("Starting WebDeploy for {0}", Path.GetFileName(publishSettingsFile));

            using (StatusFile status = new StatusFile(siteName))
            using (LogFile logFile = new LogFile(siteName, false))
            {
                sourceBaseOptions.Trace += logFile.LogEventHandler;
                destBaseOptions.Trace += logFile.LogEventHandler;

                try
                {
                    logFile.Log(TraceLevel.Info, "Beginning sync");

                    status.State = Models.DeployState.Deploying;
                    status.Save();

                    // Publish the content to the remote site
                    using (var deploymentObject = DeploymentManager.CreateObject(DeploymentWellKnownProvider.ContentPath, contentPath, sourceBaseOptions))
                    {
                        // Note: would be nice to have an async flavor of this API...
                        summary = deploymentObject.SyncTo(DeploymentWellKnownProvider.ContentPath, siteName, destBaseOptions, new DeploymentSyncOptions());
                    }

                    string summaryString = string.Format("Total Changes: {0} ({1} added, {2} deleted, {3} updated, {4} parameters changed, {5} bytes copied)",
                        summary.TotalChanges,
                        summary.ObjectsAdded,
                        summary.ObjectsDeleted,
                        summary.ObjectsUpdated,
                        summary.ParameterChanges,
                        summary.BytesCopied);

                    status.ObjectsAdded = summary.ObjectsAdded;
                    status.ObjectsDeleted = summary.ObjectsDeleted;
                    status.ObjectsUpdated = summary.ObjectsUpdated;
                    status.ParametersChanged = summary.ParameterChanges;
                    status.BytesCopied = summary.BytesCopied;

                    logFile.Log(TraceLevel.Info, summaryString);
                    logFile.Log(TraceLevel.Info, "Sync completed successfully");
                }
                catch(Exception e)
                {
                    logFile.Log(TraceLevel.Error, e.ToString());
                    success = false;
                    status.State = Models.DeployState.Failed;
                }

                if (success)
                {
                    status.State = Models.DeployState.Succeeded;
                }
            }   // Close log file and status file

            return summary;
        }

        public List<string> TestSkipRule(List<SkipRule> skipRules, string contentPath)
        {
            DeploymentBaseOptions sourceOptions = new DeploymentBaseOptions();
            DeploymentBaseOptions destOptions = new DeploymentBaseOptions();
            DeploymentSyncOptions syncOptions = new DeploymentSyncOptions();

            _skipRulesTestResults = new List<string>();

            AddSkips(skipRules.AsReadOnly(), sourceOptions, destOptions);
            syncOptions.WhatIf = true;

            // Puropsely only setting event handler for source so that we don't get duplicate
            // events being fired
            sourceOptions.TraceLevel = TraceLevel.Verbose;
            sourceOptions.Trace += AddSkipRuleResultEventHandler;

            using (DeploymentObject sourceObject = DeploymentManager.CreateObject(DeploymentWellKnownProvider.ContentPath, contentPath, sourceOptions))
            {
                sourceObject.SyncTo(destOptions, syncOptions);
            }

            _skipRulesTestResults.Sort();
            return _skipRulesTestResults;
        }

        private void AddSkipRuleResultEventHandler(object sender, DeploymentTraceEventArgs traceEvent)
        {
            DeploymentSkipDirectiveEventArgs skipArgs = traceEvent as DeploymentSkipDirectiveEventArgs;

            if (skipArgs != null)
            {
                string path = skipArgs.AbsolutePath;
                bool isDirectory = FileHelper.FileSystem.Directory.Exists(path);
                string basePath = Environment.Instance.ContentPath;
                if (path != null && path.Length > basePath.Length)
                {
                    // Removing the base filesystem content path so that people
                    // don't try to build expressions based off of that path in case
                    // it changes later.
                    path = path.Remove(0, basePath.Length);
                }

                if (isDirectory)
                {
                    path = "Directory: " + path;
                }
                else
                {
                    path = "File: " + path;
                }

                _skipRulesTestResults.Add(path);
            }
        }

        private static void AddSkips(
            IReadOnlyCollection<SkipRule> skips,
            DeploymentBaseOptions sourceBaseOptions,
            DeploymentBaseOptions destBaseOptions)
        {
            foreach (SkipRule skip in skips)
            {
                if (skip.IsDirectory)
                {
                    sourceBaseOptions.SkipDirectory(skip.Expression);
                    destBaseOptions.SkipDirectory(skip.Expression);
                }
                else
                {
                    sourceBaseOptions.SkipFile(skip.Expression);
                    destBaseOptions.SkipFile(skip.Expression);
                }
            }
        }

        private string SetDestBaseOptions(
            string publishSettingsPath,
            out DeploymentBaseOptions deploymentBaseOptions)
        {
            AzureSiteReplicator.Data.PublishSettings publishSettings = new AzureSiteReplicator.Data.PublishSettings(publishSettingsPath);
            deploymentBaseOptions = new DeploymentBaseOptions
            {
                ComputerName = publishSettings.ComputerName,
                UserName = publishSettings.Username,
                Password = publishSettings.Password,
                AuthenticationType = publishSettings.AuthenticationType
            };

            if (publishSettings.AllowUntrusted)
            {
                ServicePointManager.ServerCertificateValidationCallback = AllowCertificateCallback;
            }

            deploymentBaseOptions.TraceLevel = TraceLevel.Verbose;

            return publishSettings.SiteName;
        }

        private static bool AllowCertificateCallback(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
    }
}
