using SE_WPF.Model;
using SE_WPF.View;
using SE_WPF.Presenter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SE_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //StartupUri="MainWindow.xaml"
        protected override void OnStartup(StartupEventArgs e)
        {
            MVP();
        }

        private static void MVP()
        {
            IModel model = new MyModel();
            IView view = new MainWindow();
            MyPresenter presenter = new MyPresenter(model, view);
            view.Start();
        }
    }
}
