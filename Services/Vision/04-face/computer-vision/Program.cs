using System;
using System.Drawing;
using Microsoft.Extensions.Configuration;
using Azure;
using System.IO;
using Azure.AI.Vision.Common;
using Azure.AI.Vision.ImageAnalysis;

namespace detect_people
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                Console.WriteLine("🔧 Configuración de la aplicación cargando...");
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string aiSvcEndpoint = configuration["AIServicesEndpoint"];
                string aiSvcKey = configuration["AIServiceKey"];
                Console.WriteLine("🔑 Configuración de la aplicación cargada exitosamente.");

                // Get image
                string imageFile = "images/people.jpg";
                if (args.Length > 0)
                {
                    imageFile = args[0];
                }
                Console.WriteLine($"🖼️ Imagen seleccionada: {imageFile}");

                // Authenticate Azure AI Vision client
                Console.WriteLine("🔐 Autenticando cliente de Azure AI Vision...");
                var cvClient = new VisionServiceOptions(
                    aiSvcEndpoint,
                    new AzureKeyCredential(aiSvcKey));
                Console.WriteLine("✅ Cliente autenticado correctamente.");

                // Analyze image
                AnalyzeImage(imageFile, cvClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

static void AnalyzeImage(string imageFile, VisionServiceOptions serviceOptions)
{
    Console.WriteLine($"\n🔍 Analizando la imagen: {imageFile} \n");

    var analysisOptions = new ImageAnalysisOptions()
    {
        Features = ImageAnalysisFeature.People
    };

    try
    {
        // Get image analysis
        using var imageSource = VisionSource.FromFile(imageFile);
        using var analyzer = new ImageAnalyzer(serviceOptions, imageSource, analysisOptions);

        Console.WriteLine($"🔧 Analyzer: {analyzer}");

        var result = analyzer.Analyze();

        Console.WriteLine($"🔧 Result Reason: {result.Reason}");

        if (result.Reason == ImageAnalysisResultReason.Analyzed)
        {
            if (result.People != null)
            {
                Console.WriteLine($"👥 Personas detectadas:");
                System.Drawing.Image image = System.Drawing.Image.FromFile(imageFile);
                Graphics graphics = Graphics.FromImage(image);
                Pen pen = new Pen(Color.Cyan, 3);
                Font font = new Font("Arial", 16);
                SolidBrush brush = new SolidBrush(Color.WhiteSmoke);

                foreach (var person in result.People)
                {
                    if (person.Confidence > 0.5)
                    {
                        var r = person.BoundingBox;
                        Rectangle rect = new Rectangle(r.X, r.Y, r.Width, r.Height);
                        graphics.DrawRectangle(pen, rect);

                        Console.WriteLine($"   🟦 Bounding box: {person.BoundingBox}, 🟢 Confianza: {person.Confidence:0.0000}");
                    }
                }

                // Save annotated image
                String output_file = "detected_people.jpg";
                image.Save(output_file);
                Console.WriteLine($"💾 Resultados guardados en: {output_file}\n");
            }
        }
        else
        {
            var errorDetails = ImageAnalysisErrorDetails.FromResult(result);
            Console.WriteLine("❌ Análisis fallido.");
            Console.WriteLine($"   ❗ Razón del error: {errorDetails.Reason}");
            Console.WriteLine($"   ❗ Código del error: {errorDetails.ErrorCode}");
            Console.WriteLine($"   ❗ Mensaje del error: {errorDetails.Message}\n");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error al analizar la imagen.");
        Console.WriteLine($"   ❗ Mensaje del error: {ex.Message}\n");
    }
}

    }
}
