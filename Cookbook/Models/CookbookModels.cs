using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Cookbook.Models
{
    public class ViewPostModel
    {
        public ViewBlogModel BlogPost { get; set; }
        public ViewRecipeModel RecipePost { get; set; }
        public DateTime DateCreated { get; set; }
        public string Username { get; set; }
        public string ImageURL { get; set; }
        public string Title { get; set; }
        public int UserID { get; set; }
    }

    public class ViewBlogModel
    {
        public string Title { get; set; }
        public string Post { get; set; }
        public string Username { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int LikeCount { get; set; }
        public string ImageURL { get; set; }
        public List<string> Tags { get; set; }
    }


    public class ViewRecipeModel
    {
        public string Title { get; set; }
        public List<string> Ingredients { get; set; }
        public string Instructions { get; set; }
        public string Username { get; set; }
        public string ImageURL { get; set; }
        public List<string> Tags { get; set; }
        public int LikeCount { get; set; }
        public int FavoriteCount { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        
    }

    public class UploadRecipeModel
    {
        [Required]
        [Display(Name = "Recipe Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Ingredients")]
        [DataType(DataType.MultilineText)]
        public string Ingredients { get; set; }

        [Required]
        [Display(Name = "Recipe Instructions")]
        [DataType(DataType.MultilineText)]
        public string Instructions { get; set; }

        [Required]
        [Display(Name = "Tags")]
        public string Tags { get; set; }
    }


    public class UploadBlogPostModel
    {
        [Required]
        [Display(Name = "Post Title")]
        public string Title { get; set; }
        
        [Required]
        [Display(Name = "Post")]
        [DataType(DataType.MultilineText)]
        public string Post { get; set; }
        
        [Required]
        [Display(Name = "Tags")]
        [DataType(DataType.MultilineText)]
        public string Tags { get; set; }

    }

    //    public class Image
    //    {
    //        public int ImageID { get; set; }
    //        public int UserID { get; set; }
    //        public string ImageUrl { get; set; }
    //        public string ImageTitle { get; set; }
    //        public string ImageDescription { get; set; }

    //        public DateTime DateCreated { get; set; }
    //        public DateTime DateModified { get; set; }

    //    }

    //    public class Comment
    //    {
    //        public int CommentID { get; set; }
    //        public int UserID { get; set; }
    //        public string Content { get; set; }
    //        public DateTime DateCreated { get; set; }
    //    }

}