using Microsoft.Win32;
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
using System.Windows.Shapes;



namespace SE_WPF.View
{
    /// <summary>
    /// Interaction logic for Index_Params.xaml
    /// </summary>
    public partial class Index_Params : Window
    {
        private string m_corpusPath;
        private string m_stopWordsPath;
        private string m_stemOn;
        private string m_saveIndexPath;

        private string s_corpusPath;
        private string s_stopWordsPath;
        private string s_stemOn;
        private string s_saveIndexPath;

        public string CorpusPath
        {
            get
            {
                return m_corpusPath;
            }

            set
            {
                m_corpusPath = value;
            }
        }

        public string StopWordsPath
        {
            get
            {
                return m_stopWordsPath;
            }

            set
            {
                m_stopWordsPath = value;
            }
        }

        public string StemOn
        {
            get
            {
                return m_stemOn;
            }

            set
            {
                m_stemOn = value;
            }
        }

        public string SaveIndexPath
        {
            get
            {
                return m_saveIndexPath;
            }

            set
            {
                m_saveIndexPath = value;
            }
        }

        public Index_Params()
        {
            InitializeComponent();
            m_stemOn = "False"; //defualt
            m_corpusPath = "x";
            m_saveIndexPath = "x";
            m_stopWordsPath = "x";

            s_stemOn = "x"; 
            s_corpusPath = "x";
            s_saveIndexPath = "x";
            s_stopWordsPath = "x";
        }

        private void btn_corpus_selected(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            //filter to choose type of files like in the corpus  !!!!!!!!!!!!!!!!!
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            s_corpusPath = fbd.SelectedPath;
            pathCorpus.Text = s_corpusPath;


        }

        private void btn_stopWords_selected(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text documents (.txt)|*.txt";
            if (ofd.ShowDialog() == true)
            {
                s_stopWordsPath = ofd.FileName;
                pathStopWords.Text = s_stopWordsPath;
            }
        }

        private void btn_saveIndex_selected(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            s_saveIndexPath = fbd.SelectedPath;
            pathInd.Text = s_saveIndexPath;

        }

        private void no_selected(object sender, RoutedEventArgs e)
        {
            s_stemOn = "False";
            
        }

        private void yes_selected(object sender, RoutedEventArgs e)
        {
            s_stemOn = "True";
           
        }

        private void btn_creatIndex(object sender, RoutedEventArgs e)
        {
            m_corpusPath = s_corpusPath;
            m_stopWordsPath = s_stopWordsPath;
            m_stemOn = s_stemOn;
            m_saveIndexPath = s_saveIndexPath;
            this.Close();
        }

        private void btn_cancel(object sender, RoutedEventArgs e)
        {
            m_corpusPath = "c";
            m_stopWordsPath = "c";
            m_stemOn = "c";
            m_saveIndexPath = "c";
            this.Close();
        }
    }
}
