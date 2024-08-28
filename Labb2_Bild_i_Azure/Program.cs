using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;

namespace Labb2_Bild_i_Azure
{
    internal class Program
    {
        private static ComputerVisionClient cvClient;

        static async Task Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();
            string cogSvcEndpoint = configuration["CognitiveServicesEndpoint"];
            string cogSvcKey = configuration["CognitiveServiceKey"];

            string imagePath = "";

            Console.WriteLine("enter local path to image:");
            imagePath = Console.ReadLine();

            //Remove " from Path
            imagePath = imagePath.Replace("\"","");


            Console.WriteLine();
            Console.WriteLine("enter width for thumbnail(leave blank for standard:100):");
            int width;
            if(!int.TryParse(Console.ReadLine(), out width))
            {
                width = 100;
            }

            Console.WriteLine();
            Console.WriteLine("enter height for thumbnail(leave blank for standard:100):");
            int height;
            if(!int.TryParse(Console.ReadLine(), out height))
            {
                height = 100;
            }

            // Autentisera Computer Vision-klient
            ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(cogSvcKey);
            cvClient = new ComputerVisionClient(credentials)
            {
                Endpoint = cogSvcEndpoint
            };

            Console.Clear();

            // Analyze image
            await AnalyzeImage(imagePath);

            // Get thumbnail
            await GetThumbnail(imagePath, width, height);
        }

        private static async Task AnalyzeImage(string imagePath)
        {
            Console.WriteLine("Analyzing");

            // Specify features to be retrieved
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
              VisualFeatureTypes.Description,
              VisualFeatureTypes.Tags,
              VisualFeatureTypes.Objects,
            };

            using (var imageData = File.OpenRead(imagePath))
            {
                var analysis = await cvClient.AnalyzeImageInStreamAsync(imageData, features);


                // Description
                foreach (var caption in analysis.Description.Captions)
                {
                    Console.WriteLine($"Description: {caption.Text} (Confidence: {caption.Confidence.ToString("P")})");
                }

                // imageTags
                if (analysis.Tags.Count > 0)
                {
                    Console.WriteLine("Tags:");
                    foreach (var tag in analysis.Tags)
                    {
                        Console.WriteLine($" -{tag.Name} (Confidence: {tag.Confidence.ToString("P")})");
                    }
                }

                //Objects
                if (analysis.Objects.Count > 0)
                {
                    Console.WriteLine("Objects:");
                    foreach (var detectedObject in analysis.Objects)
                    {
                        Console.WriteLine($" -{detectedObject.ObjectProperty} (Confidence: {detectedObject.Confidence.ToString("P")})");
                    }
                }

            }
        }

        static async Task GetThumbnail(string imageFile, int width, int height)
        {
            Console.WriteLine("Generating thumbnail");

            // Thumbnail
            try
            {
                using (var imageData = File.OpenRead(imageFile))
                {
                    var thumbnailStream = await cvClient.GenerateThumbnailInStreamAsync(width, height, imageData, true);

                    // save thumbnail on desktop
                    string thumbnailFileName = "thumbnail.png";
                    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), thumbnailFileName);
                    using (Stream thumbnailFile = File.Create(path))
                    {
                        await thumbnailStream.CopyToAsync(thumbnailFile);  
                    }

                    Console.WriteLine($"Thumbnail saved on desktop.  path: {path}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

        }
    }
}
