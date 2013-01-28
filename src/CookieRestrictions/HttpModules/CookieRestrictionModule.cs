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
            if(HttpContext.Current == null || HttpContext.Current.Request == null)
            {
                return;
            }
            
            if (!CookieRestrictionsContext.Instance.HostnameIsValid)
            {
                // The javascript should also not be rendered in this case (a workaound could be to set the allowCookies cookie for the current session and allow the script to be called anyway, but its cleaner not to render it at all)
                return;
            }
            
            // Get or Set the cookies allowed cookie
            bool disallowCookiesOn = GetRequestVar(CookieRestrictionsConfig.Instance.CookiesAllowedKey) == "off" 
                || GetRequestVar(CookieRestrictionsConfig.Instance.CookiesNotAllowedkey) == "on"; // Keeped for backward compatibility

            bool allowCookiesOn = GetRequestVar(CookieRestrictionsConfig.Instance.CookiesAllowedKey) == "on";

            HttpCookie allowCookie = HttpContext.Current.Request.Cookies.Get(CookieRestrictionsConfig.Instance.CookiesAllowedKey);
            if (allowCookie == null && allowCookiesOn && !disallowCookiesOn)
            {
                HttpContext.Current.Response.Cookies.Remove(CookieRestrictionsConfig.Instance.CookiesAllowedKey);
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(CookieRestrictionsConfig.Instance.CookiesAllowedKey, "on") { Expires = DateTime.MaxValue, HttpOnly = true });
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
                HttpContext.Current.Response.Cookies.Remove(CookieRestrictionsConfig.Instance.CookiesAllowedKey);
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(CookieRestrictionsConfig.Instance.CookiesAllowedKey, "on") { Expires = DateTime.MaxValue, HttpOnly = true });
   
                return;
            }                        
            

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
                if (!string.IsNullOrEmpty(requestCookieHeader) && requestCookieHeader.Contains(string.Concat(key, "=")))
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
            var rememberRequest = GetRequestVar(CookieRestrictionsConfig.Instance.RememberKey);

            // Allow remembering the not allowcookie
            // if not allready set by querystring
            switch (rememberRequest)
            {
                case "on":
                    HttpContext.Current.Response.Cookies.Remove(CookieRestrictionsConfig.Instance.CookiesAllowedKey);
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie(CookieRestrictionsConfig.Instance.CookiesAllowedKey, "off") { Expires = DateTime.MaxValue, HttpOnly = true});
                    break;
                case "off":
                    HttpContext.Current.Response.Cookies.Remove(CookieRestrictionsConfig.Instance.CookiesAllowedKey);
                    break;
                default :
                    //find state value if cookie was remembered 
                    HttpCookie memory = HttpContext.Current.Request.Cookies.Get(CookieRestrictionsConfig.Instance.CookiesAllowedKey);
                    if (memory != null)
                    {
                        if (memory.Value == "off") // Denied Cookie marked for remembering save state to next load
                        {
                            HttpContext.Current.Response.Cookies.Remove(CookieRestrictionsConfig.Instance.CookiesAllowedKey);
                            HttpContext.Current.Response.Cookies.Add(new HttpCookie(CookieRestrictionsConfig.Instance.CookiesAllowedKey, "off") { Expires = DateTime.MaxValue, HttpOnly = true });
                        }
                    }    
                    break;
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
