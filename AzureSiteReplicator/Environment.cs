using AzureSiteReplicator.Contracts;
using AzureSiteReplicator.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace AzureSiteReplicator
{
    public class Environment : IEnvironment
    {
        private static IEnvironment s_instance = null;
        private static Object lockObj = new Object();

        public static IEnvironment Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (lockObj)
                    {
                        if (s_instance == null)
                        {
                            s_instance = new Environment();
                        }
                    }
                }

                return s_instance;
            }

            set
            {
                lock (lockObj)
                {
                    s_instance = value;
                }
            }
        }

        public Environment()
        {
            string homePath = System.Environment.ExpandEnvironmentVariables(@"%SystemDrive%\home");

            if (Directory.Exists(homePath))
            {
                // Running on Azure

                // Publish the wwwroot folder
                ContentPath = Path.Combine(homePath, "site", "wwwroot");

                SiteReplicatorPath = Path.Combine(homePath, "data", "SiteReplicator");
            }
            else
            {
                // Local case: run from App_Data for testing purpose

                string appData = HostingEnvironment.MapPath("~/App_Data");

                ContentPath = Path.Combine(appData, "source");

                SiteReplicatorPath = Path.Combine(appData, "SiteReplicator");
            }

            Trace.TraceInformation("ContentPath={0}", ContentPath);
            Directory.CreateDirectory(ContentPath);
            Trace.TraceInformation("SiteReplicator={0}", SiteReplicatorPath);
            Directory.CreateDirectory(SiteReplicatorPath);
        }

        // Path to the web content we want to replicate
        public string ContentPath { get; set; }

        // Path to the publish settings files that drive where we publish to
        public string SiteReplicatorPath { get; set; }
    }
}