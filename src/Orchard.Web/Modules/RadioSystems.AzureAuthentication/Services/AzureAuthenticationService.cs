using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Mvc;
using System.Security.Claims;
using System.Threading;
using Orchard.Users.ViewModels;
using Orchard.Roles.Models;
using Orchard.Data;
using Orchard.Roles.Services;
using Orchard;

namespace RadioSystems.AzureAuthentication.Services
{
   public class AzureAuthenticationService : IAuthenticationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMembershipService _membershipService;
        private readonly IRoleService _roleService;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;
        public AzureAuthenticationService(IHttpContextAccessor httpContextAccessor, IMembershipService membershipService, IRoleService RoleService, IRepository<UserRolesPartRecord> UserRolesRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _membershipService = membershipService;
            _roleService = RoleService;
            _userRolesRepository = UserRolesRepository;
        }

        public void SignIn(IUser user, bool createPersistentCookie)
        {
        }

        public void SignOut()
        {
        }

        public void SetAuthenticatedUserForRequest(IUser user) { }

        public IUser GetAuthenticatedUser()
        {
            var azureUser = _httpContextAccessor.Current().GetOwinContext().Authentication.User;

            if (!azureUser.Identity.IsAuthenticated)
            {
                return null;
            }

            /* string userName = String.Empty;

             if(!String.IsNullOrWhiteSpace(azureUser.Identity.Name.Trim()))
                 userName = azureUser.Identity.Name.Trim();
             else
             {
                 var claims = azureUser.Claims;
                 userName = azureUser.Claims.FirstOrDefault(c => c.Type == "preferred_username").Value;
             }

             var localUser = _membershipService.GetUser(userName);

             return localUser;*/
            var claims = azureUser.Claims;
            string userName = azureUser.Claims.FirstOrDefault(c => c.Type == "name").Value;
            // bool isnewUser = Convert.ToBoolean(azureUser.Claims.FirstOrDefault(c => c.Type == "newUser").Value);
            var localUser = _membershipService.GetUser(userName);
            if (localUser == null)
            {
                UserCreateViewModel createModel = new UserCreateViewModel();
                createModel.UserName = userName;
                createModel.Email = azureUser.Claims.FirstOrDefault(c => c.Type == "emails").Value;
                createModel.Password = "Password@123";
                createModel.ConfirmPassword = "Password@123";
                var user = _membershipService.CreateUser(new CreateUserParams(createModel.UserName, createModel.Password, createModel.Email, null, null, true));


               var role = _roleService.GetRoleByName("Administrator");
                if (role != null)
                {
                    _userRolesRepository.Create(
                        new UserRolesPartRecord
                        {
                            UserId = user.Id,
                            Role = role
                        });
                }

            }
            return localUser;
        }
    }
}