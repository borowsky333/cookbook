using Cookbook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Cookbook.Controllers
{
    public class SubscribeController : Controller
    {
        private CookbookDBModelsDataContext db = new CookbookDBModelsDataContext();
        private UsersContext userDb = new UsersContext();

        /// <summary>
        /// Retrieves the list of users the logged in user is subscribed to
        /// </summary>
        /// <returns>A page listing the users subscribed to</returns>
        public ActionResult Index()
        {
            var subscribedUserIds = (from subscribers in db.User_Subscribers
                         where subscribers.UserId == (int)Membership.GetUser().ProviderUserKey
                         select subscribers.SubscriberId).ToList();

            Dictionary<int, string> subscribedUsers = new Dictionary<int, string>();
            foreach (int id in subscribedUserIds)
            {
                subscribedUsers[id] = ((from userprofiles in userDb.UserProfiles
                                        where userprofiles.UserId == id
                                        select userprofiles.UserName).FirstOrDefault());
            }

            //subscribedUsers.OrderBy(

            ViewBag.SubscribedUsers = subscribedUsers;
            return View();
        }

        /// <summary>
        /// Subscribes the logged in user to another user.
        /// </summary>
        /// <param name="userId">The user to subscribe to</param>
        /// <returns>The result page.</returns>
        public ActionResult AddSubscriber(int userId)
        {
            if (userId == (int)Membership.GetUser().ProviderUserKey)
            {
                ViewBag.Message = "You cannot follow yourself.";
                ViewBag.Color = "Red"; //Background color to display...
                return View("Result");
            }


            User_Subscriber us = new User_Subscriber
            {
                UserId = (int)Membership.GetUser().ProviderUserKey,
                SubscriberId = userId
            };

            var currentUserId = (int)Membership.GetUser().ProviderUserKey;

            

            if(db.User_Subscribers.Contains(us))
            {
                ViewBag.Message = "You were already following this user.";
                ViewBag.Color = "Red";
                return View("Result");
            }

            db.User_Subscribers.InsertOnSubmit(us);
            db.SubmitChanges();

            var userName = (from userprofiles in userDb.UserProfiles
                            where userprofiles.UserId == currentUserId
                                 select userprofiles.UserName).FirstOrDefault();

            CookbookController.SendSMS(userId, userName + " has subscribed to your blog. Come greet your new subscriber at The Cookbook!");
            CookbookController.SendEmail(userId, userName + " has subscribed to your blog.", userName + " has subscribed to your blog. Come greet your new subscriber at The Cookbook!");
            

            ViewBag.Message = "Successfully Subscribed!";
            ViewBag.Color = "Green";

            return View("Result");
        }

        /// <summary>
        /// Removes a user from logged in user's subscriptions.
        /// </summary>
        /// <param name="userId">The subscription to remove</param>
        /// <returns>The result page</returns>
        public ActionResult RemoveSubscriber(int userId)
        {
            User_Subscriber us = new User_Subscriber
            {
                UserId = (int)Membership.GetUser().ProviderUserKey,
                SubscriberId = userId
            };

            if (!db.User_Subscribers.Contains(us))
            {
                ViewBag.Message = "You weren't following this user.";
                ViewBag.Color = "Red";
                return View("Result");
            }

            db.ExecuteQuery<Object>("DELETE FROM User_Subscriber " +
                                    "WHERE UserId=" + us.UserId +
                                    " AND SubscriberId=" + userId);

            
            db.SubmitChanges();

            ViewBag.Message = "Successfully Unsubscribed!";
            ViewBag.Color = "Green";
            return View("Result");

        }
    }
}
