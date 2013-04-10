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
                 where recipes.DateModified.Date == DateTime.Today
                 select recipes)
                 .Take(20)
                 .ToList();

            ViewBag.Recipes = recipeList;

            return View();
        }


        public ActionResult FilterByTag(string tag)
        {
            return View();
        }

    }
}
