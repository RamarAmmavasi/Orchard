using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Orchard.Environment.Extensions;
using Orchard.Security;
using Orchard.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RadioSystems.AzureAuthentication.Models;
using Orchard.Mvc;
using Orchard;
using Orchard.Localization;

namespace RadioSystems.AzureAuthentication.Controllers
{
    [Themed]
    [OrchardSuppressDependency("Orchard.Users.Controllers.AccountController")]
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrchardServices _orchardServices;
        public AccountController(IAuthenticationService authenticationService, IOrchardServices orchardServices, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _authenticationService = authenticationService;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public void LogOn()
        {
            if (Request.IsAuthenticated)
            {
                return;
            }

            var redirectUri = Url.Content("~/");

            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = redirectUri }, OwinMiddlewares._signinPolicies);
        }

        public void LogOff()
        {
            /*HttpContext.GetOwinContext().Authentication.SignOut(
              OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
              */
              //OpenID Connect sign-out request.
              if (Request.IsAuthenticated)
            {
                IEnumerable<AuthenticationDescription> authTypes = HttpContext.GetOwinContext().Authentication.GetAuthenticationTypes();
                HttpContext.GetOwinContext().Authentication.SignOut(authTypes.Select(t => t.AuthenticationType).ToArray());
                Request.GetOwinContext().Authentication.GetAuthenticationTypes();
            }
        }
        public void SignUp()
        {
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties() { RedirectUri = "/" }, OwinMiddlewares._signupPolicies);
            }
        }
        public new ActionResult Profile()
         {
            UserProfile UserProfile = new UserProfile();
            var azureUser = _httpContextAccessor.Current().GetOwinContext().Authentication.User;
            ViewBag.DisplayName = azureUser.Claims.FirstOrDefault(c => c.Type == "name").Value;
            UserProfile.Name = azureUser.Claims.FirstOrDefault(x => x.Type == "name").Value;
            UserProfile.Email = azureUser.Claims.FirstOrDefault(x => x.Type == "emails").Value;
            UserProfile.City = azureUser.Claims.FirstOrDefault(x => x.Type == "city").Value;
            UserProfile.State = azureUser.Claims.FirstOrDefault(x => x.Type == "state").Value;
            UserProfile.Country = azureUser.Claims.FirstOrDefault(x => x.Type == "country").Value;
            UserProfile.Postalcode = azureUser.Claims.FirstOrDefault(x => x.Type == "postalCode").Value;
            var shape = _orchardServices.New.Profile(UserProfile).Title(T("Profile").Text);
            return new ShapeResult(this, shape);
            //return View(UserProfile);
            /* if (Request.IsAuthenticated)
             {
                 HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties() { RedirectUri = "/" }, OwinMiddlewares._profilePolicies);
             }*/
        }

        public ActionResult AccessDenied()
        {
            return View();
        }
    }
}