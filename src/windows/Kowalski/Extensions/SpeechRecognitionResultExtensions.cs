using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;

namespace Kowalski.Extensions
{
    public static class SpeechRecognitionResultExtensions
    {
        public static string NormalizeText(this SpeechRecognitionResult result)
            => result.Text.EndsWith("?") ? result.Text : $"{result.Text}?";
    }
}
