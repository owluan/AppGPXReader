using System.IO;
using System.Threading.Tasks;

namespace AppGPXReader.Services
{
    public interface IFilePickerService
    {
        Task<Stream> GetFileStream(string contentUri);
    }
}
