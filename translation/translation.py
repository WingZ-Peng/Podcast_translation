from dotenv import load_dotenv
import os
import requests, json

def main():
    global translator_endpoint
    global cog_key
    global cog_region

    try:
        # Get Config Setting
        load_dotenv()
        cog_key = os.getenv('COG_SERVICE_KEY')
        cog_region = os.getenv('COG_SERVICE_REGION')
        translator_endpoint = 'https://api.cognitive.microsofttranslator.com'

        # Analyze each text file in the reviews folder
        reviews_folder = './reviews'
        translations_folder = './outputs'

        if not os.path.exists(translations_folder):
            os.makedirs(translations_folder)

        for file_name in os.listdir(reviews_folder):
            # read the file contents
            print('\n----------------------\n' + file_name)
            text = open(os.path.join(reviews_folder, file_name), encoding='utf8').read()
            print('\n' + text)

            # detect the language
            language = GetLanguage(text)
            print('Language:', language)

            # translate if not already English
            if language != 'zh-Hans':
                translation = Translate(text, language)
                print('\nTranslation:\n{}'.format(translation))

                # create a file path to save the translation and write the translation
                output_file_name = "translated_" + file_name
                translation_file_path = os.path.join(translations_folder, output_file_name)
                with open(translation_file_path, 'w', encoding='utf-8') as f:
                    f.write(translation)

    except Exception as ex:
        print(ex)


def GetLanguage(text):
    # default language is English
    language = 'en'

    # use the Translator detector function
    path = '/detect'
    url = translator_endpoint + path

    # build the request
    params = {
        'api-version': '3.0'
    }

    headers = {
        'Ocp-Apim-Subscription-Key': cog_key,
        'Ocp-Apim-Subscription-Region': cog_region,
        'Content-type': 'application/json'
    }

    body = [{
        'text': text
    }]

    # send the request and get response
    request = requests.post(url, params=params, headers=headers, json=body)
    response = request.json()
    print(response)

    # parse JSON array and get language
    language = response[0]['language']

    # return the language
    return language


def Translate(text, source_language):
    translation = ''

    # use the Translator translate function
    path = '/translate'
    url = translator_endpoint + path

    # Build the request
    params = {
        'api-version': '3.0',
        'from': source_language,
        'to': ['zh-Hans']
    }

    headers = {
        'Ocp-Apim-Subscription-Key': cog_key,
        'Ocp-Apim-Subscription-Region': cog_region,
        'Content-type': 'application/json'
    }

    body = [{
        'text': text
    }]

    # Send the request and get response
    request = requests.post(url, params=params, headers=headers, json=body)
    response = request.json()

    # Parse JSON array and get translation
    translation = response[0]["translations"][0]["text"]

    # return the translation
    return translation


if __name__ == '__main__':
    main()

