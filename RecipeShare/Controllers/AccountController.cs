using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using RecipeShare.Models;
using RecipeShare.Models.Services;
using RecipeShare.Models.ViewModels;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RecipeShare.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        private ApplicationSignInManager _signInManager;
        public ApplicationSignInManager SignInManager
        {
            get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            private set => _signInManager = value;
        }

        // GET: /Account/Login
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login (AJAX - returns JSON)
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Login(string Email, string Password, string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                return Json(new { success = false, error = "Email and password are required." });

            var result = await SignInManager.PasswordSignInAsync(Email, Password, isPersistent: false, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    var redirect = string.IsNullOrWhiteSpace(returnUrl) ? "/Dashboard" : returnUrl;
                    return Json(new { success = true, redirectUrl = redirect });
                default:
                    return Json(new { success = false, error = "Wrong email or password." });
            }
        }

        // GET: /Account/Register
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Register() => View();

        // POST: /Account/Register (AJAX - returns JSON)
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, errors = ModelStateErrors() });

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                DisplayName = model.DisplayName,
                PreferredCategoriesCsv = model.PreferredCategoriesCsv
            };

            var result = await UserManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                return Json(new { success = true, userId = user.Id });
            }

            return Json(new { success = false, errors = result.Errors });
        }

        // POST: /Account/Logout
        [HttpPost]
        public ActionResult Logout()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/Settings
        [HttpGet]
        public ActionResult Settings()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.DisplayName = user.DisplayName;
            ViewBag.Bio = user.Bio;
            ViewBag.ProfileImageUrl = user.ProfileImageUrl;
            ViewBag.PreferredCategoriesCsv = user.PreferredCategoriesCsv;
            return View();
        }

        // POST: /Account/UploadProfilePicture
        [HttpPost]
        public JsonResult UploadProfilePicture(HttpPostedFileBase avatar)
        {
            if (avatar == null || avatar.ContentLength == 0)
            {
                return Json(new { success = false, error = "No image file selected." });
            }

            var extension = Path.GetExtension(avatar.FileName).ToLower();
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!validExtensions.Contains(extension))
            {
                return Json(new { success = false, error = "Supported image formats are JPG, PNG, WEBP, and GIF." });
            }

            var fs = new FileService(Server.MapPath("~"));
            var url = fs.SaveFile(avatar, "avatars");

            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                user.ProfileImageUrl = url;
                UserManager.Update(user);
            }

            return Json(new { success = true, url });
        }

        // POST: /Account/UpdatePreferences (AJAX)
        [HttpPost]
        public async Task<JsonResult> UpdatePreferences(string DisplayName, string Bio, string ProfileImageUrl, string PreferredCategoriesCsv)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
                return Json(new { success = false, error = "User not found." });

            user.DisplayName = DisplayName;
            user.Bio = Bio;
            if (!string.IsNullOrEmpty(ProfileImageUrl))
            {
                user.ProfileImageUrl = ProfileImageUrl;
            }
            user.PreferredCategoriesCsv = PreferredCategoriesCsv;
            var res = await UserManager.UpdateAsync(user);
            return Json(new { success = res.Succeeded });
        }

        private object ModelStateErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
        }
    }
}
