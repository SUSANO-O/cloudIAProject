using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ImageTranslationAnalysis
{
    class Program
    {
        private static readonly string translatorKey = "9795e9e30bae45b2b5608fb6c902bd83";
        private static readonly string translatorEndpoint = "https://api.cognitive.microsofttranslator.com";
        private static readonly string location = "eastus";

        static async Task Main(string[] args)
        {
            try
            {
                // Cargar configuración desde 'appsettings.json'
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string aiSvcEndpoint = configuration["AIServicesEndpoint"];
                string aiSvcKey = configuration["AIServicesKey"];

                // Obtener imagen a analizar
                string imageFile = "images/paisaje.jpg";
                if (args.Length > 0)
                {
                    imageFile = args[0];
                }

                // Autenticación del cliente de Azure AI Vision
                ImageAnalysisClient client = new ImageAnalysisClient(
                    new Uri(aiSvcEndpoint),
                    new AzureKeyCredential(aiSvcKey));

                // Analizar la imagen
                string analysisResult = AnalyzeImage(imageFile, client);

                // Traducir el resultado al español
                string translatedText = await TranslateText(analysisResult, "en", "es");

                Console.WriteLine("\n📝 Resultado traducido al español:");
                Console.WriteLine(translatedText);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.ResetColor();
            }
        }

        static string AnalyzeImage(string imageFile, ImageAnalysisClient client)
        {
            Console.WriteLine("\n🔍 Análisis en proceso para: " + imageFile);

            using FileStream stream = new FileStream(imageFile, FileMode.Open);
            ImageAnalysisResult result = client.Analyze(
                BinaryData.FromStream(stream),
                VisualFeatures.Caption | VisualFeatures.Tags | VisualFeatures.Objects);

            string analysisDescription = "";

            // Subtítulo de la imagen
            if (result.Caption.Text != null)
            {
                analysisDescription += $"Subtítulo: \"{result.Caption.Text}\" (Confianza: {result.Caption.Confidence:0.00})\n";
            }

            // Etiquetas
            if (result.Tags.Values.Count > 0)
            {
                analysisDescription += "\nEtiquetas detectadas:\n";
                foreach (var tag in result.Tags.Values)
                {
                    analysisDescription += $"   - {tag.Name} (Confianza: {tag.Confidence:F2})\n";
                }
            }

            // Objetos detectados
            if (result.Objects.Values.Count > 0)
            {
                analysisDescription += "\nObjetos detectados:\n";
                foreach (var detectedObject in result.Objects.Values)
                {
                    analysisDescription += $"   - {detectedObject.Tags[0].Name}\n";
                }
            }

            return analysisDescription;
        }

        static async Task<string> TranslateText(string text, string fromLanguage, string toLanguage)
        {
            string route = $"/translate?api-version=3.0&from={fromLanguage}&to={toLanguage}";
            object[] body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(translatorEndpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", translatorKey);
                request.Headers.Add("Ocp-Apim-Subscription-Region", location);

                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync();

                // Extraer y retornar el texto traducido desde la respuesta JSON
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(result);
                string translatedText = jsonResponse[0]["translations"][0]["text"].ToString();
                return translatedText;
            }
        }
    }
}
