using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cookbook.Controllers
{
    public class WelcomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "NewsFeed"); //If logged in, do not show welcome page.
            }
            else
            {
                return View(); //If not logged in, show welcome page.
            }
        }

        public ActionResult FilterByTag(string tag)
        {
            return View();
        }



    }
}
