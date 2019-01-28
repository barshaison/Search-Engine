using SE_WPF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for SingleQuery.xaml
    /// </summary>
    public partial class SingleQuery : Window
    {
        private string m_query;
        private string m_toComplete;
        public Task<List<GoogleSuggestion>> suggestions;
        public bool m_allreadysuggested = false;
        public bool m_ready = false;
        private bool m_searchPressed;

        public string Query
        {
            get
            {
                return m_query;
            }

            set
            {
                m_query = value;
            }
        }

        public string ToComplete
        {
            get
            {
                return m_toComplete;
            }

            set
            {
                m_toComplete = value;
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

        public SingleQuery()
        {
            InitializeComponent();
            m_query = "x_x";
            m_searchPressed = false;
        }

       

        private void btn_search_Click(object sender, RoutedEventArgs e)
        {
            m_query = singleQtext.Text;
            m_searchPressed = true;
            this.Close();
        }

       
        private void space_pressed(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Space)
            {
                m_toComplete = singleQtext.Text;
                if (!m_allreadysuggested)
                {
                   
                    suggestions = giveSuggestions(m_toComplete);
                    Thread.Sleep(4500);
                    completeOptions.IsEnabled = true;
                }
                 
            }
            

        }

        private Task<List<GoogleSuggestion>> giveSuggestions(string m_toComplete)
        {
            List<string> ans = new List<string>();
            SearchSuggestionsAPI s = new SearchSuggestionsAPI();
            var res = s.GetSearchSuggestions(m_toComplete);

           
            return res;
        }

        private void focus(object sender, RoutedEventArgs e)
        {
            if(!m_allreadysuggested)
            {
                //if (m_ready)
                {
                    if (suggestions != null)
                    {
                        foreach (GoogleSuggestion r in suggestions.Result)
                        {
                            completeOptions.Items.Add(r.ToString());
                        }
                        m_allreadysuggested = true;
                    }
                   
                }
                
            }
            
        }

        private void selsectec(object sender, SelectionChangedEventArgs e)
        {
            singleQtext.Text = ((ComboBox)sender).SelectedItem.ToString();
        }
    }
}
