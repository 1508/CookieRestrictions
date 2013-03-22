using System.Collections.Generic;

namespace CookieRestrictions.Configuration
{
    public interface ICookieRestrictionsConfig
    {
        string CookiesNotAllowedkey { get; }
        string CookiesAllowedKey { get; }
        string RememberKey { get; }
        bool IsDisabled { get; }
        List<string> ValidHostnames { get; }
        string JavascriptLocation { get; }
    }
}