using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Azure.AI.Vision;
using Azure.AI.ComputerVision.Models;
using Azure.Identity;

namespace detect_fruit
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Load configuration
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();

                string aiSvcEndpoint = configuration["AIServicesEndpoint"];
                string aiSvcKey = configuration["AIServiceKey"];
                string projectId = configuration["ProjectId"];
                string modelName = configuration["ModelName"];

                // Initialize the CustomVisionPredictionClient
                var client = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(aiSvcKey))
                {
                    Endpoint = aiSvcEndpoint
                };

                // Specify the image to analyze
                string imageFilePath = "path/to/your/image.jpg";
                using (var stream = new FileStream(imageFilePath, FileMode.Open))
                {
                    var result = client.ClassifyImage(projectId, modelName, stream);
                    
                    // Output the results
                    foreach (var prediction in result.Predictions)
                    {
                        Console.WriteLine($"Tag: {prediction.TagName}, Probability: {prediction.Probability}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
