using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FC.Core.Interfaces;
using NLog.Interface;
using FC.Core.Extensions;

namespace FC.Core
{
    public class DependencyManager
    {
        public static ICachingService CachingService { get; set; }
    }
}
