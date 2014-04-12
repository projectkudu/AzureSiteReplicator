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

            AddSkips(repository, sourceBaseOptions, destBaseOptions);

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


        private static void AddSkips(
            IConfigRepository repository,
            DeploymentBaseOptions sourceBaseOptions,
            DeploymentBaseOptions destBaseOptions)
        {
            IReadOnlyCollection<SkipRule> skipFiles = repository.Config.SkipRules;
            foreach (SkipRule skip in skipFiles)
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
            PublishSettings publishSettings = new PublishSettings(publishSettingsPath);
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
