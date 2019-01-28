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
    /// Interaction logic for FileQuries.xaml
    /// </summary>
    public partial class FileQuries : Window
    {
        private string m_pathToQuries;
        private bool m_searchPressed;

        public FileQuries()
        {
            InitializeComponent();
            m_pathToQuries = "x";
            m_searchPressed = false;
        }

        public string PathToQuries
        {
            get
            {
                return m_pathToQuries;
            }

            set
            {
                m_pathToQuries = value;
            }
        }

        public bool SearchPressed
        {
            get
            {
                return m_searchPressed;
            }

            set
            {
                m_searchPressed = value;
            }
        }

        private void btn_browse_selected(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text documents (.txt)|*.txt";
            if (ofd.ShowDialog() == true)
            {
                m_pathToQuries = ofd.FileName;
                pathToQuries.Text = m_pathToQuries;
            }
        }

        private void btn_StartSearch_click(object sender, RoutedEventArgs e)
        {
            m_searchPressed = true;
            this.Close();
        }
    }
}
