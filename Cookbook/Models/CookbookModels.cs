using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cookbook.Models
{
    public class Cookbook
    {
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

        //look up ingredient by name
        public Dictionary<string, Ingredient> Ingredients { get; set; }
        
        public string Instructions { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

        public List<Image> PhotoAlbum { get; set; }

        public HashSet<string> Tags { get; set; }

        //look up comment by comment ID
        public Dictionary<int, Comment> Comments { get; set; }
        public int FavoriteCount { get; set; }
        public string Favoriters { get; set; }
        public int LikeCount { get; set; }
        public string Likers { get; set; }
    }

    public class Ingredient
    {
        public string Quantity { get; set; }
        public string IngredientName { get; set; }
    }

    public class BlogPost
    {
        public int BlogPostID { get; set; }
        public int UserID { get; set; }

        public string Title { get; set; }
        public string Post { get; set; }
        public List<Image> PhotoAlbum { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

        public HashSet<string> Tags { get; set; }

        //look up comment by comment ID
        public Dictionary<int, Comment> Comments { get; set; }
        public string Likers { get; set; }
        public int LikeCount { get; set; }
    }

    public class Image
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
        public DateTime DateCreated { get; set; }
    }

}