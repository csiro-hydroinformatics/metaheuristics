using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CSIRO.Metaheuristics.R.Tests
{
    [TestFixture]
    public class TestPercentiles
    {
        [Test]
        public void TestCSharpPercentilesImpl()
        {
            double[] values = new[]{9.0, 1, 8, 0, 2, 7, 3, 6, 4, 5};
            double delta = 1e-12;
            Assert.AreEqual(4.5, PercentilesCalculationsHelper.BasicPercentilesInterpolated(0.5, values), delta);
            Assert.AreEqual(2.25, PercentilesCalculationsHelper.BasicPercentilesInterpolated(0.25, values), delta);
            Assert.AreEqual(6.75, PercentilesCalculationsHelper.BasicPercentilesInterpolated(0.75, values), delta);
            Assert.AreEqual(9.0, PercentilesCalculationsHelper.BasicPercentilesInterpolated(1, values), delta);
        }
    }
}
