using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.SqlServer.Server;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Craps
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _wordSource;
        private AlphanumSource _alphanumSource;
        private PriorKnowledge _priorKnowledge;
        private long _hashesPerSecondPerGpu;
        private int _numberOfGpus;
        private IWordSource _wordsource;
        public List<IWordSource> WordSources { get; set; }
        public List<IRandomnessSource> RandomnessSources { get; set; }

        public int NumWords { get; set; }
        public string[] Words { get; set; }

        public string PassphraseOutput { get; set; }
        public string PassphraseBitsText { get; set; }

        public string AlphanumOutput { get; set; }
        public string AlphanumBitsText { get; set; }

        public string NumBitsOutput { get; set; }
        public string NumBitsTooltip { get; set; }

        public string TimeToCrack { get; set; }
        public string TimeToCrackTooltip { get; set; }

        public long HashesPerSecondPerGpu
        {
            get { return _hashesPerSecondPerGpu; }
            set { _hashesPerSecondPerGpu = value;
                NotifyPropertyChanged("HashesPerSecondPerGpu");
            }
        }

        public long HashesPerDay { get { return HashesPerSecondPerGpu*3600*24 * NumberOfGpus; }}
        public long HashesPerSecond { get { return HashesPerSecondPerGpu*3600 * NumberOfGpus; }}

        public int NumberOfGpus
        {
            get { return _numberOfGpus; }
            set { _numberOfGpus = value; NotifyPropertyChanged("NumberOfGpus"); }
        }

        public PriorKnowledge PriorKnowledge
        {
            get { return _priorKnowledge; }
            set { _priorKnowledge = value; NotifyPropertyChanged("PriorKnowledge"); }
        }

        public string WordSource
        {
            get { return _wordSource; }
            set { _wordSource = value; NotifyPropertyChanged("WordSource"); }
        }

        public bool CanCreatePhrase
        {
            get
            {
                var val = NumWords;
                return val > 0;
            }
        }

        public IWordSource Wordsource
        {
            get { return _wordsource; }
            set { _wordsource = value; }
        }

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

        public MainWindow()
        { 
            HashesPerSecondPerGpu = (long) (82 * Math.Pow(10,8)); // a default value, taken from Peter Magnussons spreadsheet.
            NumWords = 5;
            NumberOfGpus = 1;
            PriorKnowledge = PriorKnowledge.Passphrase;

            var swe = new DicewareFileSource("diceware8k-sv.txt");
            var eng = new DicewareFileSource("diceware8k.txt");
            WordSources = new List<IWordSource>();
            WordSources.Add(eng);
            WordSources.Add(swe);
            _alphanumSource = new AlphanumSource();
            WordSources.Add(_alphanumSource);

            var local = new LocalComputerRandomness();
            RandomnessSources = new List<IRandomnessSource>();
            RandomnessSources.Add(local);

            InitializeComponent();

            MainGrid.DataContext = this;
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            // redo the cracking calculations.
            DoCrackingCalculations();
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

        private bool isDoingCrackingCalculations = false;
        private void DoCrackingCalculations()
        {
            var canDoCrackingCalculations = Words != null && WordSource != null && HashesPerDay != 0 && !isDoingCrackingCalculations;
            if (canDoCrackingCalculations)
            {
                isDoingCrackingCalculations = true;
                var combinations = CrackingCombinations(Words, Wordsource, PriorKnowledge);
                TimeToCrack = CrackingTimeNatural(combinations, HashesPerSecond);
                TimeToCrackTooltip = String.Format("{0:0,0} combinations, {1:0,0} cracked per day.", combinations, HashesPerDay);
                NotifyPropertyChanged("TimeToCrack");
                NotifyPropertyChanged("TimeToCrackTooltip");
                isDoingCrackingCalculations = false;
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Regenerate();
        }


        private void Regenerate()
        {
            if (CanCreatePhrase)
            {
                int num = NumWords;
                Words = new string[num];
                var randomness = RandomnessSourceList.SelectedItem as IRandomnessSource;
                Wordsource = WordsSourceList.SelectedItem as IWordSource;

                int wordsourceNumWords = Wordsource.NumWords();
                double perWord = Math.Log(wordsourceNumWords, 2);
                double bits = Words.Length * perWord;

                GetWords(num, Words, Wordsource, randomness);
                PassphraseOutput = String.Join(" ", Words);

                DoCrackingCalculations();

                NumBitsOutput = String.Format("{0:0.##} bits", bits);
                NumBitsTooltip = BitText(perWord, num, bits, wordsourceNumWords);

                perWord = Math.Log(_alphanumSource.NumWords(), 2);
                int alphWords = 0; //(int) Math.Ceiling((float)bits / perWord);
                while (alphWords*perWord < bits)
                {
                    alphWords++;
                }
                var alphanumWords = new string[alphWords];
                bits = alphWords*perWord;
                GetWords(alphWords, alphanumWords, _alphanumSource, randomness);
                AlphanumOutput = String.Join("", alphanumWords);
                AlphanumBitsText = BitText(perWord, alphWords, bits, _alphanumSource.NumWords());

                NotifyPropertyChanged("PassphraseOutput");
                NotifyPropertyChanged("PassphraseBitsText");
                NotifyPropertyChanged("AlphanumOutput");
                NotifyPropertyChanged("AlphanumBitsText");
                NotifyPropertyChanged("NumBitsOutput");
                NotifyPropertyChanged("NumBitsTooltip");

            }
        }

        protected static string BitText(double perWord, int num, double bits, int wordsourceNumWords)
        {
            var format = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            format.NumberGroupSeparator = " ";
            format.NumberDecimalSeparator = ",";

            var s =
                "Randomness per word: {0:0.##} bits. That makes {1}*{2:0.##}={3:0.##} bits of randomness.\r\n(or {4:0,0} possible combinations).";
            var formatted = String.Format(s, perWord, num, perWord, bits, Math.Pow(wordsourceNumWords, num));
            return formatted;
        }

        private static void GetWords(int num, string[] words, IWordSource wordsource, IRandomnessSource randomness)
        {
            for (int i = 0; i < num; i++)
            {
                words[i] = wordsource.GetWord(randomness.NextRandom(0, wordsource.NumWords()));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void WordsSourceList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int numWords = (WordsSourceList.SelectedItem as IWordSource).NumWords();
            WordSource = "This list contains " + numWords + " words. That's " + Math.Log(numWords, 2) + " bits per word.";
        }


    }
}
