using System.Collections.Generic;
using System.Linq;
using ECMS.Core.Entities;
using System.Web;
using ECMS.Core.Framework;
using System;
using NLog;
using System.Threading.Tasks;
namespace ECMS.Core.Utilities
{
    public class Utility
    {
        public static int GetSiteId(string host)
        {
            var settings = ECMSSettings.CMSSettingsList.Values.ToList();
            foreach (KeyValuePair<int, ECMSSettings> pair in ECMSSettings.CMSSettingsList)
            {
                if (pair.Value.HostAliases.Split(new char[] { ',' }).Any(x => x == host))
                {
                    return pair.Key;
                }
            }
            return -1;
        }
        public static ValidUrl GetValidUrlFromContext(HttpContextBase contextBase_)
        {
            return (contextBase_.Items["validUrl"] != null ? contextBase_.Items["validUrl"] as ValidUrl : null);
        }

        public static string GetClientIP()
        {
            if (HttpContext.Current == null)
            {
                return "255.255.255.255";
            }
            if (HttpContext.Current.Request.ServerVariables["HTTP_TRUE_CLIENT_IP"] != null)
            {
                return HttpContext.Current.Request.ServerVariables["HTTP_TRUE_CLIENT_IP"];
            }
            if (HttpContext.Current.Request.ServerVariables["HTTP_CLIENT_IP"] != null)
            {
                return HttpContext.Current.Request.ServerVariables["HTTP_CLIENT_IP"];
            }

            return HttpContext.Current.Request.UserHostAddress;
        }

        public static ContentViewType CurrentViewType(HttpContextBase context_)
        {
            if (context_ != null && context_.Request != null)
            {
                if (context_.Request.Cookies["ECMS-View-Mode"] != null)
                {
                    return ContentViewType.PREVIEW;
                }
                else if (context_.Request.QueryString["vm"] != null)
                {
                    ContentViewType viewType = (ContentViewType)Enum.Parse(typeof(ContentViewType), context_.Request.QueryString["vm"].ToString(), true);
                    return viewType;
                }
                else
                {
                    return ContentViewType.PUBLISH;
                }
            }
            else
            {
                return ContentViewType.PUBLISH;
            }
        }

       
    }
}
