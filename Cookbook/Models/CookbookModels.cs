using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cookbook.Models
{
    public class Cookbook
    {
        public int CookbookID { get; set; }
        public int UserID { get; set; }

        //your own uploaded recipes
        public List<Recipe> Recipes { get; set; }

        //other user's recipes you've attached to your cookbook
        public List<Recipe> Favorites { get; set; }

        //blog (non-recipe) posts
        public List<BlogPost> Posts { get; set; }

    }

    public class Recipe
    {
        public int RecipeID { get; set; }
        public int UserID { get; set; }

        public string Title { get; set; }
        public List<string> Ingredients { get; set; }
        public string Instructions { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

        public List<string> PhotoAlbum { get; set; }

        public HashSet<string> Tags { get; set; }

        public List<Comment> Comments { get; set; }
    }

    public class BlogPost
    {
        public int BlogPostID { get; set; }
        public int UserID { get; set; }

        public string Title { get; set; }
        public string Post { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

        public HashSet<string> Tags { get; set; }

        public List<Comment> Comments { get; set; }
    }

    public class ImagePost
    {
        public int ImageID { get; set; }
        public int UserID { get; set; }
        public string ImageUrl { get; set; }
        public string ImageTitle { get; set; }
        public string ImageDescription { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

        public HashSet<string> Tags { get; set; }
    }

    public class Comment
    {
        public int CommentID { get; set; }
        public int UserID { get; set; }
        public string Content { get; set; }
        public string DateCreated { get; set; }
    }

}