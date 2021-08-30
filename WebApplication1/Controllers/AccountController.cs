using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext _dbContext;

        public AccountController()
        {
        }

        public AccountController(
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            ApplicationDbContext dbContext)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            DbContext = dbContext;
        }

        public ApplicationDbContext DbContext
        {
            get => _dbContext ?? HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            private set => _dbContext = value;
        }

        public ApplicationSignInManager SignInManager
        {
            get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            private set => _signInManager = value;
        }

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            model.Info = "";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Email != "admin@a.com" || model.Password != "Abc123_!")
            {
                model.Info = "Wrong username or password";
                return View(model);
            }

            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.UserName == model.Email);

            if (user == null)
            {
                DbContext.Users.Add(new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true
                });
                await DbContext.SaveChangesAsync();

                var userInDb = await UserManager.Users
                    .FirstOrDefaultAsync(u => u.UserName == model.Email);

                await UserManager.AddPasswordAsync(userInDb.Id, model.Password);
            }

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, true, false);

            if (result != SignInStatus.Success)
            {
                model.Info = "Error while login";
                return View(model);
            }

            return RedirectToLocal(returnUrl);
        }

        public async Task<ActionResult> Signout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}