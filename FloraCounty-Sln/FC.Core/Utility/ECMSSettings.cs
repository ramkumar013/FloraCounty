using ECMS.Core.Utilities;
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

namespace ECMS.Core
{
    public class ECMSSettings
    {
        #region Constants
        public const string DEFAULT_LOGGER = "default";
        public const string HTTPERROR_LOCALE_PREFIX = "HTTPError";
        #endregion

        #region Static Properties
        public static Dictionary<int, ECMSSettings> CMSSettingsList { get; set; }
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
        #endregion

        #region Static Constructor
        static ECMSSettings()
        {
            //TODO : Shall be called at app_Start. Will give problems at the time of unit testing.
            BeginLoadAppSettings();
        }
        #endregion

        #region Private Static Methods
        public static void BeginLoadAppSettings()
        {
            //TODO: expose this method on http.
            CMSSettingsList = new Dictionary<int, ECMSSettings>();
            string[] dirinfo = Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory + "\\configs");
            foreach (string dir in dirinfo)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                CMSSettingsList.Add(Convert.ToInt32(dirInfo.Name), LoadAppSettings(dirInfo));
            }
        }

        private static ECMSSettings LoadAppSettings(DirectoryInfo dirInfo_)
        {
            ECMSSettings setting = new ECMSSettings();
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
                    setting.InitiateXMLSiteMapGenerator();
                    setting.LoadDomainData();
                }
            }
            catch (Exception ex)
            {
                LogEventInfo info = new LogEventInfo(LogLevel.Error, ECMSSettings.DEFAULT_LOGGER, string.Format("Error while reading config file at : {0}", dirInfo_.FullName) + "\r\n" + ex.ToString());
                DependencyManager.Logger.Log(info);
            }
            return setting;
        }

        private void LoadDomainData()
        {
            DomainData = new DataSet();
            DomainData.Tables.Add(LoadCityLocationsTable());
        }

        private DataTable LoadCityLocationsTable()
        {
            DataTable table = new DataTable();
            table.TableName = "CityLocations";
            table.Columns.Add("CityName");
            table.Columns.Add("CityCode");
            table.Columns.Add("StateCountyCode");
            table.Columns.Add("CountryCode");
            table.Columns.Add("Latitude");
            table.Columns.Add("Longitude");
            table.Columns.Add("RegionCode");
            table.Rows.Add(new object[] { "New York City", "NYC", "NY", "US", "-1", "-1" });
            return table;
        }
        #endregion

        /// <summary>
        /// Site id must be injected either from app_begin_request or action filter to make ECMSSettings of current portal available using this static property.
        /// </summary>
        public static ECMSSettings Current
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

        public static ECMSSettings GetCurrentBySiteId(int siteId_)
        {
            return CMSSettingsList[siteId_];
        }

        #region XMLSitemap Related Methods
        /// <summary>
        /// Thsi method will write xmlsitemap as per standards define here : http://sitemaps.org/protocol.php
        /// </summary>
        /// <param name="websiteRoot_"></param>
        /// <param name="filePath_"></param>
        public void GenerateSitemapXml(object stateInfo_)
        {
            ECMSSettings settings = stateInfo_ as ECMSSettings;
            try
            {
                XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
                XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
                XNamespace schemaLocation = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

                XElement root = new XElement(xmlns + "urlset",
                  new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                  new XAttribute(xsi + "schemaLocation", schemaLocation));

                var siteMapIndexRoot = new XElement(xmlns + "sitemapindex",
                    new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                    new XAttribute(xsi + "schemaLocation", schemaLocation));

                var validUrls = DependencyManager.URLRepository.GetAll(settings.SiteId, false);
                int urlCounter = 1;
                foreach (var validUrl in validUrls)
                {
                    root.Add(
                        new XElement(xmlns + "url",
                            new XElement(xmlns + "loc", new XCData(settings.PortalHostName + validUrl.FriendlyUrl.Trim().ToLower().Replace(' ', '-'))),
                        new XElement(xmlns + "changefreq", validUrl.ChangeFrequency),
                        new XElement(xmlns + "priority", validUrl.SitemapPriority),
                            new XElement(xmlns + "lastmod", validUrl.LastModified.ToString("yyyy-MM-dd"))
                            ));

                    urlCounter++;
                    if (urlCounter % 45000 == 0)
                    {
                        WriteSitemapFile(AppBasePath + "\\" + settings.SiteId + "\\sitemap" + (urlCounter / 45000) + ".xml.gz", root.ToString(), true);
                        CreateSitemapIndexDoc(siteMapIndexRoot, "/xmlsitemap/" + (urlCounter / 45000));

                        root = new XElement(xmlns + "urlset",
                            new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                            new XAttribute(xsi + "schemaLocation", schemaLocation));
                    }
                }

                if (urlCounter > 0)
                {
                    CreateSitemapIndexDoc(siteMapIndexRoot, "http://" + settings.PortalHostName + "/xmlsitemap/" + (urlCounter / 45000));
                    WriteSitemapFile(AppBasePath + "\\static\\" + settings.SiteId + "\\sitemap-" + (urlCounter / 45000) + ".xml.gz", root.ToString(), true);
                    WriteSitemapFile(AppBasePath + "\\static\\" + settings.SiteId + "\\sitemapindex.xml", siteMapIndexRoot.ToString(), false);
                }
            }
            catch (Exception ex)
            {
                LogEventInfo info = new LogEventInfo(LogLevel.Error, ECMSSettings.DEFAULT_LOGGER, ex.ToString());
                DependencyManager.Logger.Log(info);
            }
        }

        private void WriteSitemapFile(string filepath_, string content_, bool compress_)
        {
            byte[] contentBytes = Encoding.ASCII.GetBytes(content_);
            using (FileStream fileStream = File.Create(filepath_))
            {
                if (compress_)
                {
                    using (GZipStream stream = new GZipStream(fileStream, CompressionMode.Compress))
                    {
                        stream.Write(contentBytes, 0, contentBytes.Length);
                    }
                }
                else
                {
                    fileStream.Write(contentBytes, 0, contentBytes.Length);
                }
            }
        }
        private void CreateSitemapIndexDoc(XElement siteMapIndexRoot, string indexFileURL_)
        {
            string[] toBeReplaced = { "&" };
            string[] byWhichReplaced = { "&amp;" };
            if (!siteMapIndexRoot.ToString().Contains(indexFileURL_.Replace(toBeReplaced[0], byWhichReplaced[0])))
            {
                XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
                var sitemap = new XElement(xmlns + "sitemap", new XElement(xmlns + "loc", indexFileURL_), new XElement(xmlns + "lastmod", DateTime.Now.ToString("yyyy-MM-dd")));
                siteMapIndexRoot.Add(sitemap);
            }
        }

        private void InitiateXMLSiteMapGenerator()
        {
            System.Threading.TimerCallback tcb = GenerateSitemapXml;
            System.Threading.Timer generateXMLSiteMapTimer = new System.Threading.Timer(tcb, this, 5 * 60 * 1000, XmlSitemapRefreshFrequency * 1000 * 60);
        }
        #endregion
    }
}
