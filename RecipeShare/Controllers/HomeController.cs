using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RecipeShare.Models;
using RecipeShare.Models.ViewModels;

namespace RecipeShare.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController()
        {
            _context = new ApplicationDbContext();
        }

        // GET: /
        public ActionResult Index()
        {
            // Pull a small showcase of top-liked recipes for the "from the kitchen" strip.
            // Using strongly-typed RecipeShowcaseItem so Razor doesn't hit RuntimeBinderException on anonymous types.
            var showcase = _context.Recipes
                .OrderByDescending(r => r.Ratings.Count(rt => rt.IsLike))
                .Take(6)
                .ToList()
                .Select(r => new RecipeShowcaseItem
                {
                    Id = r.Id,
                    Title = r.Title,
                    Category = r.Category,
                    Author = r.Author != null ? r.Author.DisplayName : "Chef",
                    PrimaryImage = r.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? r.Images.FirstOrDefault()?.ImageUrl,
                    LikeCount = r.Ratings.Count(rt => rt.IsLike)
                })
                .ToList();

            ViewBag.Showcase = showcase;
            ViewBag.CookCount = _context.Users.Count();
            ViewBag.RecipeCount = _context.Recipes.Count();
            ViewBag.LikeCount = _context.RecipeRatings.Count(rt => rt.IsLike);

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }
}
