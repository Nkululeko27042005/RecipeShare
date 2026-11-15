using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RecipeShare.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string DisplayName { get; set; }

        public string PreferredCategoriesCsv { get; set; }
    }

    public class CreateRecipeViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string VideoUrl { get; set; }
        public List<string> ImageUrls { get; set; }
        public List<IngredientInput> Ingredients { get; set; }
        public List<string> Steps { get; set; }
    }

    public class IngredientInput
    {
        public string Name { get; set; }
        public string Quantity { get; set; }
        public string Unit { get; set; }
        public string Weight { get; set; }
    }

    public class RecipeShowcaseItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Author { get; set; }
        public string PrimaryImage { get; set; }
        public int LikeCount { get; set; }
    }
}
