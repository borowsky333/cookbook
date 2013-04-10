using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Cookbook.Models
{
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
        public string Post { get; set; }
        [Required]
        [Display(Name = "Tags")]
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