using System;
using System.Web;
using CookieRestrictions.Configuration;
using CookieRestrictions.Context;

namespace CookieRestrictions.HttpModules
{
    public class CookieRestrictionModule : IHttpModule
    {
        #region IHttpModule Members

        public void Dispose()
        {

        }

        public void Init(HttpApplication context)
        {
            context.EndRequest += new EventHandler(context_EndRequest);
        }

        
        void context_EndRequest(object sender, EventArgs e)
        {
            try
            {
                if (Config.Instance.IsDisabled)
                {
                    return;
                }
                if (HttpContext.Current == null || HttpContext.Current.Request == null)
                {
                    return;
                }

                if (!CookieRestrictionsContext.Instance.HostnameIsValid)
                {
                    // The javascript should also not be rendered in this case (a workaound could be to set the allowCookies cookie for the current session and allow the script to be called anyway, but its cleaner not to render it at all)
                    return;
                }

                // Get or Set the cookies allowed cookie
                bool disallowCookiesOn = GetRequestVar(Config.Instance.CookiesAllowedKey) == "off"
                                         || GetRequestVar(Config.Instance.CookiesNotAllowedkey) == "on";
                // Keeped for backward compatibility

                bool allowCookiesOn = GetRequestVar(Config.Instance.CookiesAllowedKey) == "on";

                HttpCookie allowCookie = HttpContext.Current.Request.Cookies.Get(Config.Instance.CookiesAllowedKey);
                if (allowCookie == null && allowCookiesOn && !disallowCookiesOn)
                {
                    HttpContext.Current.Response.Cookies.Remove(Config.Instance.CookiesAllowedKey);
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie(Config.Instance.CookiesAllowedKey, "on")
                    {
                        Expires = DateTime.MaxValue,
                        HttpOnly = false
                    });
                    return;
                }

                // Return if cookies are allowed
                if (allowCookie != null && allowCookie.Value == "on" && !disallowCookiesOn)
                {
                    // Cookies have been allowed - drop out
                    return;
                }

                if (allowCookie != null && allowCookie.Value != "on" && !disallowCookiesOn)
                {
                    HttpContext.Current.Response.Cookies.Remove(Config.Instance.CookiesAllowedKey);
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie(Config.Instance.CookiesAllowedKey, "on")
                    {
                        Expires = DateTime.MaxValue,
                        HttpOnly = false
                    });

                    return;
                }
                HttpContext.Current.Response.Flush();

                // Otherwise
                // Clear all cookies currently set
                HttpContext.Current.Response.Cookies.Clear();

                // Clear all existing cookies
                string[] allKeys = HttpContext.Current.Request.Cookies.AllKeys;
                string requestCookieHeader = HttpContext.Current.Request.Headers["Cookie"];
                foreach (string key in allKeys)
                {
                    // For some reason asp.net adds the asp.net session cookie to the request cookies even if the browser did not 
                    // send any (this check ensures that the key has actualy been sent as part of the request header)
                    if (!string.IsNullOrEmpty(requestCookieHeader) &&
                        requestCookieHeader.Contains(string.Concat(key, "=")))
                    {
                        HttpCookie cookie = HttpContext.Current.Request.Cookies.Get(key);
                        HttpCookie tmp = new HttpCookie(cookie.Name, string.Empty);
                        tmp.Expires = DateTime.Now.AddMinutes(-1);
                        tmp.Domain = cookie.Domain;
                        tmp.HttpOnly = cookie.HttpOnly;
                        HttpContext.Current.Response.Cookies.Add(tmp);
                    }
                }

                // Do we remember the not allowed state ?
                var rememberRequest = GetRequestVar(Config.Instance.RememberKey);

                // Allow remembering the not allowcookie
                // if not allready set by querystring
                switch (rememberRequest)
                {
                    case "on":
                        HttpContext.Current.Response.Cookies.Remove(Config.Instance.CookiesAllowedKey);
                        HttpContext.Current.Response.Cookies.Add(new HttpCookie(Config.Instance.CookiesAllowedKey, "off")
                        {
                            Expires = DateTime.MaxValue,
                            HttpOnly = false
                        });
                        break;
                    case "off":
                        HttpContext.Current.Response.Cookies.Remove(Config.Instance.CookiesAllowedKey);
                        break;
                    default:
                        //find state value if cookie was remembered 
                        HttpCookie memory = HttpContext.Current.Request.Cookies.Get(Config.Instance.CookiesAllowedKey);
                        if (memory != null)
                        {
                            if (memory.Value == "off") // Denied Cookie marked for remembering save state to next load
                            {
                                HttpContext.Current.Response.Cookies.Remove(Config.Instance.CookiesAllowedKey);
                                HttpContext.Current.Response.Cookies.Add(
                                    new HttpCookie(Config.Instance.CookiesAllowedKey, "off")
                                    {
                                        Expires = DateTime.MaxValue,
                                        HttpOnly = false
                                    });
                            }
                        }
                        break;
                }
            }
            catch (System.Web.HttpException httpException)
            {
                // Ignore requests where a flush has destroyed to ability to change the cookie headers in the final request handler. 
                if (!httpException.Message.Contains("Server cannot modify cookies after HTTP headers have been sent."))
                {
                    // Throw any thing else up the chain
                    throw;
                }
            }
        }

        private string GetRequestVar(string key)
        {
            string val = HttpContext.Current.Request[key];
            if (string.IsNullOrEmpty(val))
                return string.Empty;

            return val;
        }

        #endregion
    }
}
