using AppGPXReader.Droid.Services;
using AppGPXReader.Services;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocationService))]
namespace AppGPXReader.Droid.Services
{
    public class LocationService : ILocationService
    {
        public async Task<Location> GetLocationAsync()
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Best);
            var location = await Geolocation.GetLocationAsync(request);
            return location;
           
        }
    }

    }


