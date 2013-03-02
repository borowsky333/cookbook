using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cookbook.Models;
namespace Cookbook.Controllers
{
    public class CookbookController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        //upload own recipe
        public ActionResult UploadRecipe(Recipe recipe)
        {
            return View();
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

        //upload a blog post
        public ActionResult UploadPost(BlogPost post)
        {
            return View();
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
        public ActionResult UploadImage(string postId, ImagePost image)
        {
            return View();
        }

        public ActionResult EditImage(ImagePost image)
        {
            return View();
        }

        public ActionResult DeleteImage(ImagePost image)
        {
            return View();
        }

        //-----------------
        //SOCIAL
        //-----------------

        //add another user's recipe to your cookbook
        public ActionResult AddRecipe(Recipe recipe)
        {
            return View();
        }

        //share another user's post onto your cookbook
        public ActionResult SharePost(BlogPost post)
        {
            return View();
        }

        public ActionResult AddComment(BlogPost post, Comment comment)
        {
            return View();
        }

        public ActionResult AddComment(Recipe recipe, Comment comment)
        {
            return View();
        }

        public ActionResult DeleteComment(Comment comment)
        {
            return View();
        }
    }
}
