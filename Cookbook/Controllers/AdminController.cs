using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cookbook.Controllers
{
    public class AdminController : Controller
    {
        
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ViewReports()
        {
            return View();
        }

        public ActionResult DeleteUser(int userId)
        {
            return View();
        }

        public ActionResult DeleteItem(int id)
        {
            return View();
        }

        public ActionResult SendEmailToUserBase(string subject, string message)
        {
            return View();
        }
       
    }
}
