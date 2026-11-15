using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RecipeShare.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public string ProfileImageUrl { get; set; }
        public string PreferredCategoriesCsv { get; set; } // Store category IDs/names as CSV for simple preference matching

        // Navigation properties
        public virtual ICollection<Recipe> Recipes { get; set; }
        public virtual ICollection<SavedRecipe> SavedRecipes { get; set; }
        public virtual ICollection<RecipeRating> Ratings { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Follow> Followers { get; set; }  // Users who follow this user
        public virtual ICollection<Follow> Following { get; set; }  // Users this user follows

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }
}
