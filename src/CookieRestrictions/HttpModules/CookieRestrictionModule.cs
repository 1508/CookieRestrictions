﻿using System;
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
            
            HttpCookie allowCookie = HttpContext.Current.Request.Cookies.Get(CookieRestrictionsConfig.Instance.CookiesAllowedKey);            

            // Get or Set the cookies allowed cookie
            bool disallowCookiesOn = GetRequestVar(CookieRestrictionsConfig.Instance.CookiesNotAllowedkey) == "on";
            bool allowCookiesOn = GetRequestVar(CookieRestrictionsConfig.Instance.CookiesAllowedKey) == "on";
            if (allowCookie == null && allowCookiesOn && !disallowCookiesOn)
            {                
                allowCookie = new HttpCookie(CookieRestrictionsConfig.Instance.CookiesAllowedKey, "on");
                allowCookie.Expires = DateTime.MaxValue;
                allowCookie.HttpOnly = false;
                HttpContext.Current.Response.Cookies.Add(allowCookie);
            }
            
            // Return if cookies are allowed
            if (allowCookie != null && allowCookie.Value == "on" && !disallowCookiesOn)
                return;                        

            // Otherwise
            // Clear all cookies currently set
            HttpContext.Current.Response.Cookies.Clear();

            // Clear all existing cookies
            string[] allKeys = HttpContext.Current.Request.Cookies.AllKeys;
            string requestCookieHader = HttpContext.Current.Request.Headers["Cookie"];
            foreach (string key in allKeys)
            {
                // For some reason asp.net adds the asp.net session cookie to the request cookies even if the browser did not send any (this check ensures that the key has actualy been sent as part of the request header)
                if (!string.IsNullOrEmpty(requestCookieHader) && requestCookieHader.Contains(string.Concat(key, "=")))
                {
                    HttpCookie cookie = HttpContext.Current.Request.Cookies.Get(key);
                    HttpCookie tmp = new HttpCookie(cookie.Name, string.Empty);
                    tmp.Expires = DateTime.Now.AddMinutes(-1);
                    tmp.Domain = cookie.Domain;
                    tmp.HttpOnly = cookie.HttpOnly;
                    HttpContext.Current.Response.Cookies.Add(tmp);
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
