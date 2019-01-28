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
    /// Interaction logic for SaveQueryResults.xaml
    /// </summary>
    public partial class SaveQueryResults : Window
    {
        string m_saveResOn;
        string m_whatToSave;
        bool m_savePressed;

        public SaveQueryResults()
        {
            m_saveResOn = "x";
            m_whatToSave = "x";
            m_savePressed = false;
            InitializeComponent();
        }

        public string SaveResOn
        {
            get
            {
                return m_saveResOn;
            }

            set
            {
                m_saveResOn = value;
            }
        }

        public string WhatToSave
        {
            get
            {
                return m_whatToSave;
            }

            set
            {
                m_whatToSave = value;
            }
        }

        public bool SavePressed
        {
            get
            {
                return m_savePressed;
            }

            set
            {
                m_savePressed = value;
            }
        }

        private void btn_browse_click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            m_saveResOn = fbd.SelectedPath;
            pathToSaveOn.Text = m_saveResOn;
        }

        private void single_selected(object sender, RoutedEventArgs e)
        {
            m_whatToSave = "single";
        }

        private void fileQ_selected(object sender, RoutedEventArgs e)
        {
            m_whatToSave = "file";
        }

        private void btn_Save_click(object sender, RoutedEventArgs e)
        {
            m_savePressed = true;
            this.Close();
        }

       
    }
}
