﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Cookbook.Models;
namespace Cookbook.Controllers
{
    [Authorize]
    public class CookbookController : Controller
    {
        CookbookDBModelsDataContext db = new CookbookDBModelsDataContext();


        //redirect if not logged in
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult UploadRecipe()
        {
            return View();
        }


        [HttpPost]
        public ActionResult UploadRecipe(UploadRecipeModel newRecipe)
        {
            Recipe recipeEntry = new Recipe();
            recipeEntry.Title = newRecipe.Title;
            recipeEntry.Instructions = newRecipe.Instructions;
            
            //TODO: need logic for adding tags to tag table.
            recipeEntry.DateCreated = DateTime.Now;

            recipeEntry.DateModified = DateTime.Now;
            recipeEntry.UserID = (int)Membership.GetUser().ProviderUserKey;

            db.Recipes.InsertOnSubmit(recipeEntry);
            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        //edit own recipe
        public ActionResult EditRecipe(Recipe recipe)
        {
            return View();
        }

        public ActionResult DeleteRecipe(Recipe recipe)
        {
            return View();
        }


        public ActionResult UploadPost()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadPost(BlogPost newPost)
        {
            BlogPost post = new BlogPost();
            post.Title = newPost.Title;
            post.Post = newPost.Post;

            //TODO: need logic for adding tags to tag table.
            post.DateCreated = DateTime.Now;

            post.DateModified = DateTime.Now;
            post.UserId = (int)Membership.GetUser().ProviderUserKey;

            db.BlogPosts.InsertOnSubmit(post);
            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        public ActionResult EditPost(BlogPost post)
        {
            return View();
        }

        public ActionResult DeletePost(BlogPost post)
        {
            return View();
        }

        //postId must be the ID of either a blog post or recipe post
        public ActionResult UploadImage(string postId, Image image)
        {
            return View();
        }

        public ActionResult EditImage(string postId, Image image)
        {
            return View();
        }

        public ActionResult DeleteImage(string postId, Image image)
        {
            return View();
        }

        //-----------------
        //SOCIAL
        //-----------------

        //add another user's recipe to your cookbook
        public ActionResult FavoriteRecipe(Recipe recipe)
        {
            return View();
        }

        public ActionResult AddComment(int postID, int commentID)
        {
            return View();
        }

        public ActionResult DeleteComment(int postID, int commentID)
        {
            return View();
        }
        
        public ActionResult LikePost(int postID)
        {
            return View();
        }

        public ActionResult Report(int id)
        {
            return View();
        }
    }
}
