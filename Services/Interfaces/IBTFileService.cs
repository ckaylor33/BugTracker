using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BugTracker.Services.Interfaces
{
    public interface IBTFileService
    {
        //byte[] one of the simplest forms of data we can store in the db, lighter than other options
        //that might be chosen by the user
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);

        public string ConvertByteArrayToFile(byte[] fileData, string extension);

        public string GetFileIcon(string file);

        //estimates file size based on bytes composed within the file
        public string FormatFileSize(long bytes);
    }
}
