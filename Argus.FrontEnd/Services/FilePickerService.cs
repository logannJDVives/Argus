using System;
using System.IO;
using System.Threading.Tasks;

namespace Argus.FrontEnd.Services
{
    public interface IFilePickerService
    {
        Task<FilePickerResult?> PickFileAsync();
    }

    public class FilePickerResult
    {
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    }

    public class FilePickerService : IFilePickerService
    {
        public async Task<FilePickerResult?> PickFileAsync()
        {
            try
            {
                var customFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".zip" } },
                        { DevicePlatform.Android, new[] { "application/zip" } },
                        { DevicePlatform.iOS, new[] { "public.zip-archive" } },
                        { DevicePlatform.MacCatalyst, new[] { "public.zip-archive" } }
                    });

                var options = new PickOptions
                {
                    PickerTitle = "Select a ZIP file",
                    FileTypes = customFileType
                };

                var result = await FilePicker.Default.PickAsync(options);

                if (result == null)
                    return null;

                using var stream = await result.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);

                return new FilePickerResult
                {
                    FileName = result.FileName,
                    FileSize = ms.Length,
                    FileBytes = ms.ToArray()
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
