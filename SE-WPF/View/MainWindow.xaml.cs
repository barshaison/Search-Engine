using SE_WPF.View;
using System;
using System.Collections.Generic;
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

namespace SE_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,IView
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }

        //<Image  Source="Capture.PNG"/>
        //<Rectangle.Fill>
             //   <ImageBrush ImageSource = "Capture.PNG" />
           // </ Rectangle.Fill >
        public event viewEventDelegate ViewChanged;

        public void Output(string output)
        {
            MessageBox.Show(output);
        }

        public void Start()
        {
            this.Show();
            cnvs.Fill=(new ImageBrush(new BitmapImage(
                 new Uri("Capture.jpg", UriKind.Relative))));
            
        }
        public string getSaveQueryParams(ref bool savePressed)
        {
            SaveQueryResults sq = new SaveQueryResults();
            sq.ShowDialog();
            savePressed = sq.SavePressed;
            string pathToSaveOn = sq.SaveResOn;
            string whatToSave = sq.WhatToSave;
            return pathToSaveOn + " " + whatToSave;
        }
        public string getDictPath(ref bool isLoadSelected)
        {
           
            string pathToDict = "";
            DictPath dp = new DictPath();
            dp.ShowDialog();
            isLoadSelected = dp.LoadSelected;
           bool isStem = dp.IsStem;
            if (isStem)
            {
                pathToDict = "Stemed-Dictionary.txt Stemmed-DocsLength.txt N.txt Stemed-PostingF.txt DocLangDict.txt Langs.txt DictionaryTF.txt"; //pass with stem
            }
            else
            {
                pathToDict = "Dictionary.txt DocsLength.txt N.txt PostingF.txt DocLangDict.txt Langs.txt Stemed-DictionaryTF.txt"; //pass without stem
            }
            //pathToDict = dp.PathToDict;
            //if stem/off
            return pathToDict;
        }

        public string getSingleQuery(ref bool searchPressed)
        {
            SingleQuery sq = new SingleQuery();
            sq.ShowDialog();
            searchPressed = sq.SearchPressed;
            string query = sq.Query;
            
            return query;
        }
        public void outputFileSearchResult(Dictionary<string, List<string>> searchResults)
        {
            SearchResults sr = new SearchResults();
            foreach (string qID in searchResults.Keys)
            {
                List<string> currQueryResult = searchResults[qID];
                string query = currQueryResult.ElementAt(currQueryResult.Count - 1).ToLower();
                sr.searchRes.Items.Insert(0, "Query Entered: ---" + query + " ---");
                sr.searchRes.Items.Insert(1, "Number of documents retrived: ---" + (currQueryResult.Count - 1) + " ---");
                sr.searchRes.Items.Insert(2, "------------");
                int i = 3;
                for (int j = 0; j < currQueryResult.Count - 1; j++)
                {
                    sr.searchRes.Items.Insert(i, currQueryResult.ElementAt(j));
                    i++;
                    sr.searchRes.Items.Insert(i, "------------");
                    i++;
                }
            }
            sr.ShowDialog();
        }
        public void outputSearchresult(List<string> searchResults)
        {
            SearchResults sr = new SearchResults();
            string queryAndID = searchResults.ElementAt(searchResults.Count - 1).ToLower();
            string query = queryAndID.Split('|')[0];
            sr.searchRes.Items.Insert(0, "Query Entered: ---" + query + " ---");
            sr.searchRes.Items.Insert(1, "Number of documents retrived: ---" + (searchResults.Count-1) + " ---");
            sr.searchRes.Items.Insert(2, "------------");
           int i = 3;
            for (int j = 0; j < searchResults.Count - 1;j++)
            {
                sr.searchRes.Items.Insert(i, searchResults.ElementAt(j));
                i++;
                sr.searchRes.Items.Insert(i, "------------");
                i++;
            }

            sr.ShowDialog();
        }

        public string getQuriesFilePath(ref bool searchPressed)
        {
            FileQuries fq = new FileQuries();
            fq.ShowDialog();
            searchPressed = fq.SearchPressed;
            string pathToFileQ = fq.PathToQuries;
            return pathToFileQ;
        }

        public string getIndexParameters()
        {
            
            Index_Params parameters = new Index_Params();
            parameters.ShowDialog();
            string corpusPath = parameters.CorpusPath; 
            string stopWordsPath = parameters.StopWordsPath; //mazeDetailsWindow.stopWordsPath
            string stemOn = parameters.StemOn; //take from checkbox
            string saveIndexOn = parameters.SaveIndexPath; //mazeDetailsWindow.saveIndexOn
            if (corpusPath == "x" || stopWordsPath=="x" ||  saveIndexOn=="x")
            {
                return "x|x|x|x";
            }
            return corpusPath + "|" + stopWordsPath + "|" + stemOn + "|" + saveIndexOn;
        }

        public string getPathToSaveDic()
        {
            PathToSaveDicOn path = new PathToSaveDicOn();
            path.ShowDialog();
            string p = path.Path;
            if(p == null)
            {
                p = "x";
                Output("You must choose a path\n\n come on   :(");
            }
            return p;
        }

        public void outputDic(Dictionary<string, long> dictionary)
        {
            ShowDictionary sd = new ShowDictionary();
            sd.dic_text.Inlines.Add(new Run("Term : total_TF"));
            sd.dic_text.Inlines.Add(new LineBreak());
            foreach (string term in dictionary.Keys)
            {
                sd.dic_text.Inlines.Add(new Run(term + "    " + dictionary[term]));
                sd.dic_text.Inlines.Add(new LineBreak());
            }
            sd.ShowDialog();
        }

        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            ViewChanged("Generate Index");
           
        }

        private void btn_display_click(object sender, RoutedEventArgs e)
        {
            ViewChanged("Display");
        }

        private void btn_Reset_click(object sender, RoutedEventArgs e)
        {
            ViewChanged("Reset");
        }

        private void btn_Save_Dic_click(object sender, RoutedEventArgs e)
        {
            ViewChanged("SaveDic");
        }

        private void btn_language_click(object sender, RoutedEventArgs e)
        {
            ViewChanged("language");
        }

        public List<string> getChosenLangs(HashSet<string> langs)
        {
            List<string> ans;
            Languages lgs = new Languages();

            foreach (string lang in langs)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = lang;
                lgs.lang_text.Items.Add(item);

            }
            lgs.ShowDialog();
            ans = lgs.ChosenLangs;
            return ans;
        }
       /* public void outputLang(HashSet<string> langs)
        {
            Languages lgs = new Languages();

            foreach(string lang in langs)
            {
                lgs.lang_text.Items.Add(lang);

            }
            lgs.ShowDialog();
        }*/

        private void btn_LoadDic(object sender, RoutedEventArgs e)
        {
            // Output("This operation will be available on part B :)");
            ViewChanged("loadDict");
        }

        private void controls_click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Start --> Opens window with requested fileds to fill in order to start Index generation\n\nDisplay Dictionary --> Displays table with term name and term total frequancy\n\nReset--> Resets program memory and deletes index from disk\n\nSave Dictionary On Disk--> Saves the dictionary on selected folder\n\nLoad Dictionary From Disk-->will be available in the future\n\nChoose Language--> Displays list of documents languages");
        }

        private void instractions_click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("To generate index press start,fill all the requested fields and pres Create");
        }

        private void btn_about_click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This program was written by Daniel Barshai   Id: 203194048\n\nAll rights reserved (c)");
        }

        /*public string getPathToDict()
        {
            throw new NotImplementedException();
        }*/

        private void btn_fileQ_Click(object sender, RoutedEventArgs e)
        {
            ViewChanged("fileQuery");
        }

        private void btn_singleQ_Click(object sender, RoutedEventArgs e)
        {
            ViewChanged("singleQuery");
        }

        private void btn_saveQ_Click(object sender, RoutedEventArgs e)
        {
            ViewChanged("saveQResults");
        }
    }
}
