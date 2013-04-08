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
        //CookbookDBModelsDataContext db = new CookbookDBModelsDataContext();


        //redirect if not logged in
        public ActionResult Index()
        {

            //var myRecipes =
            //    (from recipes in db.Recipes
            //     where recipes.RecipeID == 1
            //     select recipes).Take(20);
            

            //Recipe newRecipe = new Recipe();

            //newRecipe.Title = "My Recipe";
            //newRecipe.Instructions = "INSTRUCTION!";

            //db.Recipes.InsertOnSubmit(newRecipe);
            //db.SubmitChanges();

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
