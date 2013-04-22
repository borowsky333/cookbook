using System;
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
        private CookbookDBModelsDataContext db = new CookbookDBModelsDataContext();


        //redirect if not logged in
        public ActionResult Index()
        {
            var recipes = GetRecipes((int)Membership.GetUser().ProviderUserKey);
            var recipeDict = new Dictionary<Recipe, List<string>>();
            
            foreach (var recipe in recipes)
            {
                var ingredients =
                    (from allIngredients in db.Ingredients
                     where allIngredients.RecipeId == recipe.RecipeID
                     select allIngredients.Name).ToList();
                recipeDict.Add(recipe, ingredients);
            }

            ViewBag.MyRecipes = recipeDict;
            ViewBag.MyPosts = GetPosts((int)Membership.GetUser().ProviderUserKey);

            return View();
        }

        public ActionResult ViewCookbook(int userId)
        {
            var recipes = GetRecipes(userId);
            var recipeDict = new Dictionary<Recipe, List<string>>();

            foreach (var recipe in recipes)
            {
                var ingredients =
                    (from allIngredients in db.Ingredients
                     where allIngredients.RecipeId == recipe.RecipeID
                     select allIngredients.Name).ToList();
                recipeDict.Add(recipe, ingredients);
            }

            ViewBag.MyRecipes = recipeDict;
            ViewBag.MyPosts = GetPosts(userId);

            return View();
        }

        public List<Recipe> GetRecipes(int userId)
        {
            var recipes = (from allRecipes in db.Recipes
                           where allRecipes.UserID == userId
                          select allRecipes).Take(20).ToList();
            return recipes;
        }

        public List<BlogPost> GetPosts(int userId)
        {
            var posts = (from allPosts in db.BlogPosts
                         where allPosts.UserId == userId
                         select allPosts).Take(20).ToList();
            return posts;
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
            
            recipeEntry.DateCreated = DateTime.Now;

            recipeEntry.DateModified = DateTime.Now;
            recipeEntry.UserID = (int)Membership.GetUser().ProviderUserKey;

            db.Recipes.InsertOnSubmit(recipeEntry);

            db.SubmitChanges();

            var ingredients = newRecipe.Ingredients.Split(',').ToList();
            foreach (var ingredient in ingredients)
            {
                Ingredient entry = new Ingredient();
                entry.Name = ingredient.Trim();
                entry.RecipeId = recipeEntry.RecipeID;
                db.Ingredients.InsertOnSubmit(entry);
            }

            var tags = newRecipe.Tags.Split(',').ToList();
            foreach (var tag in tags)
            {
                Recipe_Tag newTag = new Recipe_Tag();
                newTag.RecipeID = recipeEntry.RecipeID;
                newTag.Tag = tag.Trim();
                db.Recipe_Tags.InsertOnSubmit(newTag);
            }
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
        public ActionResult UploadPost(UploadBlogPostModel newPost)
        {
            BlogPost post = new BlogPost();
            post.Title = newPost.Title;
            post.Post = newPost.Post;

            post.DateCreated = DateTime.Now;

            post.DateModified = DateTime.Now;
            post.UserId = (int)Membership.GetUser().ProviderUserKey;

            db.BlogPosts.InsertOnSubmit(post);
            db.SubmitChanges();


            var tags = newPost.Tags.Split(',').ToList();
            foreach (var tag in tags)
            {
                BlogPost_Tag newTag = new BlogPost_Tag();
                newTag.BlogPostId = post.BlogPostId;
                newTag.Tag = tag.Trim();
                db.BlogPost_Tags.InsertOnSubmit(newTag);
            }
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
