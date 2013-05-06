using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cookbook.Models;
using WebMatrix.WebData;
using Amazon.SimpleEmail.Model;
using Amazon.SimpleEmail;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace Cookbook.Controllers
{
    [Authorize]
    public class DiscoveryFeedController : Controller
    {
        private CookbookDBModelsDataContext db = new CookbookDBModelsDataContext();
        private UsersContext userDb = new UsersContext();

        /// <summary>
        /// Displays recipes/blog posts from the past 5 days.
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <returns>The discovery feed</returns>
        public ActionResult Index(Nullable<int> page)
        {
            var recipeList =
                (from recipes in db.Recipes
                 where recipes.DateCreated > DateTime.Now.AddDays(-5)
                 select recipes)
                 .ToList();

            var blogPostList =
                (from posts in db.BlogPosts
                 where posts.DateCreated > DateTime.Now.AddDays(-5)
                 select posts)
                .ToList();

            if (page == null || page < 1)
                page = 1;
            List<ViewPostModel> sortedPosts = CookbookController.GetCombinedPosts(recipeList, blogPostList, (int)page, ViewBag);



            return View(sortedPosts);
        }


        public ActionResult FilterByTag(string tag)
        {
            return View();
        }

    }
}
