using System;
using System.Collections.Generic;
using System.Text;

namespace CookieRestrictions
{
    public static class HtmlUtil
    {
        public static string RenderJavascript()
        {
            if (Configuration.CookieRestrictionsConfig.Instance.IsDisabled)
                return string.Empty;

            return string.Format("<script type=\"text/javascript\" src=\"{0}\"></script>",
                                 Configuration.CookieRestrictionsConfig.Instance.JavascriptLocation);
        }
    }
}
