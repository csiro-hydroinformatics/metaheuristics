using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CSIRO.Metaheuristics.Utils;
using CSIRO.Metaheuristics.Logging;

namespace CSIRO.Metaheuristics.Tests
{
    [TestFixture]
    public class TestLogging
    {
        [Test]
        public void TestInMemoryLogging()
        {
            InMemoryLogger logger;
            Dictionary<string, string> strMsg;
            Dictionary<string, string> popTags;
            createTestLogContent(out logger, out strMsg, out popTags);
            var logInfo = logger.ExtractLog("Results name");

            var columnKeys = logInfo.Item1;
            var lines = logInfo.Item2;
            Assert.AreEqual(4, lines.Count());
            Assert.AreEqual(8, columnKeys.Count());

            foreach (var name in new[] { "Message", "Category", "0", "1", "2", "1_s", "2_s", "0_s" })
                Assert.IsTrue(columnKeys.Contains(name), "Expected column name " + name);

            AssertEquivalentDictionaries(strMsg, lines[0]);

            var expectedLine = new Dictionary<string, string>(){
                    {"0", "1"}, 
                    {"1", "2.2"},
                    {"2", "4"}, 
                    {"1_s", "2.2"}, 
                    {"2_s", "4"}, 
                    {"0_s", "1" }};
            foreach (var item in popTags)
                expectedLine.Add(item.Key, item.Value);
            AssertEquivalentDictionaries(expectedLine, lines[2]);

        }

        private static void createTestLogContent(out InMemoryLogger logger, out Dictionary<string, string> strMsg, out Dictionary<string, string> popTags)
        {
            LoggerMhTestHelper.CreateTestLogContent(out logger, out strMsg, out popTags);
        }

        [Test]
        public void TestLoggerToColumns()
        {
            InMemoryLogger logger;
            Dictionary<string, string> strMsg;
            Dictionary<string, string> popTags;
            createTestLogContent(out logger, out strMsg, out popTags);

            Dictionary<string,string[]> strInfo;
            Dictionary<string,double[]> numericInfo;
            logger.ToColumns(out strInfo, out numericInfo);
            var expectedNumeric = new Dictionary<string, double[]>(){
                    {"0", new []   {double.NaN, 1.0,1.0,2.0}}, 
                    {"1", new []   {double.NaN, 2.0,2.2,3.0}}, 
                    {"2", new []   {double.NaN, 3.0,4.0,5.0}}, 
                    {"0_s", new [] {double.NaN, 1.0,1.0,2.0}}, 
                    {"1_s", new [] {double.NaN, 2.0,2.2,3.0}}, 
                    {"2_s", new [] {double.NaN, 3.0,4.0,5.0}}
            };
            AssertEquivalentDictionaries(expectedNumeric, numericInfo);

        }

        private static void AssertEquivalentDictionaries<T>(Dictionary<string, T> a, Dictionary<string, T> b)
        {
            if (a.Count() != b.Count()) throw new AssertionException("Dictionaries have a different length - cannot be equivalent in content");
            foreach (var k in a.Keys)
            {
                Assert.AreEqual(a[k], b[k]);
            }
        }

        private static TestHyperCube createPoint(double x, double y, double z)
        {
            return TestHyperCube.CreatePoint(0, 0, 10, x, y, z);
        }

    }
}
