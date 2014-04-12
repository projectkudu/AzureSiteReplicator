using AzureSiteReplicator.Contracts;
using AzureSiteReplicator.Data;
using AzureSiteReplicator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace AzureSiteReplicator.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            List<SiteStatusModel> statuses = new List<SiteStatusModel>();
            foreach (var site in Replicator.Instance.Repository.Sites)
            {
                statuses.Add(new SiteStatusModel(site.Status));
            }

            ReplicationInfoModel model = new ReplicationInfoModel()
            {
                SiteStatuses = statuses,
                SkipFiles = Replicator.Instance.Repository.Config.SkipRules
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            if (file.ContentLength > 0)
            {
                try
                {
                    PublishSettings settings = new PublishSettings(file.InputStream);
                    string fileName = settings.SiteName + ".publishSettings";
                    var path = Path.Combine(Environment.Instance.SiteReplicatorPath, fileName);
                    
                    file.SaveAs(path);

                    Replicator.Instance.Repository.AddSite(path);

                    // Trigger a deployment since we just added a new target site
                    Replicator.Instance.TriggerDeployment();
                }
                catch (IOException)
                {
                    // todo: error handling
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public HttpResponseMessage SkipRules(IList<SkipRule> skipRules)
        {
            if (skipRules == null)
            {
                Replicator.Instance.Repository.Config.ClearSkips();
            }
            else
            {
                Replicator.Instance.Repository.Config.SetSkips(skipRules.ToList());
            }

            Replicator.Instance.Repository.Config.Save();
            Replicator.Instance.TriggerDeployment();

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpDelete]
        public HttpResponseMessage Site(string name)
        {
            Replicator.Instance.Repository.RemoveSite(name);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        public FileResult LogFile(string siteName)
        {
            using (LogFile logFile = new LogFile(siteName, true))
            {
                byte[] fileBytes = FileHelper.FileSystem.File.ReadAllBytes(logFile.FilePath);
                return File(
                    fileBytes,
                    System.Net.Mime.MediaTypeNames.Application.Octet,
                    "deploy.log");
            }
        }
    }
}