using System.Web;
using CookieRestrictions.Configuration;
namespace CookieRestrictions.Context
{
    public class CookieRestrictionsContext
    {
        private static CookieRestrictionsContext instance = null;
        public static CookieRestrictionsContext Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CookieRestrictionsContext();
                }

                return instance;
            }
        }

        public bool HostnameIsValid
        { 
            get
            {
                string hostname = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
                if (string.IsNullOrEmpty(hostname) || (Config.Instance.ValidHostnames.Count > 0 && !Config.Instance.ValidHostnames.Contains(hostname)))
                    return false;

                return true;
            }
        }
    }
}
