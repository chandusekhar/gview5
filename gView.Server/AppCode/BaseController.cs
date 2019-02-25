﻿using gView.Core.Framework.Exceptions;
using gView.Framework.system;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            this.ActionStartTime = DateTime.UtcNow;

            base.OnActionExecuting(context);
        }

        protected DateTime? ActionStartTime = null;

        #region Security

        const string AuthCookieName = "gview5-auth-token";

        protected AuthToken GetAuthToken()
        {
            return LoginAuthToken(this.Request);
        }

        protected void SetAuthCookie(AuthToken authToken)
        {
            var cookieOptions = new CookieOptions()
            {
                Expires = null,
                Secure = true,
                HttpOnly = true
            };

            this.Response.Cookies.Append(AuthCookieName, authToken.ToString(), cookieOptions);
        }

        protected void RemoveAuthCookie()
        {
            this.Response.Cookies.Delete(AuthCookieName);
        }

        static public AuthToken LoginAuthToken(HttpRequest request)
        {
            AuthToken authToken = null;

            try
            {
                #region From Token

                string token = request.Query["token"];
                if(String.IsNullOrWhiteSpace(token) && request.HasFormContentType)
                {
                    try
                    {
                        token = request.Form["token"];
                    }
                    catch { }
                }
                if (!String.IsNullOrEmpty(token))
                {
                    return authToken = AuthToken.FromString(token);
                }

                #endregion

                #region From Cookie

                string cookie = request.Cookies[AuthCookieName];
                if (!String.IsNullOrWhiteSpace(cookie))
                {
                    return authToken = AuthToken.FromString(cookie);
                }

                #endregion

                return authToken = new AuthToken()
                {
                    Username = String.Empty
                };
            }
            finally
            {
                if (authToken.IsExpired)
                    throw new InvalidTokenException();
            }
        }

        static public string LoginUsername(HttpRequest request)
        {
            try
            {
                return LoginAuthToken(request).Username;
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        #endregion

        async virtual protected Task<IActionResult> SecureMethodHandler(Func<Identity, Task<IActionResult>> func, Func<Exception, IActionResult> onException = null)
        {
            try
            {
                var identity = new Identity(this.GetAuthToken().Username);

                return await func(identity);
            }
            catch (Exception ex)
            {
                if (onException != null)
                {
                    return onException(ex);
                }
                else
                {
                    throw ex;
                }
            }
        }
    }
}
