using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using AppGPXReader.Services;

[assembly: Dependency(typeof(AppGPXReader.Droid.Services.FilePickerServiceAndroid))]
namespace AppGPXReader.Droid.Services
{
    public class FilePickerServiceAndroid : IFilePickerService
    {
        public async Task<Stream> GetFileStream(string contentUri)
        {
            var context = Android.App.Application.Context;
            var resolver = context.ContentResolver;
            var inputStream = resolver.OpenInputStream(Android.Net.Uri.Parse(contentUri));
            var memoryStream = new MemoryStream();
            await inputStream.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
    }
}
