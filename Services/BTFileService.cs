using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTFileService : IBTFileService
    {
        private readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };

        //code to do this built into .Net Core framework
        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            try
            {
                string imageBase64Data = Convert.ToBase64String(fileData);
                return string.Format($"data:{extension};base64,{imageBase64Data}");
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        public async Task<byte[]> EncodeImageAsync(string fileName)
        {
            var file = $"{Directory.GetCurrentDirectory()}/wwwroot/img/{fileName}";
            return await File.ReadAllBytesAsync(file);
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                MemoryStream memoryStream = new();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                memoryStream.Close();
                memoryStream.Dispose();

                return byteFile;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string FormatFileSize(long bytes)
        {
            int counter = 0;
            //Convert long to decimal by assigning it to a decimal variable
            decimal fileSize = bytes;
            while (Math.Round(fileSize / 1024) >= 1)
            {
                fileSize /= bytes;
                counter++;
            }
            return string.Format("{0:n1}{1}", fileSize, suffixes[counter]);
        }

        public string GetFileIcon(string file)
        {
            string fileImage = "default";

            if (!string.IsNullOrEmpty(file))
            {
                fileImage = Path.GetExtension(file).Replace(".", "");
                return $"/img/contenttype/{fileImage}.png";
            }
            return fileImage;
        }
    }
}
