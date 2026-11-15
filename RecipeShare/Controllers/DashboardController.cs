using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RecipeShare.Models;
using RecipeShare.Models.Services;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace RecipeShare.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RecommendationService _recommendationService;

        public DashboardController()
        {
            _context = new ApplicationDbContext();
            _recommendationService = new RecommendationService(_context);
        }

        private ContentResult CamelJson(object data)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            return Content(JsonConvert.SerializeObject(data, settings), "application/json");
        }

        // GET: /Dashboard (Page)
        [HttpGet]
        public ActionResult Index() => View();

        // GET: /Dashboard/GetDashboardData (JSON)
        [HttpGet]
        public ContentResult GetDashboardData()
        {
            var userId = User.Identity.GetUserId();

            var recommended = _recommendationService.GetRecommendedRecipes(userId, 1, 6)
                .Select(r => new
                {
                    r.Id,
                    r.Title,
                    r.Description,
                    Author = r.Author != null ? r.Author.DisplayName : "Chef",
                    r.Category,
                    PrimaryImage = r.Images?.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? r.Images?.FirstOrDefault()?.ImageUrl,
                    LikeCount = r.Ratings?.Count(rt => rt.IsLike) ?? 0,
                    CommentCount = r.Comments?.Count ?? 0
                }).ToList();

            var topLiked = _context.Recipes.Include(r => r.Ratings).Include(r => r.Author)
                .OrderByDescending(r => r.Ratings.Count(rt => rt.IsLike))
                .Take(5)
                .ToList()
                .Select(r => new { 
                    r.Id, 
                    r.Title, 
                    LikeCount = r.Ratings.Count(rt => rt.IsLike), 
                    Author = r.Author != null ? r.Author.DisplayName : "Chef" 
                })
                .ToList();

            var risingUsers = _context.Users.Include(u => u.Followers).Include(u => u.Recipes)
                .OrderByDescending(u => u.Followers.Count)
                .Take(5)
                .ToList()
                .Select(u => new { 
                    u.Id, 
                    DisplayName = u.DisplayName ?? u.UserName, 
                    u.ProfileImageUrl, 
                    FollowerCount = u.Followers.Count, 
                    RecipeCount = u.Recipes.Count 
                })
                .ToList();

            var savedRecipes = _context.SavedRecipes.Include(s => s.Recipe)
                .Where(s => s.UserId == userId)
                .ToList()
                .Select(s => new { s.Recipe.Id, s.Recipe.Title })
                .ToList();

            return CamelJson(new
            {
                Recommended = recommended,
                TopLiked = topLiked,
                RisingUsers = risingUsers,
                SavedRecipes = savedRecipes
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }
}
