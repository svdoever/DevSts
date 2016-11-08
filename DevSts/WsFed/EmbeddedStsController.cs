/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see LICENSE
 */

using System;
using System.IdentityModel.Services;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using System.Xml.Linq;
using DevSts.Assets;

namespace DevSts.WsFed
{
    [AllowAnonymous]
    public class DevStsController : Controller
    {
        private ContentResult Html(string html)
        {
            return Content(html, "text/html");
        }

        public ActionResult Index()
        {
            var message = WSFederationMessage.CreateFromUri(Request.Url);
            var signInMsg = message as SignInRequestMessage;
            if (signInMsg != null)
            {
                var user = GetUser();
                if (user != null)
                {
                    return ProcessSignIn(signInMsg, user);
                }
                else
                {
                    return ShowUserList();
                }
            }

            var signOutMsg = message as SignOutRequestMessage;
            if (signOutMsg != null)
            {
                return ProcessSignOut(signOutMsg);
            }

            return new EmptyResult();
        }

        private ActionResult ShowUserList()
        {
            var html = AssetManager.LoadString(DevStsConstants.SignInFile);

            var users = UserManager.GetAllUserNames();
            var options = "";
            foreach (var user in users)
            {
                options += String.Format("<option value='{0}'>{0}</option>", user);
            }
            html = html.Replace("{options}", options);
            
            var url = Request.Url.PathAndQuery;
            html = html.Replace("{signInUrl}", url);
            
            return Html(html);
        }

        private ClaimsPrincipal GetUser()
        {
            var username = Request.Form["username"];
            if (String.IsNullOrWhiteSpace(username)) return null;

            var claims = UserManager.GetClaimsForUser(username);
            if (claims == null || !claims.Any()) return null;
            
            var id = new ClaimsIdentity(claims);
            return new ClaimsPrincipal(id);
        }

        private ActionResult ProcessSignIn(SignInRequestMessage signInMsg, ClaimsPrincipal user)
        {
            var config = new DevStsTokenServiceConfiguration();
            var sts = config.CreateSecurityTokenService();

            var appPath = Request.ApplicationPath;
            if (!appPath.EndsWith("/")) appPath += "/";

            // when the reply querystringparameter has been specified, don't overrule it. 
            if (String.IsNullOrEmpty(signInMsg.Reply))
                signInMsg.Reply = new Uri(Request.Url, appPath).AbsoluteUri;
            var response = FederatedPassiveSecurityTokenServiceOperations.ProcessSignInRequest(signInMsg, user, sts);

            var body = response.WriteFormPost();
            return Html(body);
        }

        private ActionResult ProcessSignOut(SignOutRequestMessage signOutMsg)
        {
            var appPath = Request.ApplicationPath;
            if (!appPath.EndsWith("/")) appPath += "/";

            var rpUrl = new Uri(Request.Url, appPath);
            var signOutCleanupMsg = new SignOutCleanupRequestMessage(rpUrl);
            var signOutUrl = signOutCleanupMsg.WriteQueryString();

            var html = AssetManager.LoadString(DevStsConstants.SignOutFile);
            html = html.Replace("{redirectUrl}", signOutMsg.Reply);
            html = html.Replace("{signOutUrl}", signOutUrl);

            return Html(html);
        }
        
        public string WsFederationMetadata()
        {            
            var config = new DevStsTokenServiceConfiguration();            
            var uri = new Uri(Request.Url.AbsoluteUri).GetLeftPart(UriPartial.Authority);            
            var claims = UserManager.GetAllUniqueClaimTypes();
            return config.GetFederationMetadata(uri, claims).ToString(SaveOptions.DisableFormatting);            
        }
    }
}
