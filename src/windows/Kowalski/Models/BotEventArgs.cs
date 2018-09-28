using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kowalski.Models
{
    public class BotEventArgs : EventArgs
    {
        public string Text { get; }

        public BotEventArgs(string text)
        {
            Text = text;
        }
    }
}
