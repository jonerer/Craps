using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
            Assert.AreEqual(Utils.NaturalTime(new BigInteger(""+5119096458)), "162 years");
            Assert.AreEqual(Utils.NaturalTime(new BigInteger("" + 624889)), "7 days");
            Assert.AreEqual(Utils.NaturalTime(new BigInteger("" + 3500)), "58 minutes");
            Assert.AreEqual(Utils.NaturalTime(new BigInteger("" + 35)), "35 seconds");
        }

        [Test]
        public void TestCombinations()
        {
            var fivenumbers = new string[] {"1", "2", "3", "4", "5"};
            var fivewords = new string[] {"one", "two", "three", "four", "five"};
            var sixwords = new string[] {"one", "two", "three", "four", "five", "six"};
            var en = new DicewareFileSource("diceware8k.txt");
            var alphasource = new AlphanumSource();
            var alpha = PriorKnowledge.Alphanumeric;
            var pass = PriorKnowledge.Passphrase;

            Assert.AreEqual(Utils.CrackingCombinations(fivenumbers, alphasource, alpha), 
                Utils.CrackingCombinations(fivenumbers,alphasource,pass));
            Assert.AreEqual(Utils.CrackingCombinations(fivewords, en, pass), new BigInteger("36893488147419103232"));
            Assert.AreEqual(Utils.CrackingCombinations(sixwords, en, pass), new BigInteger("302231454903657293676544"));

            Assert.AreEqual(Utils.CrackingCombinations(fivewords, en, alpha), new BigInteger("11361668153983839080134359106715648"));
        }

        [Test]
        public void TestCrackingTime()
        {
            var twoWords = new string[] {"one", "two"};
            var en = new DicewareFileSource("diceware8k.txt");
            var alpha = PriorKnowledge.Alphanumeric;
            var pass = PriorKnowledge.Passphrase;

            var HashesPerSecond = 100000;
            var alphaCombos = Utils.CrackingCombinations(twoWords, en, alpha);
            var passCombos = Utils.CrackingCombinations(twoWords, en, pass);

            // https://www.google.se/search?q=61**11&oq=61**11&aqs=chrome.0.69i57j0j69i62l3.6089j0&sourceid=chrome&ie=UTF-8#bav=on.2,or.r_cp.r_qf.&fp=ddb5824ed82ce730&q=62**6+%2F+(10000000+*+24)&safe=off
            // https://www.google.se/search?q=61**11&oq=61**11&aqs=chrome.0.69i57j0j69i62l3.6089j0&sourceid=chrome&ie=UTF-8#bav=on.2,or.r_cp.r_qf.&fp=ddb5824ed82ce730&q=8192**2+%2F+(10000000+)&safe=off

            Assert.AreEqual("6 days", Utils.CrackingTimeNatural(alphaCombos, HashesPerSecond));
            Assert.AreEqual("11 minutes", Utils.CrackingTimeNatural(passCombos, HashesPerSecond));
        }
    }
}
