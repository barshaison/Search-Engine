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
    /// Interaction logic for PathToSaveDicOn.xaml
    /// </summary>
    public partial class PathToSaveDicOn : Window
    {
        private string m_path;
        private string s_path;

        public PathToSaveDicOn()
        {
            InitializeComponent();
        }

        public string Path
        {
            get
            {
                return m_path;
            }

        }

        private void btn_browse_path(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            s_path = fbd.SelectedPath;
            pathTxt.Text = s_path;
        }

        private void btn_save_click(object sender, RoutedEventArgs e)
        {
            m_path = s_path;
            
            this.Close();
        }
    }
}
