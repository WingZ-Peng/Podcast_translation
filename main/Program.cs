using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;

class Program {
    static async Task Main(string[] args) {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string azureKey = configuration["AzureKey"];
        string azureLocation = configuration["AzureLocation"];
        string inputFolder = configuration["InputFolder"]; 
        string outputFolder = configuration["OutputFolder"];
        int maxRetries = int.Parse(configuration["MaxRetries"]);

        foreach (var waveFile in Directory.EnumerateFiles(inputFolder, "*.wav")) {
            string fileNameWithoutPath = Path.GetFileName(waveFile);
            string outFileName = Path.ChangeExtension(fileNameWithoutPath, "txt");
            string textFile = Path.Combine(outputFolder, outFileName);

            if (File.Exists(textFile)) {
                int counter = 1;
                while (File.Exists(Path.Combine(outputFolder, $"_{counter}_" + outFileName))) {
                    counter++;
                }
                textFile = Path.Combine(outputFolder, $"_{counter}_" + outFileName);
            }

            int retries = 0;
            bool success = false;

            while (retries <  maxRetries && !success) {
                try {
                    FileInfo fileInfo = new FileInfo(waveFile);
                    if (fileInfo.Exists) {
                        var speechConfig = SpeechConfig.FromSubscription(azureKey, azureLocation);
                        using var audioConfig = AudioConfig.FromWavFileInput(fileInfo.FullName);
                        using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
                        var stopRecognition = new TaskCompletionSource<int>();

                        // 使用using确保能正确地关闭和释放
                        using FileStream fileStream = File.OpenWrite(textFile);
                        using StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

                        speechRecognizer.Recognized += (s, e) => {
                            switch(e.Result.Reason) {
                                case ResultReason.RecognizedSpeech:
                                    streamWriter.WriteLine(e.Result.Text);
                                    break;
                                case ResultReason.NoMatch:
                                    Console.WriteLine($"No match for file {fileNameWithoutPath}");
                                    break;
                            }
                        };

                        speechRecognizer.Canceled += (s, e) => {
                            if (e.Reason != CancellationReason.EndOfStream) {
                                Console.WriteLine($"Speech recognition canceled for file {fileNameWithoutPath}");
                            }
                            stopRecognition.TrySetResult(0);
                        };

                        speechRecognizer.SessionStopped += (s, e) => {
                            Console.WriteLine($"Speech recognition stopped for file {fileNameWithoutPath}");
                            stopRecognition.TrySetResult(0);
                        };

                        Console.WriteLine($"Speech recognition started for file {fileNameWithoutPath}");
                        await speechRecognizer.StartContinuousRecognitionAsync();
                        Task.WaitAny(new[] { stopRecognition.Task });
                        await speechRecognizer.StopContinuousRecognitionAsync();

                        success = true;
                    }
                }

                catch (Exception ex) {
                    retries++;
                    Console.WriteLine($"An error occurred for file {fileNameWithoutPath}: {ex.Message}. Retrying ({retries}/{maxRetries})...");
                }
            }

            if (!success) {
                Console.WriteLine($"Failed to process file {fileNameWithoutPath} after {maxRetries} attempts.");
            }
        }
    }
}