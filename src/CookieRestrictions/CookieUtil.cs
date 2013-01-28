using System;
using System.Web;
using CookieRestrictions.Configuration;

namespace CookieRestrictions
{
    public static class CookieUtil
    {
        public static void AllowCookies()
        {
            HttpContext.Current.Response.Cookies.Remove(CookieRestrictionsConfig.Instance.CookiesAllowedKey);
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(CookieRestrictionsConfig.Instance.CookiesAllowedKey, "on") { Expires = DateTime.MaxValue, HttpOnly = true });
        }

        /// <summary>
        /// Disallow cookies on the solution
        /// </summary>
        /// <param name="remember"></param>
        public static void DisallowCookies(bool remember = false)
        {
            HttpCookie httpCookie = HttpContext.Current.Request.Cookies.Get(CookieRestrictionsConfig.Instance.CookiesAllowedKey);
            if (httpCookie == null)
            {
                if (remember == false)
                {
                    return;
                }

                // remember true and no prior cookie
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(CookieRestrictionsConfig.Instance.CookiesAllowedKey, "off") { Expires = DateTime.MaxValue, HttpOnly = true });
                return;
            }
            
            if (httpCookie.Value == "off")
            {
                return;
            }

            if (remember == false)
            {
                httpCookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(httpCookie);
                return;
            }

            httpCookie.Value = "off";
            HttpContext.Current.Response.Cookies.Add(httpCookie);
        }

        /// <summary>
        /// Are cookies Allowed
        /// </summary>
        /// <returns></returns>
        public static bool AllowingCookies()
        {
            if (HttpContext.Current.Request.QueryString[CookieRestrictionsConfig.Instance.CookiesAllowedKey] != null 
                && HttpContext.Current.Request.QueryString[CookieRestrictionsConfig.Instance.CookiesAllowedKey] == "on")
            {
                return true;
            }
            if (HttpContext.Current.Request.QueryString[CookieRestrictionsConfig.Instance.CookiesNotAllowedkey] != null 
                && HttpContext.Current.Request.QueryString[CookieRestrictionsConfig.Instance.CookiesNotAllowedkey] == "on")
            {
                return false;
            }

            HttpCookie httpCookie = HttpContext.Current.Request.Cookies.Get(CookieRestrictionsConfig.Instance.CookiesAllowedKey);
            if (httpCookie == null)
                return false;
            
            return httpCookie.Value == "on" ? true : false;
        }

        /// <summary>
        /// Do we suppress cookies and remember it ?
        /// </summary>
        /// <returns></returns>
        public static bool SuppressingCookiesDisclaimer()
        {

            if (HttpContext.Current.Request.QueryString[CookieRestrictionsConfig.Instance.CookiesAllowedKey] != null
                && HttpContext.Current.Request.QueryString[CookieRestrictionsConfig.Instance.CookiesAllowedKey] == "off")
            {
                if (HttpContext.Current.Request.QueryString[CookieRestrictionsConfig.Instance.RememberKey] == "on")
                {
                    return true;
                }

                return false;
            }

            HttpCookie httpCookie = HttpContext.Current.Request.Cookies.Get(CookieRestrictionsConfig.Instance.CookiesAllowedKey);
            if (httpCookie == null)
            {
                return false;
            }

            return httpCookie.Value == "off" ? true : false; 
        }
    }
}