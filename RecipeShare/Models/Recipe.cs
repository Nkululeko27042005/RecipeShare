using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecipeShare.Models
{
    public class Recipe
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        public string Category { get; set; } // e.g., "Dessert", "Main Course"

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Foreign key
        public string AuthorId { get; set; }
        public virtual ApplicationUser Author { get; set; }

        // Media
        public string VideoUrl { get; set; } // Optional video for the dish

        // Navigation
        public virtual ICollection<RecipeImage> Images { get; set; }
        public virtual ICollection<Ingredient> Ingredients { get; set; }
        public virtual ICollection<RecipeStep> Steps { get; set; }
        public virtual ICollection<RecipeRating> Ratings { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<SavedRecipe> SavedByUsers { get; set; }
    }

    // Models/RecipeImage.cs
    public class RecipeImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; } // First image shown in feed

        public int RecipeId { get; set; }
        public virtual Recipe Recipe { get; set; }
    }

    // Models/Ingredient.cs
    public class Ingredient
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Quantity { get; set; } // e.g., "2", "1/2"
        public string Unit { get; set; }     // e.g., "cups", "grams", "tbsp"
        public string Weight { get; set; }   // e.g., "200g" (optional)

        public int RecipeId { get; set; }
        public virtual Recipe Recipe { get; set; }
    }

    // Models/RecipeStep.cs
    public class RecipeStep
    {
        public int Id { get; set; }
        public int StepNumber { get; set; } // 1, 2, 3...
        public string Instruction { get; set; }

        public int RecipeId { get; set; }
        public virtual Recipe Recipe { get; set; }
    }

    // Models/RecipeRating.cs
    public class RecipeRating
    {
        public int Id { get; set; }
        public bool IsLike { get; set; } // true = like, false = dislike

        [Required]
        [Index("IX_User_Recipe_Rating", 1, IsUnique = true)]
        [StringLength(128)]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Index("IX_User_Recipe_Rating", 2, IsUnique = true)]
        public int RecipeId { get; set; }
        public virtual Recipe Recipe { get; set; }
    }

    // Models/SavedRecipe.cs
    public class SavedRecipe
    {
        public int Id { get; set; }
        public DateTime SavedAt { get; set; }

        [Required]
        [Index("IX_User_Recipe_Saved", 1, IsUnique = true)]
        [StringLength(128)]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Index("IX_User_Recipe_Saved", 2, IsUnique = true)]
        public int RecipeId { get; set; }
        public virtual Recipe Recipe { get; set; }
    }

    // Models/Comment.cs
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int RecipeId { get; set; }
        public virtual Recipe Recipe { get; set; }
    }

    // Models/Follow.cs
    public class Follow
    {
        public int Id { get; set; }

        [Required]
        [Index("IX_Follower_Followee", 1, IsUnique = true)]
        [StringLength(128)]
        public string FollowerId { get; set; }  // The user who follows
        public virtual ApplicationUser Follower { get; set; }

        [Required]
        [Index("IX_Follower_Followee", 2, IsUnique = true)]
        [StringLength(128)]
        public string FolloweeId { get; set; }  // The user being followed
        public virtual ApplicationUser Followee { get; set; }
    }
}
