using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cookbook.Models;

namespace Cookbook.Controllers
{
    [Authorize]
    public class DiscoveryFeedController : Controller
    {
        private CookbookDBModelsDataContext db = new CookbookDBModelsDataContext();
        private UsersContext userDb = new UsersContext();

        public ActionResult Index()
        {
            var recipeList =
                (from recipes in db.Recipes
                 select recipes)
                 .Take(50)
                 .ToList();

            List<ViewPostModel> postList = new List<ViewPostModel>();

            foreach (var recipe in recipeList)
            {
                ViewRecipeModel recipeView = new ViewRecipeModel();
                recipeView.DateModified = recipe.DateModified;
                recipeView.FavoriteCount = recipe.FavoriteCount;
                recipeView.Instructions = recipe.Instructions;
                recipeView.LikeCount = recipe.LikeCount;

                var ingredients =
                    (from allIngredients in db.Ingredients
                     where allIngredients.RecipeId == recipe.RecipeID
                     select allIngredients.Name).ToList();
                recipeView.Ingredients = ingredients;

                var tags =
                    (from allTags in db.Recipe_Tags
                     where allTags.RecipeID == recipe.RecipeID
                     select allTags.Tag).ToList();
                recipeView.Tags = tags;

                ViewPostModel post = new ViewPostModel();
                post.DateCreated = recipe.DateCreated;
                post.RecipePost = recipeView;
                post.Username = (from userprofiles in userDb.UserProfiles
                                 where userprofiles.UserId == recipe.UserID
                                 select userprofiles.UserName).FirstOrDefault();
                post.ImageURL = recipe.ImageUrl;
                post.Title = recipe.Title;
                post.UserID = recipe.UserID;
                postList.Add(post);

            }

            var blogPostList =
                (from posts in db.BlogPosts
                select posts)
                .Take(50)
                .ToList();

            foreach (var blog in blogPostList)
            {
                ViewBlogModel blogView = new ViewBlogModel();
                blogView.DateModified = blog.DateModified;
                blogView.LikeCount = blog.LikeCount;
                blogView.Post = blog.Post;

                var tags =
                    (from allTags in db.BlogPost_Tags
                     where allTags.BlogPostId == blog.BlogPostId
                     select allTags.Tag).ToList();
                blogView.Tags = tags;

                ViewPostModel post = new ViewPostModel();
                post.DateCreated = blog.DateCreated;
                post.BlogPost = blogView;
                post.Username = (from userprofiles in userDb.UserProfiles
                                 where userprofiles.UserId == blog.UserId
                                 select userprofiles.UserName).FirstOrDefault();
                post.ImageURL = blog.ImageUrl;
                post.Title = blog.Title;
                post.UserID = blog.UserId;
                postList.Add(post);
            }

            List<ViewPostModel> sortedPosts = postList.OrderByDescending(p => p.DateCreated).ToList();



            return View(sortedPosts);
        }


        public ActionResult FilterByTag(string tag)
        {
            return View();
        }

    }
}
