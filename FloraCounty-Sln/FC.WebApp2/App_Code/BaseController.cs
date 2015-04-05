using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using FC.Core.Helper;
using FC.Core;
namespace FC.WebApp.AppCode
{
    public class BaseController : Controller
    {
        public string GetView()
        {
            return "~/Views/.cshtml";
        }

        public string GetErrorHandlerView()
        {
            if (GetSiteIdFromContext() > -1)
            {
                return "~/Views/" + GetSiteIdFromContext() + "/FC-Error-Handler.cshtml";
            }
            else
            {
                return "~/Views/Shared/FC-Error-Handler.cshtml";
            }
        }
        public int GetSiteIdFromContext()
        {
            return Convert.ToInt32(ControllerContext.HttpContext.Items["SiteId"]);
        }

        public int GetErrorStatusCode()
        {
            if (this.HttpContext != null && ControllerContext.HttpContext.Items["ResponseStatusCode"] != null)
            {
                return Convert.ToInt32(ControllerContext.HttpContext.Items["ResponseStatusCode"]);
            }
            else
            {
                return 500;
            }
        }

        public string GetErrorMessage()
        {
            // why not store tags in resouce file:
            // http://stackoverflow.com/questions/1588790/best-practices-tips-for-storing-html-tags-in-resource-files

            //StringBuilder builder = new StringBuilder(ECMSResources.ResourceManager.GetString(ECMSSettings.HTTPERROR_LOCALE_PREFIX + GetErrorStatusCode().ToString()));
            //builder.Replace("{11}", "<h2>");
            //builder.Replace("{12}", "</h2>");
            //builder.Replace("{21}", "<a href=\"/\">");
            //builder.Replace("{22}", "</a>");
            //return builder.ToString();

            return string.Empty;
        }


        public FCMember FCUser
        {
            get
            {
                FCMember member = DependencyManager.CachingService.Get<FCMember>("LoggedInUser");
                if (member == null && HttpContext.User != null && HttpContext.User.Identity.IsAuthenticated)
                {
                    DefaultUserProfileService service = new DefaultUserProfileService(SecurityHelper.Decrypt(ConfigurationManager.ConnectionStrings["mongodb"].ConnectionString, true));
                    member = service.GetProfileByUserName(HttpContext.User.Identity.Name);
                    if (member != null)
                    {
                        DependencyManager.CachingService.Set<FCMember>("LoggedInUser", member);
                    }
                }
                return member;
            }
        }

        public string GetControllerView(string viewName_)
        {
            return Convert.ToString("~/views/admin/" + this.RouteData.Values["controller"]) + "-" + viewName_ + ".cshtml";
        }
    }
}