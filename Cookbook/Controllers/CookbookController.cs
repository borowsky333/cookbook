using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;
using Cookbook.Models;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;


namespace Cookbook.Controllers
{
    [Authorize]
    public class CookbookController : Controller
    {
        private CookbookDBModelsDataContext db = new CookbookDBModelsDataContext();
        private static UsersContext userDb = new UsersContext();
        private AmazonS3 s3client = new AmazonS3Client();

        /// <summary>
        /// Returns user's own cookbook.
        /// </summary>
        /// <param name="page">The page number the user currently is viewing</param>
        /// <returns></returns>
        public ActionResult Index(Nullable<int> page)
        {
            ViewBag.UserId = WebSecurity.CurrentUserId;

            var recipes = GetRecipes(WebSecurity.CurrentUserId);

            if (page == null || page < 1)
                page = 1;
            List<ViewPostModel> sortedPosts = GetCombinedPosts(GetRecipes(WebSecurity.CurrentUserId),
                                                               GetBlogPosts(WebSecurity.CurrentUserId),
                                                               (int)page, ViewBag);

            return View(sortedPosts);
        }

        /// <summary>
        /// View another user's cookbook
        /// </summary>
        /// <param name="userId">The user you're looking at</param>
        /// <param name="page">Current page number</param>
        /// <returns></returns>
        public ActionResult ViewCookbook(int userId, Nullable<int> page)
        {
            try
            {
                //if the user id is the logged in user's, redirect to index
                if (userId == (int)Membership.GetUser().ProviderUserKey)
                {
                    return RedirectToAction("Index");
                }

                User_Subscriber us = new User_Subscriber
                {
                    UserId = (int)Membership.GetUser().ProviderUserKey,
                    SubscriberId = userId
                };

                if (db.User_Subscribers.Contains(us))
                {
                    ViewBag.IsSubscribed = true;
                }
                else
                {
                    ViewBag.IsSubscribed = false;
                }

                ViewBag.UserId = userId;
                ViewBag.Username = (from allUsers in userDb.UserProfiles
                                    where allUsers.UserId == userId
                                    select allUsers.UserName).First();

                if (page == null || page < 1)
                    page = 1;
                var sortedPosts = GetCombinedPosts(GetRecipes(userId), GetBlogPosts(userId), (int)page, ViewBag);

                return View(sortedPosts);
            }
            catch (InvalidOperationException e)
            {
                ViewBag.Error = "Error retrieving user profile";
                return View("Error");
            }
        }

        /// <summary>
        /// Grabs all recipes from a single user
        /// </summary>
        /// <param name="userId">the user to retrieve recipes from</param>
        /// <returns></returns>
        public List<Recipe> GetRecipes(int userId)
        {
            var recipes = (from allRecipes in db.Recipes
                           where allRecipes.UserID == userId
                           select allRecipes).ToList();
            return recipes;
        }

        /// <summary>
        /// Grabs all blog posts from a single user
        /// </summary>
        /// <param name="userId">the user to retrieve blog posts from</param>
        /// <returns></returns>
        public List<BlogPost> GetBlogPosts(int userId)
        {
            var posts = (from allPosts in db.BlogPosts
                         where allPosts.UserId == userId
                         select allPosts).ToList();
            return posts;
        }


        /// <summary>
        /// Retrieves a list of post view models (both recipes and blogs) and orders them by date (newest first).
        /// </summary>
        /// <param name="recipes">The recipes to create view models for</param>
        /// <param name="blogPosts">The blog posts to create view models for</param>
        /// <param name="page">The current page number</param>
        /// <param name="ViewBag"></param>
        /// <returns>Date ordered list of recipes/blogs</returns>
        public static List<ViewPostModel> GetCombinedPosts(List<Recipe> recipes, List<BlogPost> blogPosts,
                                                           int page, dynamic ViewBag)
        {

            CookbookDBModelsDataContext db = new CookbookDBModelsDataContext();
            UsersContext userDb = new UsersContext();

            List<ViewPostModel> postList = new List<ViewPostModel>();

            foreach (var recipe in recipes)
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
                post.PostId = recipe.RecipeID;
                post.UserID = recipe.UserID;
                if (db.Recipe_Likers.Contains(new Recipe_Liker { UserId = WebSecurity.CurrentUserId, RecipeId = post.PostId }))
                {
                    post.Liked = true;
                }
                else
                {
                    post.Liked = false;
                }
                postList.Add(post);

            }

            foreach (var blog in blogPosts)
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
                post.PostId = blog.BlogPostId;
                post.UserID = blog.UserId;
                if (db.BlogPost_Likers.Contains(new BlogPost_Liker { UserId = WebSecurity.CurrentUserId, BlogPostId = post.PostId }))
                {
                    post.Liked = true;
                }
                else
                {
                    post.Liked = false;
                }
                postList.Add(post);
            }


            int pageLength = 15;
            int startPage = (page - 1) * pageLength;
            ViewBag.Page = (int)page;
            ViewBag.LastPage = (int)Math.Ceiling((double)postList.Count() / pageLength);

            if (startPage < postList.Count)
            {
                if (startPage + pageLength - 1 >= postList.Count)
                {
                    pageLength = postList.Count - startPage;
                }

                return postList.OrderByDescending(p => p.DateCreated).ToList().GetRange(startPage, pageLength);
            }
            else
            {
                return new List<ViewPostModel>();//there are no results in this page, return an empty list
            }

        }

        /// <summary>
        /// Upload recipe page
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadRecipe()
        {
            return View();
        }

        /// <summary>
        /// Enters uploaded recipe into database
        /// </summary>
        /// <param name="newRecipe">The user-uploaded recipe</param>
        /// <param name="file">The image</param>
        /// <returns>Redirects to user's cookbook</returns>
        [HttpPost]
        public ActionResult UploadRecipe(UploadRecipeModel newRecipe, HttpPostedFileBase file)
        {
            Recipe recipeEntry = new Recipe();
            recipeEntry.Title = newRecipe.Title;
            recipeEntry.Instructions = newRecipe.Instructions;
            recipeEntry.DateCreated = DateTime.Now;
            recipeEntry.DateModified = DateTime.Now;
            recipeEntry.UserID = WebSecurity.CurrentUserId;

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
                string imageKey = "recipes/" + WebSecurity.CurrentUserId + "/" + recipeEntry.RecipeID;
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

        /// <summary>
        /// Retrieves blog post upload page
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadPost()
        {
            return View();
        }

        /// <summary>
        /// Inserts uploaded blog post into the database.
        /// </summary>
        /// <param name="newPost">The blog post being uploaded</param>
        /// <param name="file">The image to attach.</param>
        /// <returns>Redirects to user's cookbook</returns>
        [HttpPost]
        public ActionResult UploadPost(UploadBlogPostModel newPost, HttpPostedFileBase file)
        {
            BlogPost post = new BlogPost();
            post.Title = newPost.Title;
            post.Post = newPost.Post;
            post.DateCreated = DateTime.Now;
            post.DateModified = DateTime.Now;
            post.UserId = WebSecurity.CurrentUserId;

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
                object[] param = { };
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

        /// <summary>
        /// Favorites another user's recipe
        /// </summary>
        /// <param name="recipeId">The recipe to favorite</param>
        /// <returns>Refreshes the page.</returns>
        public ActionResult FavoriteRecipe(int recipeId)
        {
            Recipe_Favoriter newFavoriter = new Recipe_Favoriter();
            newFavoriter.RecipeId = recipeId;
            newFavoriter.UserId = WebSecurity.CurrentUserId;
            if (!db.Recipe_Favoriters.Contains(newFavoriter))
            {
                db.Recipe_Favoriters.InsertOnSubmit(newFavoriter);
                var recipe = (from recipes in db.Recipes
                              where recipes.RecipeID == recipeId
                              select recipes).FirstOrDefault();
                recipe.FavoriteCount++;


                db.SubmitChanges();

                var favoriterUserName = (from userprofiles in userDb.UserProfiles
                                         where userprofiles.UserId == newFavoriter.UserId
                                     select userprofiles.UserName).FirstOrDefault();

                var userID = (from recipes in db.Recipes
                              where recipes.RecipeID == recipeId
                              select recipes.UserID).FirstOrDefault();

                SendSMS(userID, favoriterUserName + " has favorited one of your recipes. Come visit Cookbook and check out which recipe "
                    + favoriterUserName + " favorited!");
                SendEmail(userID, favoriterUserName + " has favorited one of your recipes.", favoriterUserName +
                    " has favorited one of your recipes. Come visit Cookbook and check out which recipe " +
                    favoriterUserName + " favorited!");
            }
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        public ActionResult AddComment(int postID, int commentID)
        {
            return View();
        }

        public ActionResult DeleteComment(int postID, int commentID)
        {
            return View();
        }

        /// <summary>
        /// Likes a user's blog post.
        /// </summary>
        /// <param name="postID">The blog post to like</param>
        /// <returns>Refreshes the page.</returns>
        public ActionResult LikeBlog(int postID)
        {
            BlogPost_Liker newLiker = new BlogPost_Liker();
            newLiker.BlogPostId = postID;
            newLiker.UserId = WebSecurity.CurrentUserId;
            if (!db.BlogPost_Likers.Contains(newLiker))
            {
                db.BlogPost_Likers.InsertOnSubmit(newLiker);

                var blogPost = (from blogs in db.BlogPosts
                                where blogs.BlogPostId == postID
                                select blogs).FirstOrDefault();
                blogPost.LikeCount++;

                db.SubmitChanges();

                var likerUserName = (from userprofiles in userDb.UserProfiles
                                     where userprofiles.UserId == newLiker.UserId
                                     select userprofiles.UserName).FirstOrDefault();

                var userID = (from blogs in db.BlogPosts
                              where blogs.BlogPostId == postID
                              select blogs.UserId).FirstOrDefault();
                SendSMS(userID, likerUserName + " has liked one of your posts. Come visit Cookbook and check out which post " + likerUserName + " liked!");
                SendEmail(userID, likerUserName + " has liked one of your posts.", likerUserName + " has liked one of your posts. Come visit Cookbook and check out which post " + likerUserName + " liked!");
            }
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        /// <summary>
        /// Likes a user's recipe.
        /// </summary>
        /// <param name="postID">The recipe to like</param>
        /// <returns>Refreshes the page.</returns>
        public ActionResult LikeRecipe(int postID)
        {
            Recipe_Liker newLiker = new Recipe_Liker();
            newLiker.RecipeId = postID;
            newLiker.UserId = WebSecurity.CurrentUserId;
            if (!db.Recipe_Likers.Contains(newLiker))
            {
                db.Recipe_Likers.InsertOnSubmit(newLiker);
                var recipe = (from recipes in db.Recipes
                              where recipes.RecipeID == postID
                              select recipes).FirstOrDefault();
                recipe.LikeCount++;


                db.SubmitChanges();

                var likerUserName = (from userprofiles in userDb.UserProfiles
                                     where userprofiles.UserId == newLiker.UserId
                                     select userprofiles.UserName).FirstOrDefault();

                var userID = (from recipes in db.Recipes
                              where recipes.RecipeID == postID
                              select recipes.UserID).FirstOrDefault();

                SendSMS(userID, likerUserName + " has liked one of your recipes. Come visit Cookbook and check out which recipe " + likerUserName + " liked!");
                SendEmail(userID, likerUserName + " has liked one of your recipes.", likerUserName + " has liked one of your recipes. Come visit Cookbook and check out which recipe " + likerUserName + " liked!");
            }
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        /// <summary>
        /// Unlikes a user's blog post.
        /// </summary>
        /// <param name="postID">The blog post to unlike</param>
        /// <returns>Refreshes the page.</returns>
        public ActionResult UnlikeBlog(int postId)
        {
            var unliker = (from likes in db.BlogPost_Likers
                           where likes.BlogPostId == postId && likes.UserId == WebSecurity.CurrentUserId
                           select likes).FirstOrDefault();

            db.BlogPost_Likers.DeleteOnSubmit(unliker);
            var blog = (from blogs in db.BlogPosts
                        where blogs.BlogPostId == postId
                        select blogs).FirstOrDefault();
            blog.LikeCount--;

            db.SubmitChanges();

            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        /// <summary>
        /// Unlikes a user's recipe.
        /// </summary>
        /// <param name="postID">The recipe to unlike</param>
        /// <returns>Refreshes the page.</returns>
        public ActionResult UnlikeRecipe(int postId)
        {
            var unliker = (from likes in db.Recipe_Likers
                           where likes.RecipeId == postId && likes.UserId == WebSecurity.CurrentUserId
                           select likes).FirstOrDefault();
            db.Recipe_Likers.DeleteOnSubmit(unliker);
            var recipe = (from recipes in db.Recipes
                          where recipes.RecipeID == postId
                          select recipes).FirstOrDefault();
            recipe.LikeCount--;


            db.SubmitChanges();

            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        /// <summary>
        /// Retrieves who has liked the blog.
        /// </summary>
        /// <param name="postId">The blog post</param>
        /// <returns>Page displaying the likers</returns>
        public ActionResult DisplayBlogLikes(int postId)
        {
            var likerIds = (from bloglikers in db.BlogPost_Likers
                            where bloglikers.BlogPostId == postId
                            select bloglikers.UserId).ToList();

            var likerUsernames = (from users in userDb.UserProfiles
                                  where likerIds.Contains(users.UserId)
                                  select users.UserName).ToList();
            ViewBag.Likers = likerUsernames;
            ViewBag.LikeCount = likerUsernames.Count;
            return View("DisplayLikes");
        }

        /// <summary>
        /// Retrieves who has liked the recipe.
        /// </summary>
        /// <param name="postId">The recipe post</param>
        /// <returns>Page displaying the likers</returns>
        public ActionResult DisplayRecipeLikes(int postId)
        {
            var likerIds = (from recipelikers in db.Recipe_Likers
                            where recipelikers.RecipeId == postId
                            select recipelikers.UserId).ToList();

            var likerUsernames = (from users in userDb.UserProfiles
                                  where likerIds.Contains(users.UserId)
                                  select users.UserName).ToList();
            ViewBag.Likers = likerUsernames;
            ViewBag.LikeCount = likerUsernames.Count;

            return View("DisplayLikes");
        }

        public ActionResult Report(int id)
        {
            return View();
        }

        /// <summary>
        /// Uses SES to send an email to a user.
        /// </summary>
        /// <param name="userID">The user being sent the email</param>
        /// <param name="inputSubject">The subject of the email</param>
        /// <param name="inputBody">The body of the email</param>
        public static void SendEmail(int userID, string inputSubject, string inputBody) //Send notification to user via email.
        {
            try
            {
                String email = (from userprofiles in userDb.UserProfiles
                                where userprofiles.UserId == userID
                                select userprofiles.Email).FirstOrDefault(); ; //Find userID's email address.

                if (inputSubject == "")
                {
                    inputSubject = "The Cookbook - New Notification Waiting For You";
                }

                if (inputBody == "")
                {
                    inputBody = "You have received a new notification. Visit The Cookbook for more information.";
                }

                String[] arrayTO = new String[1];
                arrayTO[0] = email;

                List<string> listTO = arrayTO.ToList<String>();

                // Construct an object to contain the recipient address.
                Destination destination = new Destination().WithToAddresses(listTO);

                String FROM = "thecookbooksubscription@gmail.com";
                String SUBJECT = inputSubject;
                String BODY = inputBody;

                // Create the subject and body of the message.
                Content subject = new Content().WithData(SUBJECT);
                Content textBody = new Content().WithData(BODY);
                Body body = new Body().WithText(textBody);

                // Create a message with the specified subject and body.
                Message message = new Message().WithSubject(subject).WithBody(body);

                // Assemble the email.
                SendEmailRequest request = new SendEmailRequest().WithSource(FROM).WithDestination(destination).WithMessage(message);

                AmazonSimpleEmailServiceClient client = new AmazonSimpleEmailServiceClient();

                SendEmailResponse response = client.SendEmail(request);
                Console.WriteLine("Sent successfully.");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        /// <summary>
        /// Uses SNS to send an SMS to a user.
        /// </summary>
        /// <param name="userID">The user being sent an SMS</param>
        /// <param name="inputBody">The body of the SMS</param>
        public static void SendSMS(int userID, string inputBody)
        {
            String userName = (from userprofiles in userDb.UserProfiles
                               where userprofiles.UserId == userID
                               select userprofiles.UserName).FirstOrDefault(); //Resolve username from userID

            if (inputBody == "")
                inputBody = "Hello. You have received a new notification. Visit The Cookbook for more information.";

            AmazonSimpleNotificationServiceClient client = new AmazonSimpleNotificationServiceClient();
            PublishRequest request = new PublishRequest
            {
                TopicArn = "arn:aws:sns:us-east-1:727060774285:" + userName,
                Message = inputBody
            };

            try
            {
                PublishResponse response = client.Publish(request);
                PublishResult result = response.PublishResult;

                String[] strings = new String[1];

                for (int i = 0; i < strings.GetLength(0); i++)
                {
                    strings[i] = "Success! Message ID is: " + result.MessageId;
                }

                Console.WriteLine(strings[0]); ;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}
