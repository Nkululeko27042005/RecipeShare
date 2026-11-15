using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace RecipeShare.Models.Services
{
    public class RecommendationService
    {
        private readonly ApplicationDbContext _context;

        public RecommendationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Recipe> GetRecommendedRecipes(string userId, int page = 1, int pageSize = 20)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return new List<Recipe>();

            var preferredCategories = string.IsNullOrEmpty(user.PreferredCategoriesCsv)
                ? new List<string>()
                : user.PreferredCategoriesCsv.Split(',').Select(c => c.Trim()).Where(c => !string.IsNullOrEmpty(c)).ToList();

            var followedUserIds = _context.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FolloweeId)
                .ToList();

            var query = _context.Recipes
                .Include(r => r.Author)
                .Include(r => r.Images)
                .Include(r => r.Ratings)
                .Include(r => r.Comments)
                .OrderByDescending(r =>
                    (followedUserIds.Contains(r.AuthorId) ? 10 : 0)
                    + (preferredCategories.Contains(r.Category) ? 5 : 0)
                    + r.Ratings.Count(rt => rt.IsLike))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return query;
        }
    }
}
