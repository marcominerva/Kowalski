using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechSynthesis;

namespace Kowalski.Models
{
    public class Settings
    {
        public static Settings Instance { get; set; }

        public string AssistantName { get; set; }

        public string UserName { get; set; }

        public string DirectLineSecret { get; set; }

        public string BotId { get; set; }

        public string Culture { get; set; } = "it-IT";

        public VoiceGender VoiceGender { get; set; } = VoiceGender.Male;
    }
}
