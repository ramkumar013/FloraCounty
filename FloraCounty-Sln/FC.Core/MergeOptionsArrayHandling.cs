using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FC.Core.Utilities
{
    //Code taken from this repo - https://github.com/MrAntix/Newtonsoft.Json
    /// <summary>
    /// <para>Array handing options</para>
    /// </summary>
    public enum MergeOptionArrayHandling
    {
        /// <summary>Overwrite the left value with the right</summary>
        Overwrite,

        /// <summary>Concat the right value to the left</summary>
        Concat
    }
}
