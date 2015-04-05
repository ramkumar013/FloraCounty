using FC.Core;
using FC.WebApp2.Controllers;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace FC.WebApp2
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private string _loggerName = string.Empty;
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
        }
        void Application_Error(object sender, EventArgs e)
        {
            Logger logger = null;
            try
            {
                var httpContext = ((MvcApplication)sender).Context;
                var currentController = string.Empty;
                var currentAction = string.Empty;
                var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
                logger = LogManager.GetLogger(_loggerName);

                if (currentRouteData != null)
                {
                    if (currentRouteData.Values["controller"] != null && !String.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
                    {
                        currentController = currentRouteData.Values["controller"].ToString();
                    }

                    if (currentRouteData.Values["action"] != null && !String.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
                    {
                        currentAction = currentRouteData.Values["action"].ToString();
                    }
                    LogEventInfo info = new LogEventInfo(LogLevel.Error, FCSettings.DEFAULT_LOGGER, currentController + "--" + currentAction);
                    logger.Log(info);
                }

                var ex = Server.GetLastError();
                var controller = new ErrorController();
                var routeData = new RouteData();
                var action = "Index";
                if (ex != null)
                {
                    LogEventInfo info = new LogEventInfo(LogLevel.Error, FCSettings.DEFAULT_LOGGER, ex.ToString());
                    logger.Log(info);
                }
                if (ex is HttpException)
                {
                    var httpEx = ex as HttpException;

                    switch (httpEx.GetHttpCode())
                    {
                        case 404:
                            action = "NotFound";
                            break;
                        default:
                            action = "Index";
                            break;
                    }
                }

                httpContext.ClearError();
                httpContext.Response.Clear();
                httpContext.Response.StatusCode = ex is HttpException ? ((HttpException)ex).GetHttpCode() : 500;
                httpContext.Response.TrySkipIisCustomErrors = true;

                routeData.Values["controller"] = "Error";
                routeData.Values["action"] = action;

                controller.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);

                ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
            }
            catch (Exception ex)
            {
                LogEventInfo info = new LogEventInfo(LogLevel.Error, FCSettings.DEFAULT_LOGGER, ex.ToString());
                logger.Log(info);
            }
        }
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Directory.GetParent(Path.GetDirectoryName(path)).FullName;
            }
        }


        void Application_End()
        {
            try
            {
                HttpRuntime runtime = (HttpRuntime)typeof(System.Web.HttpRuntime).InvokeMember("_theRuntime",
                                                                                           BindingFlags.NonPublic
                                                                                           | BindingFlags.Static
                                                                                           | BindingFlags.GetField,
                                                                                           null,
                                                                                           null,
                                                                                           null);

                if (runtime == null)
                    return;

                string shutDownMessage = (string)runtime.GetType().InvokeMember("_shutDownMessage",
                                                                                 BindingFlags.NonPublic
                                                                                 | BindingFlags.Instance
                                                                                 | BindingFlags.GetField,
                                                                                 null,
                                                                                 runtime,
                                                                                 null);

                string shutDownStack = (string)runtime.GetType().InvokeMember("_shutDownStack",
                                                                               BindingFlags.NonPublic
                                                                               | BindingFlags.Instance
                                                                               | BindingFlags.GetField,
                                                                               null,
                                                                               runtime,
                                                                               null);

                File.AppendAllText(AssemblyDirectory + "\\Logs\\FCAppLogs.txt", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss:ffff") + " : shutDownMessage--" + shutDownMessage + " : shutDownStack--" + shutDownStack);
            }
            catch (Exception ex)
            {
                File.AppendAllText(AssemblyDirectory + "\\Logs\\FCAppLogs.txt", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss:ffff") + " : Error while shutdown " + ex.ToString());
            }
        }
    }
}