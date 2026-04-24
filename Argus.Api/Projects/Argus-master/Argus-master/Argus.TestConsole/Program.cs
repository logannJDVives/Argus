using System;
using System.IO;
using System.Threading.Tasks;
using Argus.Sdk;

namespace Argus.TestConsole
{
    //Ai heeft deze testconsole geschreven (tijdbesparing)
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Configuration
                const string apiBaseUrl = "https://localhost:7001";
                const string zipFilePath = @"C:\path\to\your\project.zip"; // UPDATE THIS PATH
                const string projectName = "TestProject";

                Console.WriteLine("🚀 Argus SDK Test");
                Console.WriteLine($"📍 API URL: {apiBaseUrl}");
                Console.WriteLine($"📦 ZIP File: {zipFilePath}");
                Console.WriteLine($"📛 Project Name: {projectName}");
                Console.WriteLine();

                // Validate file exists
                if (!File.Exists(zipFilePath))
                {
                    Console.WriteLine($"❌ Error: File not found: {zipFilePath}");
                    Console.WriteLine("   Update the zipFilePath variable above");
                    return;
                }

                Console.WriteLine("✓ ZIP file found");

                // Initialize SDK
                var options = new ArgusApiClientOptions(apiBaseUrl);
                using var httpClient = new HttpClient();
                var apiClient = new ArgusApiClient(httpClient, options);

                Console.WriteLine("✓ SDK initialized");
                Console.WriteLine();

                // Upload project
                Console.WriteLine("📤 Uploading project...");
                var zipBytes = await File.ReadAllBytesAsync(zipFilePath);

                var projectId = await apiClient.UploadProjectAsync(projectName, zipBytes, Path.GetFileName(zipFilePath));

                Console.WriteLine();
                Console.WriteLine("✅ Upload successful!");
                Console.WriteLine($"📋 Project ID: {projectId}");
                Console.WriteLine();
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"❌ File error: {ex.Message}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ API error: {ex.Message}");
                Console.WriteLine("   Make sure Argus.Api is running on https://localhost:7001");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"❌ Validation error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected error: {ex.Message}");
                Console.WriteLine($"   {ex.GetType().Name}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
