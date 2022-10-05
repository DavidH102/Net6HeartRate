using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using HRServer;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;


namespace Net6HeartRate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HeartRateServer _client = null;

        public MainWindow()
        {

            InitializeComponent();

            //_client = new HeartRateServer(6547);
        
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddMudServices();
            serviceCollection.AddScoped<HeartRateServer>();
            serviceCollection.AddScoped<System.Net.HttpListener>();
            Resources.Add("services", serviceCollection.BuildServiceProvider());

        }
   


        //}
    }
}
