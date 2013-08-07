using System;
using System.Globalization;
using Org.BouncyCastle.Math;

namespace Craps
{
    static internal class Utils
    {
        // wordsource should always be the one used to generate the words.
        public static BigInteger CrackingCombinations(string[] words, IWordSource source, PriorKnowledge prior)
        {
            int numChars = 0;
            int combinationsPerChar = 0;
            if (prior == PriorKnowledge.Passphrase)
            {
                numChars = words.Length;
                combinationsPerChar = source.NumWords();
            }
            else
            {
                numChars = String.Join("", words).Length;
                combinationsPerChar = AlphanumSource.Instance.NumWords();
            }
            var perChar = new BigInteger("" + combinationsPerChar);
            return perChar.Pow(numChars);
        }

        public static string NaturalTime(BigInteger seconds)
        {
            var s = "";
            if (seconds.CompareTo(new BigInteger("" + 60)) < 0)
            {
                s = seconds + " seconds";
            } else if (seconds.CompareTo(new BigInteger(""+3600)) < 0)
            {
                s = seconds.Divide(new BigInteger("" + 60)) + " minutes";
            } else if (seconds.CompareTo(new BigInteger(""+3600*24)) < 0)
            {
                s = seconds.Divide(new BigInteger(""+3600)) + " hours";
            } else if (seconds.CompareTo(new BigInteger(""+3600*24*365)) < 0)
            {
                s = seconds.Divide(new BigInteger("" + 3600*24)) + " days";
            }
            else
            {
                s = seconds.Divide(new BigInteger("" + 3600*24*365)) + " years";
            }
            return s;
        }

        public static string CrackingTimeNatural(BigInteger passCombos, long hashesPerSecond)
        {
            BigInteger seconds = passCombos.Divide(new BigInteger("" + hashesPerSecond));
            return NaturalTime(seconds);
        }

        public static string BitText(double perWord, int num, double bits, int wordsourceNumWords)
        {
            var format = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            format.NumberGroupSeparator = " ";
            format.NumberDecimalSeparator = ",";

            var s =
                "Randomness per word: {0:0.##} bits. That makes {1}*{2:0.##}={3:0.##} bits of randomness.\r\n(or {4:0,0} possible combinations).";
            var formatted = String.Format(s, perWord, num, perWord, bits, Math.Pow(wordsourceNumWords, num));
            return formatted;
        }

        public static void GetWords(int num, string[] words, IWordSource wordsource, IRandomnessSource randomness)
        {
            for (int i = 0; i < num; i++)
            {
                words[i] = wordsource.GetWord(randomness.NextRandom(0, wordsource.NumWords()));
            }
        }
    }
}