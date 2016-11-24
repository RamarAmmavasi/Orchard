﻿using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OpenIdConnect;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Owin;
using Orchard.Settings;
using Owin;
using RadioSystems.AzureAuthentication.Models;
using RadioSystems.AzureAuthentication.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;

namespace RadioSystems.AzureAuthentication
{
    public class OwinMiddlewares : IOwinMiddlewareProvider
    {
        public ILogger Logger { get; set; }

        public static string _azureClientId;
        public static string _azureTenant;
        public static string _azureADInstance;
        public static string _logoutRedirectUri;
        public static string _azureAppName;
        public static bool _sslEnabled;
        public static bool _azureWebSiteProtectionEnabled;
        public static string _signupPolicies;
        public static string _signinPolicies;
        public static string _profilePolicies;

        public OwinMiddlewares(ISiteService siteService)
        {
            Logger = NullLogger.Instance;

            var site = siteService.GetSiteSettings();
            var azureSettings = site.As<AzureSettingsPart>();

            _azureClientId = ((azureSettings.ClientId == null) || (azureSettings.ClientId == string.Empty)) ?
                "[example: 82692da5-a86f-44c9-9d53-2f88d52b478b]" : azureSettings.ClientId;

            _azureTenant = ((azureSettings.Tenant == null) || (azureSettings.Tenant == string.Empty)) ?
                "faketenant.com" : azureSettings.Tenant;

            _azureADInstance = ((azureSettings.ADInstance == null) || (azureSettings.ADInstance == string.Empty)) ?
                "https://login.microsoft.com/{0}" : azureSettings.ADInstance;

            _logoutRedirectUri = ((azureSettings.LogoutRedirectUri == null) || (azureSettings.LogoutRedirectUri == string.Empty)) ?
                site.BaseUrl : azureSettings.LogoutRedirectUri;

            _azureAppName = ((azureSettings.AppName == null) || (azureSettings.AppName == string.Empty)) ?
                "[example: MyAppName]" : azureSettings.AppName;

            _sslEnabled = azureSettings.SSLEnabled;

            _azureWebSiteProtectionEnabled = azureSettings.AzureWebSiteProtectionEnabled;
            _signupPolicies = azureSettings.SignupPolicies;
            _signinPolicies = azureSettings.SigninPolicies;
            _profilePolicies = azureSettings.ProfilePolicies;
        }
        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares()
        {
            var middlewares = new List<OwinMiddlewareRegistration>();

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            /*var openIdOptions = new OpenIdConnectAuthenticationOptions
            {
                ClientId = _azureClientId,
                Authority = string.Format(CultureInfo.InvariantCulture, _azureADInstance, _azureTenant),
                PostLogoutRedirectUri = _logoutRedirectUri,
                Notifications = new OpenIdConnectAuthenticationNotifications()
            };*/

            var cookieOptions = new CookieAuthenticationOptions();

            var bearerAuthOptions = new WindowsAzureActiveDirectoryBearerAuthenticationOptions
            {
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudience = string.Format(_sslEnabled ? "https://{0}/{1}" : "http://{0}/{1}", _azureTenant, _azureAppName)
                }
            };

            /*if (_azureWebSiteProtectionEnabled)
            {
                middlewares.Add(new OwinMiddlewareRegistration
                {
                    Priority = "9",
                    Configure = app => { app.SetDataProtectionProvider(new MachineKeyProtectionProvider()); }
                });
            }*/

            middlewares.Add(new OwinMiddlewareRegistration
            {
                Priority = "10",
                Configure = app => {
                    app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

                    app.UseCookieAuthentication(cookieOptions);

                    //app.UseOpenIdConnectAuthentication(openIdOptions);
                    if(!String.IsNullOrWhiteSpace(_signupPolicies))
                    {
                        app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
                        {
                            // For each policy, give OWIN the policy-specific metadata address, and
                            // set the authentication type to the id of the policy
                            MetadataAddress = String.Format(_azureADInstance, _azureTenant, _signupPolicies),
                            AuthenticationType = _signupPolicies,

                            // These are standard OpenID Connect parameters, with values pulled from web.config
                            ClientId = _azureClientId,
                            RedirectUri = _logoutRedirectUri,
                            PostLogoutRedirectUri = _logoutRedirectUri,
                            Scope = "openid",
                            ResponseType = "id_token",
                            ProtocolValidator = new Microsoft.IdentityModel.Protocols.OpenIdConnectProtocolValidator()
                            {
                                RequireNonce = false
                            },
                            // This piece is optional - it is used for displaying the user's name in the navigation bar.
                            TokenValidationParameters = new TokenValidationParameters
                            {
                                NameClaimType = "name",
                            },
                        });
                    }
                    if (!String.IsNullOrWhiteSpace(_signinPolicies))
                    {
                        app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
                        {
                            // For each policy, give OWIN the policy-specific metadata address, and
                            // set the authentication type to the id of the policy
                            MetadataAddress = String.Format(_azureADInstance, _azureTenant, _signinPolicies),
                            AuthenticationType = _signinPolicies,

                            // These are standard OpenID Connect parameters, with values pulled from web.config
                            ClientId = _azureClientId,
                            RedirectUri = _logoutRedirectUri,
                            PostLogoutRedirectUri = _logoutRedirectUri,
                            Scope = "openid",
                            ResponseType = "id_token",
                            ProtocolValidator = new Microsoft.IdentityModel.Protocols.OpenIdConnectProtocolValidator()
                            {
                                RequireNonce = false
                            },
                            // This piece is optional - it is used for displaying the user's name in the navigation bar.
                            TokenValidationParameters = new TokenValidationParameters
                            {
                                NameClaimType = "name",
                            },
                        });
                    }
                    if (!String.IsNullOrWhiteSpace(_profilePolicies))
                    {
                        app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
                        {
                            // For each policy, give OWIN the policy-specific metadata address, and
                            // set the authentication type to the id of the policy
                            MetadataAddress = String.Format(_azureADInstance, _azureTenant, _profilePolicies),
                            AuthenticationType = _profilePolicies,

                            // These are standard OpenID Connect parameters, with values pulled from web.config
                            ClientId = _azureClientId,
                            RedirectUri = _logoutRedirectUri,
                            PostLogoutRedirectUri = _logoutRedirectUri,
                            Scope = "openid",
                            ResponseType = "id_token",
                            ProtocolValidator = new Microsoft.IdentityModel.Protocols.OpenIdConnectProtocolValidator()
                            {
                                RequireNonce = false
                            },
                            // This piece is optional - it is used for displaying the user's name in the navigation bar.
                            TokenValidationParameters = new TokenValidationParameters
                            {
                                NameClaimType = "name",
                            },
                        });
                    }

                    //This is throwing an XML DTD is prohibited error?
                    //app.UseWindowsAzureActiveDirectoryBearerAuthentication(bearerAuthOptions);
                }
            });

            return middlewares;
        }
    }
}