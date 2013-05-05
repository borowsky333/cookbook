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

        public ActionResult SearchUsers(string query)
        {
            //Search Tags
            List<UserProfile> UserResults = (from allUsers in userDb.UserProfiles
                                        where allUsers.UserName.Contains(query)
                                        select allUsers).ToList();

            UserProfile exactMatch = UserResults.Find(delegate(UserProfile up)
                                    {
                                        return string.Equals(up.UserName,
                                                             query,
                                                             StringComparison.OrdinalIgnoreCase);
                                    });

            if (exactMatch != null)//if there is an exact match, move it to the top of the list
            {
                UserResults.Remove(exactMatch);
                UserResults.Insert(0, exactMatch);
            }

            ViewBag.UserResults = UserResults;
            
            //Search Users
            List<Recipe> TagResults = (from recipes in db.Recipes
                                    where recipes.Recipe_Tags.Contains(new Recipe_Tag { Tag = query }, new TagComparer())
                                    select recipes).ToList();

            ViewBag.TagResults = TagResults;

            return View();
        }

    }


    public class TagComparer : IEqualityComparer<Recipe_Tag>
    {
        public bool Equals(Recipe_Tag t1, Recipe_Tag t2)
        {
            return string.Equals(t1.Tag,t2.Tag,StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(Recipe_Tag tag)
        {
            return tag.Tag.GetHashCode();
        }
    }
}
