using FC.Core.Utilities;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Xml.Linq;

namespace FC.Core
{
    public class FCSettings
    {
        #region Constants
        public const string DEFAULT_LOGGER = "default";
        public const string HTTPERROR_LOCALE_PREFIX = "HTTPError";
        #endregion

        #region Static Properties
        public static Dictionary<int, FCSettings> CMSSettingsList { get; set; }
        #endregion

        #region Instance Properties
        public string CDNPath { get; set; }
        public string AppBasePath { get; set; }
        public string HostAliases { get; set; }
        public int SiteId { get; set; }
        public string PortalHostName { get; set; }
        public int XmlSitemapRefreshFrequency { get; set; }
        public string DefaultURLRewriteAction = "/Template/Compose";
        public DataSet DomainData { get; set; }
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region Static Constructor
        static FCSettings()
        {
            //TODO : Shall be called at app_Start. Will give problems at the time of unit testing.
            BeginLoadAppSettings();
        }
        #endregion

        #region Private Static Methods
        public static void BeginLoadAppSettings()
        {
            //TODO: expose this method on http.
            CMSSettingsList = new Dictionary<int, FCSettings>();
            string[] dirinfo = Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory + "\\configs");
            foreach (string dir in dirinfo)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                CMSSettingsList.Add(Convert.ToInt32(dirInfo.Name), LoadAppSettings(dirInfo));
            }
        }

        private static FCSettings LoadAppSettings(DirectoryInfo dirInfo_)
        {
            FCSettings setting = new FCSettings();
            try
            {
                using (DataSet ds = new DataSet("Configuration"))
                {
                    ds.ReadXml(dirInfo_.FullName + "\\site.config");
                    setting.CDNPath = Convert.ToString(ds.Tables["configuration"].Rows[0]["CDNPath"]) + dirInfo_.Name;
                    setting.AppBasePath = AppDomain.CurrentDomain.BaseDirectory;
                    setting.HostAliases = Convert.ToString(ds.Tables["configuration"].Rows[0]["HttpAliases"]);
                    setting.PortalHostName = Convert.ToString(ds.Tables["configuration"].Rows[0]["PortalHostName"]);
                    setting.XmlSitemapRefreshFrequency = Convert.ToInt32(ds.Tables["configuration"].Rows[0]["XmlSitemapRefreshFrequency"]);
                    setting.SiteId = Convert.ToInt32(dirInfo_.Name);
                }
            }
            catch (Exception ex)
            {
                LogEventInfo info = new LogEventInfo(LogLevel.Error, FCSettings.DEFAULT_LOGGER, string.Format("Error while reading config file at : {0}", dirInfo_.FullName) + "\r\n" + ex.ToString());
                _logger.Log(info);
            }
            return setting;
        }

        #endregion

        /// <summary>
        /// Site id must be injected either from app_begin_request or action filter to make ECMSSettings of current portal available using this static property.
        /// </summary>
        public static FCSettings Current
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Items["SiteId"] != null)
                {
                    return CMSSettingsList[Convert.ToInt32(HttpContext.Current.Items["SiteId"])];
                }
                else
                {
                    return null;
                }
            }
        }

        public static FCSettings GetCurrentBySiteId(int siteId_)
        {
            return CMSSettingsList[siteId_];
        }
        
    }
}
