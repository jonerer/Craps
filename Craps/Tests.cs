using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Org.BouncyCastle.Math;

namespace Craps
{
    [TestFixture]
    class Tests
    {
        [Test]
        public void TestNaturalTime()
        {
            Assert.AreEqual(MainWindow.NaturalTime(new BigInteger(""+5119096458)), "162 years");
            Assert.AreEqual(MainWindow.NaturalTime(new BigInteger("" + 624889)), "7 days");
            Assert.AreEqual(MainWindow.NaturalTime(new BigInteger("" + 3500)), "58 minutes");
            Assert.AreEqual(MainWindow.NaturalTime(new BigInteger("" + 35)), "35 seconds");
        }

    }
}
