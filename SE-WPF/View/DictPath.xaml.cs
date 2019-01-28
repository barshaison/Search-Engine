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
    /// Interaction logic for DictPath.xaml
    /// </summary>
    public partial class DictPath : Window
    {
        //private string m_pathToDict;
        private bool m_isStem;
        private bool m_loadSelected;

        public bool IsStem
        {
            get
            {
                return m_isStem;
            }

            set
            {
                m_isStem = value;
            }
        }

        public bool LoadSelected
        {
            get
            {
                return m_loadSelected;
            }

            set
            {
                m_loadSelected = value;
            }
        }

        public DictPath()
        {
            InitializeComponent();
            m_isStem = false;
            m_loadSelected = false;
        }

        private void no_selected(object sender, RoutedEventArgs e)
        {
            m_isStem = false;
        }

        private void yes_selected(object sender, RoutedEventArgs e)
        {
            m_isStem = true;
        }

        private void btn_load_selected(object sender, RoutedEventArgs e)
        {
            m_loadSelected = true;
            this.Close();
        }

        /* public string PathToDict
         {
             get
             {
                 return m_pathToDict;
             }

         }*/

        /* private void btn_browse_selected(object sender, RoutedEventArgs e)
         {
             OpenFileDialog ofd = new OpenFileDialog();
             ofd.Filter = "Text documents (.txt)|*.txt";
             if (ofd.ShowDialog() == true)
             {
                 m_pathToDict = ofd.FileName;
                 pathDict.Text = m_pathToDict;
             }
         }*/
    }
}
