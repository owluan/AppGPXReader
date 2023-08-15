using AppGPXReader.Models;
using AppGPXReader.Services;
using AppGPXReader.Views;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace AppGPXReader
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private ILocationService locationService;

        public MainPage()
        {
            InitializeComponent();

            locationService = DependencyService.Get<ILocationService>();

            RequestLocationPermission();
        }

        private async void RequestLocationPermission()
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
            {
                await InitializeMapWithUserLocation();
            }
            else
            {
                await DisplayAlert("Permissão de Localização", "A permissão de localização não foi concedida. Não é possível exibir sua localização atual.", "OK");
            }
        }

        private async Task InitializeMapWithUserLocation()
        {
            var location = await locationService.GetLocationAsync();

            if (location != null)
            {
                map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(location.Latitude, location.Longitude), Distance.FromKilometers(1)));
            }
            else
            {
                await DisplayAlert("Erro", "Não foi possível obter a localização atual", "OK");
            }
        }

        private async void OnLoadGpxClicked(object sender, EventArgs e)
        {
            try
            {
                // Prompts the user to choose a GPX file
                FileData fileData = await CrossFilePicker.Current.PickFile();

                if (fileData == null)
                    return;

                string filePath = fileData.FilePath;

                // Checks if the returned URI is in content format (content://)
                if (filePath.StartsWith("content://"))
                {
                    // Converts the URI to an actual file path
                    filePath = await GetActualFilePath(filePath);
                }

                // Read the contents of the GPX file
                string gpxContent = File.ReadAllText(filePath);

                // Parses the contents of the GPX file
                XDocument gpxDocument = XDocument.Parse(gpxContent);
                XNamespace ns = "http://www.topografix.com/GPX/1/1";
                XElement trkElement = gpxDocument.Root.Element(ns + "trk");

                if (trkElement == null)
                {
                    await DisplayAlert("Erro", "Arquivo GPX inválido", "OK");
                    return;
                }

                // Creates a List with each coordinate contained in the file

                var trackPoints = gpxDocument.Descendants(ns + "trkpt")
                             .Select(x => new
                             {
                                 Latitude = (double)x.Attribute("lat"),
                                 Longitude = (double)x.Attribute("lon")
                             })
                             .ToList();

                // Displays the route on the map using polylines           

                var polyline = new Polyline
                {
                    StrokeColor = Color.Blue,
                    StrokeWidth = 4
                };

                foreach (var point in trackPoints)
                {
                    var position = new Position(point.Latitude, point.Longitude);
                    polyline.Geopath.Add(position);
                }

                map.MapElements.Clear();
                map.MapElements.Add(polyline);

                // Gets the starting point of the route and moves the map to the starting position
                var startPoint = trackPoints.FirstOrDefault();

                if (startPoint != null)
                {
                    var initialPosition = new Position(startPoint.Latitude, startPoint.Longitude);

                    var zoomLevel = 14;

                    map.MoveToRegion(MapSpan.FromCenterAndRadius(initialPosition, Distance.FromKilometers(zoomLevel / 2)));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.Message, "OK");
            }
        }

        private async Task<string> GetActualFilePath(string contentUri)
        {
            using (var stream = await DependencyService.Get<IFilePickerService>().GetFileStream(contentUri))
            {
                using (var memStream = new MemoryStream())
                {
                    stream.CopyTo(memStream);
                    return await SaveFileToDisk(memStream.ToArray());
                }
            }
        }

        private Task<string> SaveFileToDisk(byte[] fileData)
        {
            string filePath = Path.Combine(FileSystem.CacheDirectory, "temp.gpx");
            File.WriteAllBytes(filePath, fileData);
            return Task.FromResult(filePath);
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
                await Navigation.PushAsync(new LoginPage());
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
