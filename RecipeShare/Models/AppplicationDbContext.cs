using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace RecipeShare.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeImage> RecipeImages { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<RecipeStep> RecipeSteps { get; set; }
        public DbSet<RecipeRating> RecipeRatings { get; set; }
        public DbSet<SavedRecipe> SavedRecipes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Follow> Follows { get; set; }

        public ApplicationDbContext() : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Follow: prevent cascade delete cycle
            modelBuilder.Entity<Follow>()
                .HasRequired(f => f.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Follow>()
                .HasRequired(f => f.Followee)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FolloweeId)
                .WillCascadeOnDelete(false);

            // RecipeRating: prevent cascade on user delete
            modelBuilder.Entity<RecipeRating>()
                .HasRequired(r => r.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.UserId)
                .WillCascadeOnDelete(false);

            // SavedRecipe: prevent cascade on user delete
            modelBuilder.Entity<SavedRecipe>()
                .HasRequired(s => s.User)
                .WithMany(u => u.SavedRecipes)
                .HasForeignKey(s => s.UserId)
                .WillCascadeOnDelete(false);

            // Comment: prevent cascade on user delete
            modelBuilder.Entity<Comment>()
                .HasRequired(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .WillCascadeOnDelete(false);
        }
    }
}
