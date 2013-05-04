using Cookbook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Cookbook.Controllers
{
    public class SubcribeController : Controller
    {
        private CookbookDBModelsDataContext db = new CookbookDBModelsDataContext();

        public ActionResult Index()
        {
            ViewBag.SubscribedUsers = (from subscribers in db.User_Subscribers
                         where subscribers.UserId == (int)Membership.GetUser().ProviderUserKey
                         select subscribers.SubscriberId).ToList();
            
            return View();
        }

        
        [HttpPost]
        public ActionResult AddSubcriber(int userId)
        {
            User_Subscriber us = new User_Subscriber
            {
                UserId = (int)Membership.GetUser().ProviderUserKey,
                SubscriberId = userId
            };

            

            if(db.User_Subscribers.Contains(us))
            {
                ViewBag.Message = "You were already following this user";
                return View();
            }

            db.User_Subscribers.InsertOnSubmit(us);
            db.SubmitChanges();


            return View();
        }

        [HttpPost]
        public ActionResult RemoveSubcriber(int userId)
        {
            return View();
        }



    }
}
