//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// Microsoft Cognitive Services (formerly Project Oxford): https://www.microsoft.com/cognitive-services
//
// Microsoft Cognitive Services (formerly Project Oxford) GitHub:
// https://github.com/Microsoft/Cognitive-Speech-TTS
//
// Copyright (c) Microsoft Corporation
// All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;

namespace Kowalski
{
    /// <summary>
    /// Inputs Options for the TTS Service.
    /// </summary>
    public class InputOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Input"/> class.
        /// </summary>
        public InputOptions(string appName, string appId, string clientId)
        {
            AppName = appName;
            AppId = appId.Replace("-", string.Empty);
            ClientId = clientId.Replace("-", string.Empty);

            Locale = "en-us";
            VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)";
            // Default to Riff16Khz16BitMonoPcm output format.
            OutputFormat = AudioOutputFormat.Riff16Khz16BitMonoPcm;
        }

        public string AppName { get; }

        public string AppId { get; }

        public string ClientId { get; }

        /// <summary>
        /// Gets or sets the request URI.
        /// </summary>
        public Uri RequestUri { get; set; }

        /// <summary>
        /// Gets or sets the audio output format.
        /// </summary>
        public AudioOutputFormat OutputFormat { get; set; }

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get
            {
                var toReturn = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("Content-Type", "application/ssml+xml")
                    };

                string outputFormat;
                switch (OutputFormat)
                {
                    case AudioOutputFormat.Raw16Khz16BitMonoPcm:
                        outputFormat = "raw-16khz-16bit-mono-pcm";
                        break;

                    case AudioOutputFormat.Raw8Khz8BitMonoMULaw:
                        outputFormat = "raw-8khz-8bit-mono-mulaw";
                        break;

                    case AudioOutputFormat.Riff16Khz16BitMonoPcm:
                        outputFormat = "riff-16khz-16bit-mono-pcm";
                        break;

                    case AudioOutputFormat.Riff8Khz8BitMonoMULaw:
                        outputFormat = "riff-8khz-8bit-mono-mulaw";
                        break;

                    case AudioOutputFormat.Ssml16Khz16BitMonoSilk:
                        outputFormat = "ssml-16khz-16bit-mono-silk";
                        break;

                    case AudioOutputFormat.Raw16Khz16BitMonoTrueSilk:
                        outputFormat = "raw-16khz-16bit-mono-truesilk";
                        break;

                    case AudioOutputFormat.Ssml16Khz16BitMonoTts:
                        outputFormat = "ssml-16khz-16bit-mono-tts";
                        break;

                    case AudioOutputFormat.Audio16Khz128KBitRateMonoMp3:
                        outputFormat = "audio-16khz-128kbitrate-mono-mp3";
                        break;

                    case AudioOutputFormat.Audio16Khz64KBitRateMonoMp3:
                        outputFormat = "audio-16khz-64kbitrate-mono-mp3";
                        break;

                    case AudioOutputFormat.Audio16Khz32KBitRateMonoMp3:
                        outputFormat = "audio-16khz-32kbitrate-mono-mp3";
                        break;

                    case AudioOutputFormat.Audio16Khz16KbpsMonoSiren:
                        outputFormat = "audio-16khz-16kbps-mono-siren";
                        break;

                    case AudioOutputFormat.Riff16Khz16KbpsMonoSiren:
                        outputFormat = "riff-16khz-16kbps-mono-siren";
                        break;

                    case AudioOutputFormat.Raw24Khz16BitMonoPcm:
                        outputFormat = "raw-24khz-16bit-mono-pcm";
                        break;

                    case AudioOutputFormat.Riff24Khz16BitMonoPcm:
                        outputFormat = "riff-24khz-16bit-mono-pcm";
                        break;

                    case AudioOutputFormat.Audio24Khz48KBitRateMonoMp3:
                        outputFormat = "audio-24khz-48kbitrate-mono-mp3";
                        break;

                    case AudioOutputFormat.Audio24Khz96KBitRateMonoMp3:
                        outputFormat = "audio-24khz-96kbitrate-mono-mp3";
                        break;

                    case AudioOutputFormat.Audio24Khz160KBitRateMonoMp3:
                        outputFormat = "audio-24khz-160kbitrate-mono-mp3";
                        break;

                    default:
                        outputFormat = "riff-16khz-16bit-mono-pcm";
                        break;
                }

                toReturn.Add(new KeyValuePair<string, string>("X-Microsoft-OutputFormat", outputFormat));
                // authorization Header
                toReturn.Add(new KeyValuePair<string, string>("Authorization", AuthorizationToken));
                toReturn.Add(new KeyValuePair<string, string>("X-Search-AppId", AppId));
                toReturn.Add(new KeyValuePair<string, string>("X-Search-ClientID", ClientId));
                // The software originating the request
                toReturn.Add(new KeyValuePair<string, string>("User-Agent", "Kowalski"));

                return toReturn;
            }
            set
            {
                Headers = value;
            }
        }

        /// <summary>
        /// Gets or sets the locale.
        /// </summary>
        public String Locale { get; set; }

        /// <summary>
        /// Gets or sets the type of the voice; male/female.
        /// </summary>
        public Gender VoiceType { get; set; }

        /// <summary>
        /// Gets or sets the name of the voice.
        /// </summary>
        public string VoiceName { get; set; }

        /// <summary>
        /// Authorization Token.
        /// </summary>
        public string AuthorizationToken { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text { get; set; }
    }
}