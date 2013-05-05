using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cookbook.Models;
using System.Web.Security;
using WebMatrix.WebData;

namespace Cookbook.Controllers
{
    public class NewsFeedController : Controller
    {
        private CookbookDBModelsDataContext db = new CookbookDBModelsDataContext();
        private UsersContext userDb = new UsersContext();

        public ActionResult Index(Nullable<int> page)
        {
            if (page == null || page < 1)
                page = 1;

            List<ViewPostModel> sortedPosts = CookbookController.GetCombinedPosts(GetRecipes(),GetBlogPosts(),(int)page,ViewBag);

            return View(sortedPosts);
        }
        


        public List<Recipe> GetRecipes()
        {
            var subscribedUserIds = (from subscribers in db.User_Subscribers
                                     where subscribers.UserId == (int)Membership.GetUser().ProviderUserKey
                                     select subscribers.SubscriberId).ToList();

            List<Recipe> recipes = new List<Recipe>();
            foreach (int id in subscribedUserIds)
            {
                recipes.AddRange((from allRecipes in db.Recipes
                                  where allRecipes.UserID == id
                                  select allRecipes).ToList());
            }

            return recipes;
        }

        public List<BlogPost> GetBlogPosts()
        {
            var subscribedUserIds = (from subscribers in db.User_Subscribers
                                     where subscribers.UserId == (int)Membership.GetUser().ProviderUserKey
                                     select subscribers.SubscriberId).ToList();

            List<BlogPost> posts = new List<BlogPost>();
            foreach (int id in subscribedUserIds)
            {
                posts.AddRange((from allPosts in db.BlogPosts
                                where allPosts.UserId == id
                                select allPosts).ToList());
            }

            return posts;
        }


        public ActionResult FilterByTag(string tag)
        {
            return View();
        }

        public ActionResult LikeBlog(int postID)
        {
            var blogPost = (from blogs in db.BlogPosts
                            where blogs.BlogPostId == postID
                            select blogs).FirstOrDefault();
            blogPost.LikeCount++;

            BlogPost_Liker newLiker = new BlogPost_Liker();
            newLiker.BlogPostId = postID;
            newLiker.UserId = WebSecurity.CurrentUserId;
            if (!db.BlogPost_Likers.Contains(newLiker))
                db.BlogPost_Likers.InsertOnSubmit(newLiker);

            db.SubmitChanges();

            var userID = (from blogs in db.BlogPosts
                          where blogs.BlogPostId == postID
                          select blogs.UserId).FirstOrDefault();

            var likerUserName = (from userprofiles in userDb.UserProfiles
                                 where userprofiles.UserId == newLiker.UserId
                                 select userprofiles.UserName).FirstOrDefault();

            CookbookController.SendSMS(userID, likerUserName + " has liked one of your posts. Come visit Cookbook and check out which post " + likerUserName + " liked!");
            CookbookController.SendEmail(userID, likerUserName + " has liked one of your posts.", likerUserName + " has liked one of your posts. Come visit Cookbook and check out which post " + likerUserName + " liked!");


            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        public ActionResult LikeRecipe(int postID)
        {
            var recipe = (from recipes in db.Recipes
                          where recipes.RecipeID == postID
                          select recipes).FirstOrDefault();
            recipe.LikeCount++;

            Recipe_Liker newLiker = new Recipe_Liker();
            newLiker.RecipeId = postID;
            newLiker.UserId = WebSecurity.CurrentUserId;
            if (!db.Recipe_Likers.Contains(newLiker))
                db.Recipe_Likers.InsertOnSubmit(newLiker);

            db.SubmitChanges();

            var likerUserName = (from userprofiles in userDb.UserProfiles
                                 where userprofiles.UserId == newLiker.UserId
                                 select userprofiles.UserName).FirstOrDefault();

            var userID = (from recipes in db.Recipes
                          where recipes.RecipeID == postID
                          select recipes.UserID).FirstOrDefault();

            CookbookController.SendSMS(userID, likerUserName + " has liked one of your recipes. Come visit Cookbook and check out which recipe " + likerUserName + " liked!");
            CookbookController.SendEmail(userID, likerUserName + " has liked one of your recipes.", likerUserName + " has liked one of your recipes. Come visit Cookbook and check out which recipe " + likerUserName + " liked!");

            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }
    }
}
