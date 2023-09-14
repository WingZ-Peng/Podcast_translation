import os
import azure.cognitiveservices.speech as speechsdk

azure_key = "Efc892dc0d0f84cb08a519c24b077f78b"
azure_location = "eastasia"
text_file = "../Shakespeare.txt"
wave_file = "../data/Shakespeare.wav"

def recognize_speech():
    if os.path.exists(wave_file):
        print("Speech recognition started.")
        speech_config = speechsdk.SpeechConfig(subscription=azure_key, region=azure_location)
        audio_config = speechsdk.AudioConfig(filename=wave_file)
        speech_recognizer = speechsdk.SpeechRecognizer(speech_config=speech_config, audio_config=audio_config)

        result = speech_recognizer.recognize_once()

        if result.reason == speechsdk.ResultReason.RecognizedSpeech:
            with open(text_file, 'w', encoding='utf-8') as file:
                file.write(result.text)
            print("Speech recognition stopped.")
        elif result.reason == speechsdk.ResultReason.NoMatch:
            print("No speech could be recognized.")
        elif result.reason == speechsdk.ResultReason.Canceled:
            cancellation = result.cancellation_details
            print(f"Speech recognition canceled: {cancellation.reason}")
            if cancellation.reason == speechsdk.CancellationReason.Error:
                print(f"Error details: {cancellation.error_details}")
    else:
        print(f"{wave_file} does not exist.")

if __name__ == "__main__":
    try:
        recognize_speech()
    except Exception as ex:
        print(ex)























