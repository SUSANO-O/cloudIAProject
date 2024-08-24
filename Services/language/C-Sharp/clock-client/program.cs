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
                Console.WriteLine("🔧 Configuración de la aplicación cargando...");

                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint))
                {
                    throw new InvalidOperationException("❗ API key o endpoint no están configurados.");
                }

                Console.WriteLine("🔑 Credenciales obtenidas y cliente inicializando...");
                var credential = new AzureKeyCredential(apiKey);
                client = new TextAnalyticsClient(new Uri(endpoint), credential);

                Console.WriteLine("✅ Cliente de Text Analytics inicializado correctamente.");
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($"🚫 ArgumentNullException: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"🚫 InvalidOperationException: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🚫 Exception: {ex.Message}");
                throw;
            }
        }

        static async Task Main(string[] args)
        {
            try
            {
                string textToAnalyze = "NO ME GUSTO NADA ESL SERVICIO JAMAS VOLVERE!";

                Console.WriteLine($"📝 Texto a analizar: \"{textToAnalyze}\"");
                Console.WriteLine("🔍 Iniciando análisis de sentimiento...");

                var response = await client.AnalyzeSentimentAsync(textToAnalyze);

                Console.WriteLine("✅ Análisis completado. Procesando resultados...");

                // Obtención de los resultados del análisis
                var documentSentiment = response.Value;

                Console.WriteLine($"💡 Sentimiento detectado: {documentSentiment.Sentiment}");
                Console.WriteLine($"👍 Puntaje positivo: {documentSentiment.ConfidenceScores.Positive}");
                Console.WriteLine($"😐 Puntaje neutral: {documentSentiment.ConfidenceScores.Neutral}");
                Console.WriteLine($"👎 Puntaje negativo: {documentSentiment.ConfidenceScores.Negative}");
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"🚫 RequestFailedException: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🚫 Exception: {ex.Message}");
            }
        }
    }
}
