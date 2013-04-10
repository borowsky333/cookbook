using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cookbook.Controllers
{
    public class SubscribersController : Controller
    {

        //View who is subscribed to you and who you are subscribed to.
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Subscribe(string username)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Unsubscribe(string username)
        {
            return View();
        }

    }
}
