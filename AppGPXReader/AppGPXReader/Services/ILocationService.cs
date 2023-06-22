using System.Threading.Tasks;
using Xamarin.Essentials;

namespace AppGPXReader.Services
{
    public interface ILocationService
    {
        Task<Location> GetLocationAsync();
    }
}
