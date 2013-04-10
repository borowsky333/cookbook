using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cookbook.Models;

namespace Cookbook.Controllers
{
    public class NewsFeedController : Controller
    {
        public ActionResult Index()
        {

            return View();
        }

        public List<BlogPost> GetPostsBySubscribed()
        {
            return null;
        }

        public List<Recipe> GetRecipesBySubscribed()
        {
            return null;
        }


        public ActionResult FilterByTag(string tag)
        {
            return View();
        }



    }
}
