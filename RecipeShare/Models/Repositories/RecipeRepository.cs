using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace RecipeShare.Models.Repositories
{
    public class RecipeRepository : Repository<Recipe>
    {
        public RecipeRepository(ApplicationDbContext context) : base(context) { }

        public Recipe GetWithDetails(int id)
        {
            return DbSet
                .Include(r => r.Author)
                .Include(r => r.Images)
                .Include(r => r.Ingredients)
                .Include(r => r.Steps)
                .Include(r => r.Comments.Select(c => c.User))
                .Include(r => r.Ratings)
                .FirstOrDefault(r => r.Id == id);
        }

        public IQueryable<Recipe> GetAllWithAuthor()
        {
            return DbSet.Include(r => r.Author).Include(r => r.Images);
        }

        public IQueryable<Recipe> GetByAuthor(string authorId)
        {
            return DbSet.Where(r => r.AuthorId == authorId)
                        .Include(r => r.Author)
                        .Include(r => r.Images);
        }

        public IQueryable<Recipe> GetTopLiked(int count)
        {
            return DbSet
                .Include(r => r.Author)
                .Include(r => r.Images)
                .OrderByDescending(r => r.Ratings.Count(rt => rt.IsLike))
                .Take(count);
        }
    }
}