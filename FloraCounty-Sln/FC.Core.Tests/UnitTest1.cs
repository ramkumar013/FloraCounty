using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FC.Core.Helper;

namespace FC.Core.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var encryptedString = SecurityHelper.Encrypt(@"mongodb://127.0.0.1/FloraCounty", true);
        }
    }
}
