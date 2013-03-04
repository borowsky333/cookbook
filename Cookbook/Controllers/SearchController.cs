using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cookbook.Controllers
{
    public class SearchController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Search(string tag = null, string users = null, string ingredients = null, string title = null)
        {
            return View();
        }

    }
}
