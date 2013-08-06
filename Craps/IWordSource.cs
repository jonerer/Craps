using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Craps
{
    public interface IWordSource
    {
        string GetWord(int number); // maximum is 8192
        int NumWords();
    }

    class DicewareFileSource : IWordSource
    {
        public string FileName { get; set; }
        public string[] Words { get; set; }

        public DicewareFileSource(string fileName)
        {
            FileName = fileName;
            var fileStream = File.OpenRead(FileName);
            var reader = new StreamReader(fileStream);
            Words = reader.ReadToEnd().Split().Where(x => x != "").ToArray();
        }

        public string GetWord(int number)
        {
            return Words[number];
        }

        public int NumWords()
        {
            return Words.Length;
        }

        public override string ToString()
        {
            return FileName;
        }
    }

    class AlphanumSource : IWordSource
    {
        public static string Possible = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVXYZ0123456789";

        public int NumWords()
        {
            return Possible.Length;
        }

        public string GetWord(int number)
        {
            return Possible[number].ToString();
        }

        public override string ToString()
        {
            return "Alphanumeric";
        }
    }
}
