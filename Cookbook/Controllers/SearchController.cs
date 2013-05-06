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

       /// <summary>
       /// Searches for users/tags that match the search term.
       /// </summary>
       /// <param name="q">The search term</param>
       /// <returns>Page displaying results</returns>
        public ActionResult Index(string q)
        {

            //Search Users
            List<UserProfile> UserResults = (from allUsers in userDb.UserProfiles
                                             where allUsers.UserName.Contains(q)
                                             select allUsers).ToList();

            
            UserProfile exactMatch = UserResults.Find(delegate(UserProfile up)
                                    {
                                        return string.Equals(up.UserName,
                                                             q,
                                                             StringComparison.OrdinalIgnoreCase);
                                    });

            if (exactMatch != null)//if there is an exact match, move it to the top of the list
            {
                UserResults.Remove(exactMatch);
                UserResults.Insert(0, exactMatch);
            }

            ViewBag.UserResults = UserResults;
            
            
            //Search Recipes
            List<Recipe> recipeResults = (from recipes in db.Recipes
                                    where recipes.Title.Contains(q)//Recipe_Tags.Contains(new Recipe_Tag { Tag = q })
                                    select recipes).ToList();

            List<BlogPost> postResults = (from posts in db.BlogPosts
                                          where posts.Title.Contains(q)
                                          select posts).ToList();

            //Will only return 15 results until paging is implemented for search results
            var combinedResults = CookbookController.GetCombinedPosts(recipeResults, postResults, 1, ViewBag);
            
            return View(combinedResults);
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
