using System.Net.Http;
using System.Text.Json;
using System.Text;
using System;
using Xamarin.Forms;
using Xamarin.Essentials;
using AppGPXReader.Utility;
using AppGPXReader.Models;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppGPXReader.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnGoogleLoginClicked(object sender, EventArgs e)
        {
            var authUrl = $"https://accounts.google.com/o/oauth2/auth?client_id={Constants.GoogleClientId}&redirect_uri={Constants.GoogleRedirectUrl}&scope=email profile&response_type=code";

            var callbackUri = new Uri(Constants.GoogleRedirectUrl);

            var authResult = await WebAuthenticator.AuthenticateAsync(new Uri(authUrl), callbackUri);

            if (authResult?.Properties != null && authResult.Properties.ContainsKey("code"))
            {
                var code = authResult.Properties["code"];

                var tokenUrl = "https://www.googleapis.com/oauth2/v4/token";
                var httpClient = new HttpClient();

                var content = new StringContent($"code={code}&client_id={Constants.GoogleClientId}&redirect_uri={Constants.GoogleRedirectUrl}&grant_type=authorization_code", Encoding.UTF8, "application/x-www-form-urlencoded");

                var tokenResponse = await httpClient.PostAsync(tokenUrl, content);
                var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

                var accessToken = JsonDocument.Parse(tokenContent).RootElement.GetProperty("access_token").GetString();

                await SecureStorage.SetAsync("accessToken", accessToken);

                // Use the access token for making further API requests or handling user authentication

                var userInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var userInfoResponse = await httpClient.GetAsync(userInfoUrl);

                if (userInfoResponse.IsSuccessStatusCode)
                {
                    var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
                    var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (userInfo != null)
                    {
                        await SecureStorage.SetAsync("UserName", userInfo.Name);
                        await SecureStorage.SetAsync("UserEmail", userInfo.Email);
                        await SecureStorage.SetAsync("UserPicture", userInfo.Picture);

                        // Crie a instância da MainPage (MasterDetailPage)
                        var menuPage = new MenuPage();
                        menuPage.Title = "Navigation";

                        var mainPage = new MasterDetailPage
                        {
                            Master = menuPage,
                            Detail = new NavigationPage(new MainPage()) // Defina a MainPage como a página de detalhes
                        };

                        // Defina a MainPage como a nova página raiz
                        App.Current.MainPage = mainPage;
                    }
                    else
                    {
                        await DisplayAlert("Erro", "Ocorreu um erro durante o login.", "OK");  // Handle the case where deserialization fails
                    }

                }
                else
                {
                    await DisplayAlert("Erro", "Ocorreu um erro durante o login.", "OK"); // Handle unsuccessful Google authentication
                }
            }
        }


    }
}