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
        private UsersContext userDb = new UsersContext();
        private AmazonS3 s3client = new AmazonS3Client();

        //redirect if not logged in
        public ActionResult Index()
        {
            var recipes = GetRecipes(WebSecurity.CurrentUserId);
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
            ViewBag.MyPosts = GetBlogPosts(WebSecurity.CurrentUserId);

            return View();
        }

        public ActionResult ViewCookbook(int userId)
        {

            var recipes = GetRecipes(userId);

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
                ViewBag.Username = post.Username;
                postList.Add(post);

            }

            var blogPosts = GetBlogPosts(userId);
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
                ViewBag.Username = post.Username;
                postList.Add(post);
            }

            List<ViewPostModel> sortedPosts = postList.OrderByDescending(p => p.DateCreated).ToList();

            return View(sortedPosts);
        }


        public List<Recipe> GetRecipes(int userId)
        {
            var recipes = (from allRecipes in db.Recipes
                           where allRecipes.UserID == userId
                           select allRecipes).Take(30).ToList();
            return recipes;
        }

        public List<BlogPost> GetBlogPosts(int userId)
        {
            var posts = (from allPosts in db.BlogPosts
                         where allPosts.UserId == userId
                         select allPosts).Take(30).ToList();
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

        public ActionResult SendEmail(int userID) //Send notification to user via email.
        {
            try
            {
                String email = ""; //Find userID's email address.

                String[] arrayTO = new String[1];
                arrayTO[0] = email;

                List<string> listTO = arrayTO.ToList<String>();

                // Construct an object to contain the recipient address.
                Destination destination = new Destination().WithToAddresses(listTO);

                String FROM = "thecookbooksubscription@gmail.com";
                String SUBJECT = "The Cookbook - New Notification Waiting For You";
                String BODY = "You have received a new notification. Visit The Cookbook for more information.";

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
                TempData["sendstatus"] = "Sent successfully.";

            }
            catch (Exception e)
            {
                TempData["sendstatus"] = e.Message;
            }

            return this.RedirectToAction("Index");
        }

        public ActionResult SendSMS(int userID) //Send notification to user via SMS.
        {
            String userName = ""; //Resolve username from userID
            AmazonSimpleNotificationServiceClient client = new AmazonSimpleNotificationServiceClient();
            PublishRequest request = new PublishRequest
            {
                TopicArn = "arn:aws:sns:us-east-1:727060774285:INSERT_USERNAME",
                Subject = "The Cookbook - New Notification",
                Message = "Hello. You have received a new notification. Visit The Cookbook for more information."
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

                TempData["result"] = strings;

            }
            catch (Exception e)
            {
                TempData["error"] = e.Message;
            }

            return this.RedirectToAction("Index");
        }

    }
}
