using System.Web;
using System.Web.Mvc;
using AzureSiteReplicator.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureSiteReplicator.Test
{
    [TestClass]
    public class HomeControllerTests
    {
        [TestMethod]
        public void ShouldNotAllowAddingNullPublishSettings()
        {
            var homeController = new HomeController();

            HttpPostedFileBase file = null;

            ActionResult result = homeController.Index(file);

            Assert.IsNotNull(result);
        }
    }
}
