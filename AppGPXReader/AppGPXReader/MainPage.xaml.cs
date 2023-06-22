using AppGPXReader.Services;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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
        private Pin pin;
        private ILocationService locationService;
        private bool isUpdatingLocation;

        public MainPage()
        {
            InitializeComponent();

            locationService = DependencyService.Get<ILocationService>();

            // Cria um Pin para servir de marcador para a posição do dispositivo
            pin = new Pin
            {
                Label = "Você está aqui",
                Type = PinType.Place,
                Position = new Position(0, 0)
            };

            map.Pins.Add(pin);
                       

            // Atualiza a posição do "Pin" 
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                UpdateUserLocation();
                return isUpdatingLocation; // Continua a atualização enquanto isUpdatingLocation for verdadeiro
            });

        }

        private async void OnLoadGpxClicked(object sender, EventArgs e)
        {
            try
            {
                // Solicita ao usuário que escolha um arquivo GPX
                FileData fileData = await CrossFilePicker.Current.PickFile();

                if (fileData == null)
                    return;

                string filePath = fileData.FilePath;

                // Verifica se o URI retornado é no formato de conteúdo (content://)
                if (filePath.StartsWith("content://"))
                {
                    // Converte o URI em um caminho de arquivo real
                    filePath = await GetActualFilePath(filePath);
                }

                // Lê o conteúdo do arquivo GPX
                string gpxContent = File.ReadAllText(filePath);

                // Analisa o conteúdo do arquivo GPX
                XDocument gpxDocument = XDocument.Parse(gpxContent); 
                XNamespace ns = "http://www.topografix.com/GPX/1/1";
                XElement trkElement = gpxDocument.Root.Element(ns + "trk");

                if (trkElement == null)
                {
                    await DisplayAlert("Erro", "Arquivo GPX inválido", "OK");
                    return;
                }

                // Cria uma Lista com cada coordenada contida no arquivo

                var trackPoints = gpxDocument.Descendants(ns + "trkpt")
                             .Select(x => new
                             {
                                 Latitude = (double)x.Attribute("lat"),
                                 Longitude = (double)x.Attribute("lon")
                             })
                             .ToList();

                // Exibe a rota no mapa utilizando polilinhas

                List<Position> routeCoordinates = new List<Position>();

                foreach (var point in trackPoints)
                {
                    var position = new Position(point.Latitude, point.Longitude);
                    routeCoordinates.Add(position);
                }

                var polyline = new Polyline
                {
                    StrokeColor = Color.Blue,
                    StrokeWidth = 4
                };

                foreach (var point in trackPoints)
                {
                    polyline.Geopath.Add(new Position(point.Latitude, point.Longitude));
                }

                map.MapElements.Clear();
                map.MapElements.Add(polyline);

                // Obtem o ponto inicial da rota e Move o mapa para a posição inicial
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

        private async void OnCurrentLocationClicked(object sender, EventArgs e)
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

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Inicia a atualização da posição do "Pin"
            isUpdatingLocation = true;
        }
    

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Para de atualizar a posição do "Pin"
            isUpdatingLocation = false;
        }

        private async void UpdateUserLocation()
        {
            var location = await locationService.GetLocationAsync();

            if (location != null)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    pin.Position = new Position(location.Latitude, location.Longitude);
                });
            }
        }
    }
}
