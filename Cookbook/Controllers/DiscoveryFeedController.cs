using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cookbook.Models;

namespace Cookbook.Controllers
{
    public class DiscoveryFeedController : Controller
    {
        private CookbookDBModelsDataContext db = new CookbookDBModelsDataContext();

        public ActionResult Index()
        {
            var recipeList =
                (from recipes in db.Recipes
                 //where recipes.DateModified.Date == DateTime.Today
                 select recipes)
                 .Take(50)
                 .ToList();
            var recipeDict = new Dictionary<Recipe, List<string>>();

            foreach (var recipe in recipeList)
            {
                var ingredients =
                    (from allIngredients in db.Ingredients
                     where allIngredients.RecipeId == recipe.RecipeID
                     select allIngredients.Name).ToList();
                recipeDict.Add(recipe, ingredients);
            }

            var postList =
                (from posts in db.BlogPosts
                 select posts)
                 .Take(50)
                 .ToList();

            ViewBag.Recipes = recipeDict;
            ViewBag.Posts = postList;

            return View();
        }


        public ActionResult FilterByTag(string tag)
        {
            return View();
        }

    }
}
