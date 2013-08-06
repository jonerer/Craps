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
        private string _wordSourceText;
        private AlphanumSource _alphanumSource;
        public List<IWordSource> WordSources { get; set; }
        public List<IRandomnessSource> RandomnessSources { get; set; }

        public string PassphraseOutput { get; set; }
        public string PassphraseBitsText { get; set; }

        public string AlphanumOutput { get; set; }
        public string AlphanumBitsText { get; set; }

        public string NumBitsOutput { get; set; }
        public string NumBitsTooltip { get; set; }

        public string TimeToCrack { get; set; }
        public string TimeToCrackTooltip { get; set; }

        public static long HashesPerSecond = (long) (8.2 * Math.Pow(10,9));
        public static long HashesPerDay = HashesPerSecond*3600*24;

        public string WordSourceText
        {
            get { return _wordSourceText; }
            set { _wordSourceText = value; NotifyPropertyChanged("WordSourceText");
            }
        }

        public bool CanCreatePhrase
        {
            get
            {
                int val = 0;
                var success = int.TryParse(NumWordsInput.Text, out val);
                return success && val > 0;
            }
        }

        public MainWindow()
        {
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
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Regenerate();
        }


        private void Regenerate()
        {
            if (CanCreatePhrase)
            {
                int num = int.Parse(NumWordsInput.Text);
                var words = new string[num];
                var randomness = RandomnessSourceList.SelectedItem as IRandomnessSource;
                var wordsource = WordsSourceList.SelectedItem as IWordSource;

                int wordsourceNumWords = wordsource.NumWords();
                double perWord = Math.Log(wordsourceNumWords, 2);
                double bits = words.Length * perWord;

                GetWords(num, words, wordsource, randomness);
                PassphraseOutput = String.Join(" ", words);

                NumBitsOutput = String.Format("{0:0.##} bits", bits);
                NumBitsTooltip = BitText(perWord, num, bits, wordsourceNumWords);
                var numWords = new BigInteger(""+wordsourceNumWords);
                var combinations = numWords.Pow(num);
                BigInteger days = combinations.Divide(new BigInteger(""+HashesPerDay));
                TimeToCrack = "" + days + " days";
                TimeToCrackTooltip = "" + combinations + " combinations, " + HashesPerDay + " cracked per day.";

                perWord = Math.Log(_alphanumSource.NumWords(), 2);
                int alphWords = 0; //(int) Math.Ceiling((float)bits / perWord);
                while (alphWords*perWord < bits)
                {
                    alphWords++;
                }
                words = new string[alphWords];
                bits = alphWords*perWord;
                GetWords(alphWords, words, _alphanumSource, randomness);
                AlphanumOutput = String.Join("", words);
                AlphanumBitsText = BitText(perWord, alphWords, bits, _alphanumSource.NumWords());

                NotifyPropertyChanged("PassphraseOutput");
                NotifyPropertyChanged("PassphraseBitsText");
                NotifyPropertyChanged("AlphanumOutput");
                NotifyPropertyChanged("AlphanumBitsText");
                NotifyPropertyChanged("NumBitsOutput");
                NotifyPropertyChanged("NumBitsTooltip");
                NotifyPropertyChanged("TimeToCrack");
                NotifyPropertyChanged("TimeToCrackTooltip");
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
            WordSourceText = "This list contains " + numWords + " words. That's " + Math.Log(numWords, 2) + " bits per word.";
        }
    }
}
