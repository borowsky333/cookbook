﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cookbook.Models;
using System.Web.Security;
using WebMatrix.WebData;

namespace Cookbook.Controllers
{
    [Authorize]
    public class NewsFeedController : Controller
    {
        private CookbookDBModelsDataContext db = new CookbookDBModelsDataContext();
        private UsersContext userDb = new UsersContext();

        /// <summary>
        /// Displays the recipes/blog posts from user's the logged in user is subscribed to.
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <returns>The news feed.</returns>
        public ActionResult Index(Nullable<int> page)
        {
            if (page == null || page < 1)
                page = 1;

            List<ViewPostModel> sortedPosts = CookbookController.GetCombinedPosts(GetRecipes(),GetBlogPosts(),(int)page,ViewBag);

            return View(sortedPosts);
        }
        

        /// <summary>
        /// Retrieves recipes from the user's subscribed list.
        /// </summary>
        /// <returns>List of recipes from the user's subscribed list.</returns>
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

            //add current user's recipes
            recipes.AddRange((from allRecipes in db.Recipes
                              where allRecipes.UserID == (int)Membership.GetUser().ProviderUserKey
                              select allRecipes).ToList());

            return recipes;
        }

        /// <summary>
        /// Retrieves blogposts from the user's subscribed list.
        /// </summary>
        /// <returns>List of blogposts from the user's subscribed list.</returns>
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

            //add current user's posts
            posts.AddRange((from allPosts in db.BlogPosts
                            where allPosts.UserId == (int)Membership.GetUser().ProviderUserKey
                            select allPosts).ToList());
            return posts;
        }


        public ActionResult FilterByTag(string tag)
        {
            return View();
        }

    }
}
