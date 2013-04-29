using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Cookbook.Models;

using Amazon.S3;
using Amazon.S3.Model;

namespace Cookbook.Controllers
{
    [Authorize]
    public class CookbookController : Controller
    {
        private CookbookDBModelsDataContext db = new CookbookDBModelsDataContext();
        private AmazonS3 s3client = new AmazonS3Client();



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
        public ActionResult UploadRecipe(UploadRecipeModel newRecipe, HttpPostedFileBase file)
        {
            Recipe recipeEntry = new Recipe();
            recipeEntry.Title = newRecipe.Title;
            recipeEntry.Instructions = newRecipe.Instructions;

            //TODO: need logic for adding tags to tag table.
            recipeEntry.DateCreated = DateTime.Now;

            recipeEntry.DateModified = DateTime.Now;
            recipeEntry.UserID = (int)Membership.GetUser().ProviderUserKey;

            db.Recipes.InsertOnSubmit(recipeEntry);

            db.SubmitChanges();

            //If there is an image, upload it to S3
            if (file != null && file.ContentLength > 0)
            {
                //make sure the file is less than 5 MB
                if (file.ContentLength > 5 * 1024 * 1024)
                {
                    ViewBag.Error = "Image must be less than 5 MB";
                    return View("Error");
                }

                //make sure the file is an image
                string[] acceptedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" };
                bool isImage = false;
                foreach (string extension in acceptedExtensions)
                {
                    if (file.FileName.ToLower().EndsWith(extension))
                    {
                        isImage = true;
                        break;
                    }
                }
                if (!isImage)
                {
                    ViewBag.Error = "Image format not supported.  Accepted formats are: " +
                             ".jpg, .jpeg, .png, .gif, .bmp, and .tiff";
                    return View("Error");
                }

                //upload the image to S3
                string imageKey = "recipes/" + (int)Membership.GetUser().ProviderUserKey + "/" + recipeEntry.RecipeID;
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = "Cookbook_Images",
                    Key = imageKey,
                    InputStream = file.InputStream,
                };

                try
                {
                    s3client.PutObject(request);
                }
                catch (AmazonS3Exception e)
                {
                    ViewBag.Error = "An error occurred storing the image:\n" + e.Message;
                    return View("Error");
                }

                //store the image info in the database
                /*Image dbImage = new Image
                {
                    DateCreated = DateTime.Now,
                    UserId = (int)Membership.GetUser().ProviderUserKey,
                    DateModified = DateTime.Now,
                    ImageDescription = "",
                    ImageTitle = file.FileName,
                    ImageUrl = "https://s3.amazonaws.com/Cookbook_Images/" + imageKey
                };
                db.Images.InsertOnSubmit(dbImage);*/

                string imageUrl = "https://s3.amazonaws.com/Cookbook_Images/" + imageKey;
                object[] param = { };
                db.ExecuteQuery<Object>(@"UPDATE Recipe " +
                                "SET Recipe.ImageUrl='" + imageUrl + "'" +
                                "WHERE Recipe.RecipeId='" + recipeEntry.RecipeID + "'", param);
                
                

                
            }
            

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
        public ActionResult UploadPost(UploadBlogPostModel newPost, HttpPostedFileBase file)
        {
            BlogPost post = new BlogPost();
            post.Title = newPost.Title;
            post.Post = newPost.Post;

            post.DateCreated = DateTime.Now;

            post.DateModified = DateTime.Now;
            post.UserId = (int)Membership.GetUser().ProviderUserKey;


            db.BlogPosts.InsertOnSubmit(post);
            db.SubmitChanges();


            //If there is an image, upload it to S3
            if (file != null && file.ContentLength > 0)
            {
                //make sure the file is less than 5 MB
                if (file.ContentLength > 5 * 1024 * 1024)
                {
                    ViewBag.Error = "Image must be less than 5 MB";
                    return View("Error");
                }

                //make sure the file is an image
                string[] acceptedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" };
                bool isImage = false;
                foreach (string extension in acceptedExtensions)
                {
                    if (file.FileName.ToLower().EndsWith(extension))
                    {
                        isImage = true;
                        break;
                    }
                }
                if (!isImage)
                {
                    ViewBag.Error = "Image format not supported.  Accepted formats are: " +
                             ".jpg, .jpeg, .png, .gif, .bmp, and .tiff";
                    return View("Error");
                }

                //upload the image to S3
                string imageKey = "posts/" + (int)Membership.GetUser().ProviderUserKey + "/" + post.BlogPostId;
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = "Cookbook_Images",
                    Key = imageKey,
                    InputStream = file.InputStream,
                };

                try
                {
                    s3client.PutObject(request);
                }
                catch (AmazonS3Exception e)
                {
                    ViewBag.Error = "An error occurred storing the image:\n" + e.Message;
                    return View("Error");
                }

                string imageUrl = "https://s3.amazonaws.com/Cookbook_Images/" + imageKey;
                object[] param = {};
                db.ExecuteQuery<Object>(@"UPDATE BlogPost " +
                                "SET BlogPost.ImageUrl='" + imageUrl + "'" +
                                "WHERE BlogPost.BlogPostId='" + post.BlogPostId + "'", param);
            }


            


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
