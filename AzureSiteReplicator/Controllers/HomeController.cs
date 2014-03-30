using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using Microsoft.Web.Deployment;

namespace AzureSiteReplicator.Controllers
{

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewData["publishSettingsFiles"] = Directory.GetFiles(Environment.Instance.PublishSettingsPath).Select(path => Path.GetFileName(path).Split('.').First());
            //figure out where you are 
            var uri = new Uri(Request.Url.ToString());
            IPHostEntry host = Dns.GetHostEntry(uri.Host);
            ViewData["masterRegion"] = getRegionNameFromHost(host.HostName);

            //figure out where the files are
            string regionCodes = "";
            string[] pubFiles = Directory.GetFiles(Environment.Instance.PublishSettingsPath);
            foreach (string pubFile in pubFiles)
            {
                PublishSettings publishSettings = new PublishSettings(pubFile);
                regionCodes += getRegionNameFromHost(publishSettings.PublishUrlRaw)+",";
            }
            ViewData["regionCodes"] = regionCodes.TrimEnd(',');

            return View();
        }

        private string getRegionNameFromHost(string host){

            string subDomain = host.Split('.')[0];
            //waws-prod-bay-001
            string[] urlArr = subDomain.Split('-');
            string masterRegion = "HK1";
            if (urlArr.Length == 4)
                masterRegion = urlArr[2].ToUpper();
            return masterRegion;
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            if (file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Environment.Instance.PublishSettingsPath, fileName);
                file.SaveAs(path);

                // Trigger a deployment since we just added a new target site
                //Replicator.Instance.TriggerDeployment();
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
    }
}