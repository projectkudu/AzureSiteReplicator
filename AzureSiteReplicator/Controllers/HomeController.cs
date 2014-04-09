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
//using System.Web.Http;
using System.Web.Mvc;

namespace AzureSiteReplicator.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Replicator.Instance.Repository.Reset();
            ReplicationInfoModel model = new ReplicationInfoModel()
            {
                SiteStatuses = Replicator.Instance.Repository.SiteStatuses,
                SkipFiles = Replicator.Instance.Repository.Config.SkipFiles
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
                }
                catch (IOException)
                {
                    // etodo: error handling
                }

                // Trigger a deployment since we just added a new target site
                Replicator.Instance.TriggerDeployment();
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
        public HttpResponseMessage SkipFiles(List<string> skipRules)
        {
            Replicator.Instance.Repository.Config.SetSkips(skipRules);
            Replicator.Instance.Repository.Config.Save();

            Replicator.Instance.TriggerDeployment();

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}