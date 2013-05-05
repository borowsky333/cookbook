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

        public ActionResult LikeBlog(int postID)
        {
            BlogPost_Liker newLiker = new BlogPost_Liker();
            newLiker.BlogPostId = postID;
<<<<<<< HEAD
            newLiker.UserId = WebSecurity.CurrentUserId;
            if (!db.BlogPost_Likers.Contains(newLiker))
            {
                db.BlogPost_Likers.InsertOnSubmit(newLiker);
                var blogPost = (from blogs in db.BlogPosts
                                where blogs.BlogPostId == postID
                                select blogs).FirstOrDefault();
                blogPost.LikeCount++;
                db.SubmitChanges();

                var userID = (from blogs in db.BlogPosts
                              where blogs.BlogPostId == postID
                              select blogs.UserId).FirstOrDefault();

                var likerUserName = (from userprofiles in userDb.UserProfiles
                                     where userprofiles.UserId == newLiker.UserId
                                     select userprofiles.UserName).FirstOrDefault();

                CookbookController.SendSMS(userID, likerUserName + " has liked one of your posts. Come visit Cookbook and check out which post " + likerUserName + " liked!");
                CookbookController.SendEmail(userID, likerUserName + " has liked one of your posts.", likerUserName + " has liked one of your posts. Come visit Cookbook and check out which post " + likerUserName + " liked!");
            }

            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        public ActionResult LikeRecipe(int postID)
        {
            Recipe_Liker newLiker = new Recipe_Liker();
            newLiker.RecipeId = postID;

            newLiker.UserId = WebSecurity.CurrentUserId;
            if (!db.Recipe_Likers.Contains(newLiker))
            {
                db.Recipe_Likers.InsertOnSubmit(newLiker);
                var recipe = (from recipes in db.Recipes
                              where recipes.RecipeID == postID
                              select recipes).FirstOrDefault();
                recipe.LikeCount++;
                db.SubmitChanges();

                var likerUserName = (from userprofiles in userDb.UserProfiles
                                     where userprofiles.UserId == newLiker.UserId
                                     select userprofiles.UserName).FirstOrDefault();

                var userID = (from recipes in db.Recipes
                              where recipes.RecipeID == postID
                              select recipes.UserID).FirstOrDefault();

                CookbookController.SendSMS(userID, likerUserName + " has liked one of your recipes. Come visit Cookbook and check out which recipe " + likerUserName + " liked!");
                CookbookController.SendEmail(userID, likerUserName + " has liked one of your recipes.", likerUserName + " has liked one of your recipes. Come visit Cookbook and check out which recipe " + likerUserName + " liked!");
            }

            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }
    }
}
