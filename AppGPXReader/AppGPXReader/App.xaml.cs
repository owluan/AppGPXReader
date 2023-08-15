using AppGPXReader.Views;
using System;
using Xamarin.Auth;
using Xamarin.Forms;

namespace AppGPXReader
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new LoginPage());
        }
    }
}
