using System;
using System.Collections.Generic;
using System.Web.Configuration;

namespace CookieRestrictions.Configuration
{
    public class Config
    {
        private static ICookieRestrictionsConfig _instance;

        private static object _lock = new object();

        public static ICookieRestrictionsConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            string configurationType = WebConfigurationManager.AppSettings.Get("CookieRestrictions.ConfigurationType");

                            if (string.IsNullOrEmpty(configurationType))
                            {

                                return _instance = new DefaultConfiguration();
                            }
                            else
                            {
                                Type type = Type.GetType(configurationType);

                                if (type == null)
                                {
                                    return _instance = new DefaultConfiguration();    
                                }

                                var instance = Activator.CreateInstance(type) as ICookieRestrictionsConfig;

                                if (instance == null)
                                {
                                    return _instance = new DefaultConfiguration();
                                }

                                return _instance = instance;
                            }
                        }
                    }    
                }

              
                return _instance;
            }
        }
    }
}
