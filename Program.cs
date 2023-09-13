using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

string azureKey = "fc892dc0d0f84cb08a519c24b077f78b";
string azureLocation = "eastasia";
string waveFile = "../data/test.wav";
string fileNameWithoutPath = Path.GetFileName(waveFile);
string outFileName = Path.ChangeExtension(fileNameWithoutPath, "txt");
string textFile = Path.Combine("../reviews", outFileName);

// single identification
// try
// {
//     FileInfo fileInfo = new FileInfo(waveFile);
//     if (fileInfo.Exists)
//     {
//         Console.WriteLine("Speech recognition started.");
//         var speechConfig = SpeechConfig.FromSubscription(azureKey, azureLocation);
//         using var audioConfig = AudioConfig.FromWavFileInput(fileInfo.FullName);
//         using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
//         var result = await speechRecognizer.RecognizeOnceAsync();

//         FileStream fileStream = File.OpenWrite(textFile);
//         StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
//         streamWriter.WriteLine(result.Text);
//         streamWriter.Close();
//         Console.WriteLine("Speech recognition stopped.");
//     }
// }
// catch (Exception ex)
// {
//     Console.WriteLine(ex.Message);
// }

// continuous identification 
try
{
    FileInfo fileInfo = new FileInfo(waveFile);
    if (fileInfo.Exists)
    {
        var speechConfig = SpeechConfig.FromSubscription(azureKey, azureLocation);
        using var audioConfig = AudioConfig.FromWavFileInput(fileInfo.FullName);
        using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
        var stopRecognition = new TaskCompletionSource<int>();

        FileStream fileStream = File.OpenWrite(textFile);
        StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

        speechRecognizer.Recognized += (s, e) =>
        {
            switch(e.Result.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    streamWriter.WriteLine(e.Result.Text);
                    break;
                case ResultReason.NoMatch:
                    Console.WriteLine("Speech could not be recognized.");
                    break;
            }
        };

        speechRecognizer.Canceled += (s, e) =>
        {
            if (e.Reason != CancellationReason.EndOfStream)
            {
                Console.WriteLine("Speech recognition canceled.");
            }
            stopRecognition.TrySetResult(0);
            streamWriter.Close();
        };

        speechRecognizer.SessionStopped += (s, e) =>
        {
            Console.WriteLine("Speech recognition stopped.");
            stopRecognition.TrySetResult(0);
            streamWriter.Close();
        };

        Console.WriteLine("Speech recognition started.");
        await speechRecognizer.StartContinuousRecognitionAsync();
        Task.WaitAny(new[] { stopRecognition.Task });
        await speechRecognizer.StopContinuousRecognitionAsync();
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}