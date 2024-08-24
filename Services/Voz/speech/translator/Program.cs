using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text;

// Import namespaces
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;

namespace speech_translation
{
    class Program
    {
        private static SpeechConfig speechConfig;
        private static SpeechTranslationConfig translationConfig;

        static async Task Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string aiSvcKey = configuration["SpeechKey"];
                string aiSvcRegion = configuration["SpeechRegion"];

                // Set console encoding to unicode
                Console.InputEncoding = Encoding.Unicode;
                Console.OutputEncoding = Encoding.Unicode;

                // Configure translation
                translationConfig = SpeechTranslationConfig.FromSubscription(aiSvcKey, aiSvcRegion);
                translationConfig.SpeechRecognitionLanguage = "en-US";
                translationConfig.AddTargetLanguage("fr");
                translationConfig.AddTargetLanguage("es");
                translationConfig.AddTargetLanguage("hi");
                Console.WriteLine("Ready to translate from " + translationConfig.SpeechRecognitionLanguage);

                // Configure speech
                speechConfig = SpeechConfig.FromSubscription(aiSvcKey, aiSvcRegion);

                string targetLanguage = "";
                while (targetLanguage != "quit")
                {
                    Console.WriteLine("\nEnter a target language\n fr = French\n es = Spanish\n hi = Hindi\n Enter anything else to stop\n");
                    targetLanguage = Console.ReadLine().ToLower();

                    if (translationConfig.TargetLanguages.Contains(targetLanguage))
                    {
                        await Translate(targetLanguage);
                    }
                    else
                    {
                        Console.WriteLine("❌ Language not supported. Exiting...");
                        targetLanguage = "quit";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error during execution.");
                Console.WriteLine($"   ❗ Error message: {ex.Message}");
            }
        }

        static async Task Translate(string targetLanguage)
        {
            string translation = "";

            try
            {
                using AudioConfig audioConfig = AudioConfig.FromDefaultMicrophoneInput();
                using TranslationRecognizer translator = new TranslationRecognizer(translationConfig, audioConfig);
                
                Console.WriteLine("🎙️ Speak now...");
                TranslationRecognitionResult result = await translator.RecognizeOnceAsync();
                
                Console.WriteLine($"🔍 Translating '{result.Text}'");

                if (result.Reason == ResultReason.TranslatedSpeech)
                {
                    if (result.Translations.TryGetValue(targetLanguage, out translation))
                    {
                        Console.OutputEncoding = Encoding.UTF8;
                        Console.WriteLine($"   🌍 Translation: {translation}");
                    }
                    else
                    {
                        Console.WriteLine($"   ❌ Translation for '{targetLanguage}' not available.");
                    }
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine("   ❓ No match found.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    Console.WriteLine("   ❌ Translation canceled.");
                    Console.WriteLine($"     ❗ Reason: {cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"     ❗ Error code: {cancellation.ErrorCode}");
                        Console.WriteLine($"     ❗ Error details: {cancellation.ErrorDetails}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error during translation.");
                Console.WriteLine($"   ❗ Error message: {ex.Message}");
            }
        }
    }
}
