using AppGPXReader.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.ObjectModel;

namespace AppGPXReader.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage
    {
        public UserInfo UserInfo { get; set; } = new UserInfo();
        public ObservableCollection<Models.MenuItem> MenuItems { get; set; }

        public MenuPage()
        {
            InitializeComponent();

            UserInfo.Name = SecureStorage.GetAsync("UserName").Result;
            UserInfo.Email = SecureStorage.GetAsync("UserEmail").Result;
            UserInfo.Picture = SecureStorage.GetAsync("UserPicture").Result;

            menuPageListView = this.FindByName<ListView>("menuPageListView");

            MenuItems = new ObservableCollection<Models.MenuItem>
        {
            new Models.MenuItem { Title = "Home", TargetType = typeof(MainPage) }, // Substitua HomePage pela página que deseja exibir
            new Models.MenuItem { Title = "Settings", TargetType = typeof(MainPage) } // Substitua SettingsPage pela página que deseja exibir
            // Adicione mais itens conforme necessário
        };

            BindingContext = this;
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            try
            {
                // Revoke access token
                await RevokeAccessTokenAsync(SecureStorage.GetAsync("accessToken").Result);

                // Clear application user information
                SecureStorage.Remove("UserName");
                SecureStorage.Remove("UserEmail");
                SecureStorage.Remove("UserPicture");
                SecureStorage.Remove("accessToken");

                // Navigate back to login page
                App.Current.MainPage = new NavigationPage(new LoginPage());
            }
            catch (Exception ex)
            {
                // Lidar com exceções, se necessário
                await DisplayAlert("Erro", ex.Message, "OK");
            }
        }

        private async Task RevokeAccessTokenAsync(string accessToken)
        {
            var client = new HttpClient();

            var content = new FormUrlEncodedContent(new[]
            {
                 new KeyValuePair<string, string>("token", accessToken)
            });

            var response = await client.PostAsync("https://accounts.google.com/o/oauth2/revoke", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Logout", "Logout efetuado com sucesso!", "OK"); // Logout successful
            }
            else
            {
                await DisplayAlert("Erro", "Ocorreu um erro durante o logout.", "OK");  // Logout failed
            }

        }              

    }
}