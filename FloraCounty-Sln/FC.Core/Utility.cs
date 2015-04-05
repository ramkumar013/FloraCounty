using System.Collections.Generic;
using System.Linq;
using FC.Core.Entities;
using System.Web;
using System;
using NLog;
using System.Threading.Tasks;
namespace FC.Core.Utilities
{
    public class Utility
    {
        public static int GetSiteId(string host)
        {
            var settings = FCSettings.FCSettingsList.Values.ToList();
            foreach (KeyValuePair<int, FCSettings> pair in FCSettings.FCSettingsList)
            {
                if (pair.Value.HostAliases.Split(new char[] { ',' }).Any(x => x == host))
                {
                    return pair.Key;
                }
            }
            return -1;
        }


        public static string GetClientIP(HttpContextBase contextBase_)
        {
            if (contextBase_ == null)
            {
                return "255.255.255.255";
            }
            if (contextBase_.Request.ServerVariables["HTTP_TRUE_CLIENT_IP"] != null)
            {
                return HttpContext.Current.Request.ServerVariables["HTTP_TRUE_CLIENT_IP"];
            }
            if (contextBase_.Request.ServerVariables["HTTP_CLIENT_IP"] != null)
            {
                return contextBase_.Request.ServerVariables["HTTP_CLIENT_IP"];
            }

            return contextBase_.Request.UserHostAddress;
        }
    }
}
