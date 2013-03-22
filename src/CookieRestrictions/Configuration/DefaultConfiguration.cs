using System.Collections.Generic;
using System.Web.Configuration;

namespace CookieRestrictions.Configuration
{
    public class DefaultConfiguration : ICookieRestrictionsConfig
    {
        private List<string> validHostnames = null;
        private string _javascriptLocation;
        private bool? _disabled;

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

        public bool IsDisabled
        {
            get
            {
                if (this._disabled.HasValue)
                {
                    return this._disabled.Value;
                }

                string sDisabled = WebConfigurationManager.AppSettings.Get("CookieRestrictions.Disabled");

                bool disabled;
                if (!bool.TryParse(sDisabled, out disabled))
                {
                    disabled = false;
                }

                this._disabled = disabled;

                return this._disabled.Value;
            }
        }

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

        public string JavascriptLocation
        {
            get
            {
                if (string.IsNullOrEmpty(this._javascriptLocation))
                {
                    string location = WebConfigurationManager.AppSettings.Get("CookieRestrictions.JavascriptLocation");

                    if (string.IsNullOrEmpty(location))
                    {
                        location = "/resources/js/CookieRestrictions.js";
                    }

                    this._javascriptLocation = location;
                }

                return this._javascriptLocation;
            }
        }
    }
}