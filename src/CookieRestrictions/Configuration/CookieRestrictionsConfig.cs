using System.Collections.Generic;
using System.Web.Configuration;

namespace CookieRestrictions.Configuration
{
    public class CookieRestrictionsConfig
    {
        private static CookieRestrictionsConfig instance = null;
        public static CookieRestrictionsConfig Instance 
        {
            get
            {
                if (instance == null)
                {
                    instance = new CookieRestrictionsConfig();
                }

                return instance;
            }
        }

        public string CookiesNotAllowedkey
        {
            get
            {
                return "disallowCookies";
            }
        }

        public string CookiesAllowedKey
        {
            get
            {
                return "allowCookies";
            }
        }

        public string RememberKey
        {
            get
            {
                return "remember";
            }
        }

        private List<string> validHostnames = null;
        public List<string> ValidHostnames
        {
            get
            {
                if (validHostnames == null)
                {
                    validHostnames = new List<string>();
                    string domainString = WebConfigurationManager.AppSettings.Get("CookieRestrictions.ValidHostnames");
                    if (!string.IsNullOrEmpty(domainString))
                    {
                        string[] domains = domainString.Split(',');
                        foreach (string domain in domains)
                        {
                            if (!string.IsNullOrEmpty(domain.Trim()))
                                validHostnames.Add(domain.Trim());
                        }
                    }
                }

                return validHostnames;
            }
        }
    }
}
