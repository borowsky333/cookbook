using Cookbook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cookbook.Controllers
{
    public class SearchController : Controller
    {
        private CookbookDBModelsDataContext db = new CookbookDBModelsDataContext();
        private UsersContext userDb = new UsersContext();

        public ActionResult SearchUsers(string userName)
        {
            List<UserProfile> result = (from allUsers in userDb.UserProfiles
                                        where allUsers.UserName.Contains(userName)
                                        select allUsers).ToList();

            UserProfile exactMatch = result.Find(delegate(UserProfile up)
                                    {
                                        return string.Equals(up.UserName,
                                                             userName,
                                                             StringComparison.OrdinalIgnoreCase);
                                    });

            if (exactMatch != null)//if there is an exact match, move it to the top of the list
            {
                result.Remove(exactMatch);
                result.Insert(0, exactMatch);
            }

            ViewBag.UserResult = result;
            return View();
        }

        public ActionResult SearchTags(string tag)
        {
            List<Recipe> results = (from recipes in db.Recipes
                                    where recipes.Recipe_Tags.Contains(new Recipe_Tag { Tag = tag }, new TagComparer())
                                    select recipes).ToList();

            ViewBag.TagResults = results;

            return View();
        }

    }


    private class TagComparer : IEqualityComparer<Recipe_Tag>
    {
        public bool equals(Recipe_Tag t1, Recipe_Tag t2)
        {
            return string.Equals(t1.Tag,t2.Tag,StringComparison.OrdinalIgnoreCase);
        }
    }
}
