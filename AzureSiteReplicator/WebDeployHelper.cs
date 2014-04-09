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
            {
                try
                {
                    status.State = Models.DeployState.Deploying;
                    status.Save();

                    // Publish the content to the remote site
                    using (var deploymentObject = DeploymentManager.CreateObject(DeploymentWellKnownProvider.ContentPath, contentPath, sourceBaseOptions))
                    {
                        // Note: would be nice to have an async flavor of this API...
                        summary = deploymentObject.SyncTo(DeploymentWellKnownProvider.ContentPath, siteName, destBaseOptions, new DeploymentSyncOptions());
                    }
                }
                catch
                {
                    success = false;
                    status.State = Models.DeployState.Failed;
                }

                if (success)
                {
                    status.State = Models.DeployState.Succeeded;
                }
            }

            return summary;
        }

        private static void AddSkips(
            IConfigRepository repository,
            DeploymentBaseOptions sourceBaseOptions,
            DeploymentBaseOptions destBaseOptions)
        {
            IEnumerable<string> skipFiles = repository.Config.SkipFiles;
            foreach (string skip in skipFiles)
            {
                sourceBaseOptions.SkipFile(skip);
            }

            foreach (string skip in skipFiles)
            {
                destBaseOptions.SkipFile(skip);
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

            return publishSettings.SiteName;
        }

        private static bool AllowCertificateCallback(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
    }
}
