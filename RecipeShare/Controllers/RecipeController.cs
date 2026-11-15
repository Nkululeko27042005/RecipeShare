using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RecipeShare.Models;
using RecipeShare.Models.Services;
using RecipeShare.Models.ViewModels;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecipeShare.Controllers
{
    [Authorize]
    public class RecipeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RecipeService _recipeService;
        private readonly RecommendationService _recommendationService;

        public RecipeController()
        {
            _context = new ApplicationDbContext();
            _recipeService = new RecipeService(_context);
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

        // GET: /Recipe/Feed (JSON)
        [HttpGet]
        public ContentResult Feed(int page = 1, int pageSize = 20, string tab = "all")
        {
            var userId = User.Identity.GetUserId();
            var followed = _context.Follows.Where(f => f.FollowerId == userId).Select(f => f.FolloweeId).ToList();

            var q = _context.Recipes.Include(r => r.Author).Include(r => r.Images).Include(r => r.Ratings).Include(r => r.Comments).AsQueryable();

            if (tab == "following")
                q = q.Where(r => followed.Contains(r.AuthorId));

            var recipes = q.OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .ToList()
                .Select(r => new
                {
                    r.Id,
                    r.Title,
                    r.Description,
                    r.Category,
                    Author = r.Author != null ? r.Author.DisplayName : "Chef",
                    AuthorProfileImageUrl = r.Author != null ? r.Author.ProfileImageUrl : null,
                    PrimaryImage = r.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? r.Images.FirstOrDefault()?.ImageUrl,
                    LikeCount = r.Ratings.Count(rt => rt.IsLike),
                    CommentCount = r.Comments.Count
                }).ToList();

            return CamelJson(recipes);
        }

        // GET: /Recipe/Details/5 (Page view)
        [HttpGet]
        public ActionResult Details(int id)
        {
            ViewBag.RecipeId = id;
            return View();
        }

        // GET: /Recipe/GetDetails/5 (JSON)
        [HttpGet]
        public ContentResult GetDetails(int id)
        {
            var recipe = _recipeService.GetRecipe(id);
            if (recipe == null) return CamelJson(new { error = "Not found" });

            var userId = User.Identity.GetUserId();
            var userRating = recipe.Ratings.FirstOrDefault(r => r.UserId == userId);

            return CamelJson(new
            {
                recipe.Id,
                recipe.Title,
                recipe.Description,
                recipe.Category,
                recipe.VideoUrl,
                Author = recipe.Author != null ? recipe.Author.DisplayName : "Chef",
                AuthorId = recipe.AuthorId,
                AuthorProfileImageUrl = recipe.Author != null ? recipe.Author.ProfileImageUrl : null,
                Images = recipe.Images.Select(i => new { i.ImageUrl, i.IsPrimary }),
                Ingredients = recipe.Ingredients.Select(i => new { i.Name, i.Quantity, i.Unit, i.Weight }),
                Steps = recipe.Steps.OrderBy(s => s.StepNumber).Select(s => new { s.StepNumber, s.Instruction }),
                Comments = recipe.Comments.OrderByDescending(c => c.CreatedAt).Select(c => new { 
                    DisplayName = c.User != null ? c.User.DisplayName : "Anonymous", 
                    ProfileImageUrl = c.User != null ? c.User.ProfileImageUrl : null,
                    c.Content, 
                    c.CreatedAt 
                }),
                LikeCount = recipe.Ratings.Count(r => r.IsLike),
                DislikeCount = recipe.Ratings.Count(r => !r.IsLike),
                UserHasLiked = userRating != null && userRating.IsLike,
                UserHasDisliked = userRating != null && !userRating.IsLike,
                IsSaved = recipe.SavedByUsers.Any(s => s.UserId == userId)
            });
        }

        // POST: /Recipe/Create
        [HttpPost]
        public ContentResult Create(CreateRecipeViewModel model)
        {
            if (!ModelState.IsValid)
                return CamelJson(new { success = false, errors = "Invalid data" });

            var userId = User.Identity.GetUserId();
            var recipe = new Recipe
            {
                Title = model.Title,
                Description = model.Description,
                Category = model.Category,
                VideoUrl = model.VideoUrl
            };

            var images = model.ImageUrls?.Select((url, idx) => new RecipeImage
            {
                ImageUrl = url,
                IsPrimary = idx == 0
            }).ToList() ?? new List<RecipeImage>();

            var ingredients = model.Ingredients?.Select(i => new Ingredient
            {
                Name = i.Name,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Weight = i.Weight
            }).ToList() ?? new List<Ingredient>();

            var steps = model.Steps?.Select((instruction, idx) => new RecipeStep
            {
                StepNumber = idx + 1,
                Instruction = instruction
            }).ToList() ?? new List<RecipeStep>();

            var created = _recipeService.CreateRecipe(recipe, userId, images, ingredients, steps);

            return CamelJson(new { success = true, recipeId = created.Id });
        }

        // POST: /Recipe/Rate
        [HttpPost]
        public ContentResult Rate(int recipeId, bool isLike)
        {
            var userId = User.Identity.GetUserId();
            _recipeService.RateRecipe(recipeId, userId, isLike);
            var recipe = _recipeService.GetRecipe(recipeId);
            return CamelJson(new
            {
                success = true,
                likeCount = recipe?.Ratings.Count(r => r.IsLike) ?? 0,
                dislikeCount = recipe?.Ratings.Count(r => !r.IsLike) ?? 0
            });
        }

        // POST: /Recipe/Save
        [HttpPost]
        public ContentResult Save(int recipeId)
        {
            var userId = User.Identity.GetUserId();
            _recipeService.SaveRecipe(recipeId, userId);
            return CamelJson(new { success = true });
        }

        // POST: /Recipe/Unsave
        [HttpPost]
        public ContentResult Unsave(int recipeId)
        {
            var userId = User.Identity.GetUserId();
            _recipeService.UnsaveRecipe(recipeId, userId);
            return CamelJson(new { success = true });
        }

        // POST: /Recipe/Comment
        [HttpPost]
        public ContentResult Comment(int recipeId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return CamelJson(new { success = false, error = "Comment cannot be empty" });

            var userId = User.Identity.GetUserId();
            var comment = _recipeService.AddComment(recipeId, userId, content);
            var user = _context.Users.Find(userId);
            return CamelJson(new
            {
                success = true,
                commentId = comment.Id,
                displayName = user?.DisplayName ?? "Anonymous",
                profileImageUrl = user?.ProfileImageUrl,
                content = comment.Content,
                createdAt = comment.CreatedAt
            });
        }

        // GET: /Recipe/GetTopLiked (JSON)
        [HttpGet]
        public ContentResult GetTopLiked(int count = 10)
        {
            var recipes = _context.Recipes.Include(r => r.Author).Include(r => r.Images).Include(r => r.Ratings)
                .OrderByDescending(r => r.Ratings.Count(rt => rt.IsLike))
                .Take(count)
                .ToList()
                .Select(r => new
                {
                    r.Id,
                    r.Title,
                    Author = r.Author != null ? r.Author.DisplayName : "Chef",
                    PrimaryImage = r.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? r.Images.FirstOrDefault()?.ImageUrl,
                    LikeCount = r.Ratings.Count(rt => rt.IsLike)
                })
                .ToList();

            return CamelJson(recipes);
        }

        // GET: /Recipe/TopLiked (Page)
        [HttpGet]
        public ActionResult TopLiked() => View();

        // GET: /Recipe/GetMyRecipes (JSON)
        [HttpGet]
        public ContentResult GetMyRecipes()
        {
            var userId = User.Identity.GetUserId();
            var recipes = _context.Recipes.Include(r => r.Images).Include(r => r.Ratings)
                .Where(r => r.AuthorId == userId)
                .ToList()
                .Select(r => new
                {
                    r.Id,
                    r.Title,
                    r.Category,
                    PrimaryImage = r.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? r.Images.FirstOrDefault()?.ImageUrl,
                    LikeCount = r.Ratings.Count(rt => rt.IsLike)
                })
                .ToList();

            return CamelJson(recipes);
        }

        // GET: /Recipe/MyRecipes (Page)
        [HttpGet]
        public ActionResult MyRecipes() => View();

        // GET: /Recipe/GetSavedRecipes (JSON)
        [HttpGet]
        public ContentResult GetSavedRecipes()
        {
            var userId = User.Identity.GetUserId();
            var recipes = _context.SavedRecipes.Include(s => s.Recipe.Author).Include(s => s.Recipe.Images)
                .Where(s => s.UserId == userId)
                .ToList()
                .Select(s => new
                {
                    s.Recipe.Id,
                    s.Recipe.Title,
                    Author = s.Recipe.Author != null ? s.Recipe.Author.DisplayName : "Chef",
                    PrimaryImage = s.Recipe.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? s.Recipe.Images.FirstOrDefault()?.ImageUrl
                })
                .ToList();

            return CamelJson(recipes);
        }

        // GET: /Recipe/SavedRecipes (Page)
        [HttpGet]
        public ActionResult SavedRecipes() => View();

        // GET: /Recipe/Create (Page)
        [HttpGet]
        public ActionResult Create() => View();

        // POST: /Recipe/UploadImage
        [HttpPost]
        public ContentResult UploadImage(HttpPostedFileBase file)
        {
            var fs = new FileService(Server.MapPath("~"));
            var url = fs.SaveFile(file, "images");
            return CamelJson(new { url });
        }

        // POST: /Recipe/UploadVideo
        [HttpPost]
        public ContentResult UploadVideo(HttpPostedFileBase file)
        {
            var fs = new FileService(Server.MapPath("~"));
            var url = fs.SaveFile(file, "videos");
            return CamelJson(new { url });
        }

        // GET: /Recipe/ByUser/{id} (JSON)
        [HttpGet]
        public ContentResult ByUser(string id)
        {
            var recipes = _context.Recipes.Include(r => r.Images)
                .Where(r => r.AuthorId == id)
                .ToList()
                .Select(r => new
                {
                    r.Id,
                    r.Title,
                    r.Category,
                    PrimaryImage = r.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? r.Images.FirstOrDefault()?.ImageUrl
                }).ToList();
            return CamelJson(recipes);
        }

        // GET: /Recipe/Search (Page or JSON)
        [HttpGet]
        public ActionResult Search(string q)
        {
            if (Request.IsAjaxRequest() || (Request.AcceptTypes != null && Request.AcceptTypes.Any(t => t.Contains("application/json"))))
            {
                if (string.IsNullOrWhiteSpace(q))
                    return CamelJson(new List<object>());

                var term = q.ToLower();
                var results = _context.Recipes.Include(r => r.Author).Include(r => r.Images).Include(r => r.Ratings)
                    .Where(r => r.Title.ToLower().Contains(term) || r.Description.ToLower().Contains(term) || r.Category.ToLower().Contains(term))
                    .OrderByDescending(r => r.Ratings.Count(rt => rt.IsLike))
                    .Take(30)
                    .ToList()
                    .Select(r => new
                    {
                        r.Id,
                        r.Title,
                        r.Description,
                        r.Category,
                        Author = r.Author != null ? r.Author.DisplayName : "Chef",
                        PrimaryImage = r.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? r.Images.FirstOrDefault()?.ImageUrl,
                        LikeCount = r.Ratings.Count(rt => rt.IsLike)
                    })
                    .ToList();

                return CamelJson(results);
            }

            ViewBag.Query = q ?? "";
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }
}
