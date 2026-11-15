using RecipeShare.Models.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RecipeShare.Models.Services
{
    public class RecipeService
    {
        private readonly ApplicationDbContext _context;
        private readonly RecipeRepository _recipeRepo;

        public RecipeService(ApplicationDbContext context)
        {
            _context = context;
            _recipeRepo = new RecipeRepository(context);
        }

        public Recipe CreateRecipe(Recipe recipe, string authorId, List<RecipeImage> images,
                                   List<Ingredient> ingredients, List<RecipeStep> steps)
        {
            recipe.AuthorId = authorId;
            recipe.CreatedAt = DateTime.UtcNow;

            // Normalize step numbers
            for (int i = 0; i < steps.Count; i++)
            {
                steps[i].StepNumber = i + 1;
            }

            recipe.Images = images;
            recipe.Ingredients = ingredients;
            recipe.Steps = steps;

            _recipeRepo.Add(recipe);
            _context.SaveChanges();
            return recipe;
        }

        public Recipe GetRecipe(int id) => _recipeRepo.GetWithDetails(id);

        public bool RateRecipe(int recipeId, string userId, bool isLike)
        {
            var existing = _context.RecipeRatings
                .FirstOrDefault(r => r.RecipeId == recipeId && r.UserId == userId);

            if (existing != null)
            {
                if (existing.IsLike == isLike)
                    return false; // Already rated this way

                existing.IsLike = isLike;
            }
            else
            {
                _context.RecipeRatings.Add(new RecipeRating
                {
                    RecipeId = recipeId,
                    UserId = userId,
                    IsLike = isLike
                });
            }

            _context.SaveChanges();
            return true;
        }

        public void SaveRecipe(int recipeId, string userId)
        {
            if (_context.SavedRecipes.Any(s => s.RecipeId == recipeId && s.UserId == userId))
                return;

            _context.SavedRecipes.Add(new SavedRecipe
            {
                RecipeId = recipeId,
                UserId = userId,
                SavedAt = DateTime.UtcNow
            });
            _context.SaveChanges();
        }

        public void UnsaveRecipe(int recipeId, string userId)
        {
            var saved = _context.SavedRecipes
                .FirstOrDefault(s => s.RecipeId == recipeId && s.UserId == userId);
            if (saved != null)
            {
                _context.SavedRecipes.Remove(saved);
                _context.SaveChanges();
            }
        }

        public Comment AddComment(int recipeId, string userId, string content)
        {
            var comment = new Comment
            {
                RecipeId = recipeId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };
            _context.Comments.Add(comment);
            _context.SaveChanges();
            return comment;
        }
    }
}