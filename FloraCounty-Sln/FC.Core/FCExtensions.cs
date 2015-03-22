using FC.Core.Entities;
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

namespace FC.Core.Extensions
{
    public static class FCExtensions
    {
        

        public static string ToTitle(this string str_)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo; // TODO : Remove hardcoding./
            return textInfo.ToTitleCase(str_);
        }
    }
}
