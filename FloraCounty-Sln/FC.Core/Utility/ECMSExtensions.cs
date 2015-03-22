using ECMS.Core.Entities;
using ECMS.Core.Framework;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ECMS.Core.Extensions
{
    public static class ECMSExtensions
    {
        public static void LoadFromJObject(this ContentItemHead contentItemHead_, JObject object_)
        {
            foreach (JToken token in object_.Children())
            {
                if (token is JProperty)
                {
                    switch ((token as JProperty).Name.ToString())
                    {
                        case "Title":
                            contentItemHead_.Title = (token as JProperty).Value.ToString();
                            break;
                        case "KeyWords":
                            contentItemHead_.KeyWords = (token as JProperty).Value.ToString();
                            break;
                        case "Description":
                            contentItemHead_.Description = (token as JProperty).Value.ToString();
                            break;
                        case "PageMetaTags":
                            contentItemHead_.PageMetaTags = (token as JProperty).Value.ToString();
                            break;
                    }
                }
            }
        }

        //public static void Log(this LoggerAdapter loggerAdapter_, LogEventInfo info, HttpContextBase context_)
        //{
        //    if (context_ != null && context_.Items["LoggerName"] != null && ECMS.Core.Utilities.Utility.CurrentViewType(context_) == ContentViewType.PREVIEW)
        //    {
        //        string _loggerName = ((System.Guid)context_.Items["LoggerName"]).ToString();
        //        Logger logger = LogManager.GetLogger(_loggerName);
        //        if (logger != null)
        //        {
        //            logger.Debug(info);
        //        }
        //    }
        //    DependencyManager.Logger.Log(info);
        //}

        public static string ToTitle(this string str_)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo; // TODO : Remove hardcoding./
            return textInfo.ToTitleCase(str_);
        }
    }
}
