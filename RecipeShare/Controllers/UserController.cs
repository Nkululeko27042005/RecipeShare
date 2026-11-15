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
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FollowService _followService;

        public UserController()
        {
            _context = new ApplicationDbContext();
            _followService = new FollowService(_context);
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

        // POST: /User/Follow
        [HttpPost]
        public ContentResult Follow(string followeeId)
        {
            var userId = User.Identity.GetUserId();
            var success = _followService.FollowUser(userId, followeeId);
            return CamelJson(new { success });
        }

        // POST: /User/Unfollow
        [HttpPost]
        public ContentResult Unfollow(string followeeId)
        {
            var userId = User.Identity.GetUserId();
            var success = _followService.UnfollowUser(userId, followeeId);
            return CamelJson(new { success });
        }

        // GET: /User/GetRisingUsers (JSON)
        [HttpGet]
        public ContentResult GetRisingUsers(int count = 10)
        {
            var users = _context.Users.Include(u => u.Followers).Include(u => u.Recipes)
                .OrderByDescending(u => u.Followers.Count)
                .Take(count)
                .ToList()
                .Select(u => new
                {
                    u.Id,
                    DisplayName = u.DisplayName ?? u.UserName,
                    u.ProfileImageUrl,
                    FollowerCount = u.Followers.Count,
                    RecipeCount = u.Recipes.Count
                })
                .ToList();

            return CamelJson(users);
        }

        // GET: /User/Rising (Page)
        [HttpGet]
        public ActionResult Rising() => View();

        // GET: /User/GetProfile/{id} (JSON)
        [HttpGet]
        public ContentResult GetProfile(string id)
        {
            var targetId = string.IsNullOrWhiteSpace(id) ? User.Identity.GetUserId() : id;
            var user = _context.Users.Include(u => u.Followers).Include(u => u.Following).Include(u => u.Recipes)
                .FirstOrDefault(u => u.Id == targetId);

            if (user == null) return CamelJson(new { error = "User not found" });

            var currentUserId = User.Identity.GetUserId();

            return CamelJson(new
            {
                user.Id,
                DisplayName = user.DisplayName ?? user.UserName,
                user.Bio,
                user.ProfileImageUrl,
                FollowerCount = user.Followers.Count,
                FollowingCount = user.Following.Count,
                RecipeCount = user.Recipes.Count,
                IsFollowedByCurrentUser = _context.Follows
                    .Any(f => f.FollowerId == currentUserId && f.FolloweeId == targetId)
            });
        }

        // GET: /User/Profile/{id} (Page)
        [HttpGet]
        public new ActionResult Profile(string id)
        {
            ViewBag.UserId = string.IsNullOrWhiteSpace(id) ? User.Identity.GetUserId() : id;
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }
}
