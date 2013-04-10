using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cookbook.Controllers
{
    public class DiscoveryFeedController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult FilterByTag(string tag)
        {
            return View();
        }

    }
}
