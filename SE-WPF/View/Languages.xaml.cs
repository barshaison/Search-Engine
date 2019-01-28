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
    /// Interaction logic for Languages.xaml
    /// </summary>
    public partial class Languages : Window
    {
        private List<string> m_chosenLangs;

        public Languages()
        {
            InitializeComponent();
        }

        public List<string> ChosenLangs
        {
            get
            {
                return m_chosenLangs;
            }

            set
            {
                m_chosenLangs = value;
            }
        }

        private void btn_chose_click(object sender, RoutedEventArgs e)
        {
            m_chosenLangs = new List<string>();
           var chosen = lang_text.SelectedItems;
            foreach(ListBoxItem item in chosen)
            {
                string lang = item.Content.ToString();
                m_chosenLangs.Add(lang);
            }
            this.Close();
        }
    }
}
