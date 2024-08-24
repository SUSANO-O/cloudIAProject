using System;
using System.Threading.Tasks;
using Azure;
using Azure.AI.TextAnalytics;

namespace TextAnalyticsExample
{
    class Program
    {
        // Clave API y endpoint directamente en el código
        private static readonly string apiKey = "0b89bbc292c341dd87e65c6ef1be8e8b";
        private static readonly string endpoint = "https://lenguajejarvis.cognitiveservices.azure.com/";

        private static readonly TextAnalyticsClient client;

        static Program()
        {
            try
            {
                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint))
                {
                    throw new InvalidOperationException("API key or endpoint is not set.");
                }

                var credential = new AzureKeyCredential(apiKey);
                client = new TextAnalyticsClient(new Uri(endpoint), credential);
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($"ArgumentNullException: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"InvalidOperationException: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        static async Task Main(string[] args)
        {
            try
            {
                string textToAnalyze = "I love the new features of the latest version!";

                Console.WriteLine("Analyzing sentiment...");
                var response = await client.AnalyzeSentimentAsync(textToAnalyze);

                // Obtención de los resultados del análisis
                var documentSentiment = response.Value;

                Console.WriteLine($"Sentiment: {documentSentiment.Sentiment}");
                Console.WriteLine($"Positive Score: {documentSentiment.ConfidenceScores.Positive}");
                Console.WriteLine($"Neutral Score: {documentSentiment.ConfidenceScores.Neutral}");
                Console.WriteLine($"Negative Score: {documentSentiment.ConfidenceScores.Negative}");
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"RequestFailedException: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}
