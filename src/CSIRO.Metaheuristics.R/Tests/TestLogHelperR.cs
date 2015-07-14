using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSIRO.Metaheuristics.Logging;
using CSIRO.Metaheuristics.Tests;
using NUnit.Framework;
using RDotNet;

namespace CSIRO.Metaheuristics.R.Tests
{
    [TestFixture]
    public class TestLogHelperR
    {
        [Test]
        public void TestConversionToDataFrame()
        {
            InMemoryLogger logger;
            Dictionary<string, string> strMsg;
            Dictionary<string, string> popTags;
            LoggerMhTestHelper.CreateTestLogContent(out logger, out strMsg, out popTags);

            var names = new[]{
                "Category"
                ,"Message"
                ,"X0"
                ,"X1"
                ,"X2"
                ,"X0_s"
                ,"X1_s"
                ,"X2_s"};
            // R does not like column names that start with a numeric char, and prepends X
            var df = logger.ToDataFrame();
            var e = REngine.GetInstance();
            e.SetSymbol("TestConversionToDataFrame_df", df);
            e.SetSymbol("TestConversionToDataFrame_expected_colnames", e.CreateCharacterVector(names));
            var b = e.Evaluate("setequal( names(TestConversionToDataFrame_df), TestConversionToDataFrame_expected_colnames )");
            Assert.IsTrue(b.AsLogical().First());
            e.Evaluate("rm(TestConversionToDataFrame_df, TestConversionToDataFrame_expected_colnames)");

        }
    }
}
