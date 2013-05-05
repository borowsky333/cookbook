using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cookbook.Models;
using WebMatrix.WebData;
using Amazon.SimpleEmail.Model;
using Amazon.SimpleEmail;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

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


        public ActionResult LikeBlog(int postID)
        {
            var blogPost = (from blogs in db.BlogPosts
                            where blogs.BlogPostId == postID
                            select blogs).FirstOrDefault();
            blogPost.LikeCount++;

            BlogPost_Liker newLiker = new BlogPost_Liker();
            newLiker.BlogPostId = postID;
            newLiker.UserId = WebSecurity.CurrentUserId;
            if (!db.BlogPost_Likers.Contains(newLiker))
                db.BlogPost_Likers.InsertOnSubmit(newLiker);

            db.SubmitChanges();

            var userID = (from blogs in db.BlogPosts
                          where blogs.BlogPostId == postID
                          select blogs.UserId).FirstOrDefault();
            SendSMS(userID);
            SendEmail(userID);


            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        public ActionResult LikeRecipe(int postID)
        {
            var recipe = (from recipes in db.Recipes
                          where recipes.RecipeID == postID
                          select recipes).FirstOrDefault();
            recipe.LikeCount++;

            Recipe_Liker newLiker = new Recipe_Liker();
            newLiker.RecipeId = postID;
            newLiker.UserId = WebSecurity.CurrentUserId;
            if (!db.Recipe_Likers.Contains(newLiker))
                db.Recipe_Likers.InsertOnSubmit(newLiker);

            db.SubmitChanges();

            var userID = (from recipes in db.Recipes
                          where recipes.RecipeID == postID
                          select recipes.UserID).FirstOrDefault();
            SendSMS(userID);
            SendEmail(userID);

            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        private void SendEmail(int userID) //Send notification to user via email.
        {
            try
            {
                String email = (from userprofiles in userDb.UserProfiles
                                where userprofiles.UserId == userID
                                select userprofiles.Email).FirstOrDefault(); ; //Find userID's email address.

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
        }

        private void SendSMS(int userID) //Send notification to user via SMS.
        {
            String userName = (from userprofiles in userDb.UserProfiles
                               where userprofiles.UserId == userID
                               select userprofiles.UserName).FirstOrDefault(); //Resolve username from userID
            AmazonSimpleNotificationServiceClient client = new AmazonSimpleNotificationServiceClient();
            PublishRequest request = new PublishRequest
            {
                TopicArn = "arn:aws:sns:us-east-1:727060774285:" + userName,
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
        }
    }
}
