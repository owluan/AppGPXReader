﻿using System.Net.Http;
using System.Text.Json;
using System.Text;
using System;
using Xamarin.Forms;
using Xamarin.Essentials;
using AppGPXReader.Utility;
using AppGPXReader.Models;
using System.Net.Http.Headers;

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
            var authUrl = $"https://accounts.google.com/o/oauth2/auth?client_id={Constants.GoogleClientId}&redirect_uri={Constants.GoogleRedirectUrl}&scope=email&response_type=code";

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

                // Use the access token for making further API requests or handling user authentication

                // Agora, faça uma chamada à API UserInfo do Google usando o token de acesso
                var userInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var userInfoResponse = await httpClient.GetAsync(userInfoUrl);

                if (userInfoResponse.IsSuccessStatusCode)
                {
                    var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
                    var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (userInfo != null)
                    {
                        await SecureStorage.SetAsync("UserName", userInfo.Email);
                        await SecureStorage.SetAsync("UserEmail", userInfo.Email);

                        await Navigation.PushAsync(new MainPage());
                    }
                    else
                    {
                        // Trate o caso em que a desserialização falha
                    }

                }
                else
                {
                    // Handle unsuccessful Google authentication
                }
            }
        }
    }
}