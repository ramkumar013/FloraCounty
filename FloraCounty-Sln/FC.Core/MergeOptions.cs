using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FC.Core.Utilities
{
    //Code taken from this repo - https://github.com/MrAntix/Newtonsoft.Json
    /// <summary>
    /// <para>Options for merging JTokens</para>
    /// </summary>
    public class MergeOptions
    {
        /// <summary>
        /// <para>How to handle arrays</para>
        /// </summary>
        public MergeOptionArrayHandling ArrayHandling { get; set; }

        /// <summary>
        /// <para>Default for merge options</para>
        /// </summary>
        public static readonly MergeOptions Default = new MergeOptions
        {
            ArrayHandling = MergeOptionArrayHandling.Overwrite,
            ADD_NONE_EXISTING = false
        };

        public bool ADD_NONE_EXISTING;
    }
}
