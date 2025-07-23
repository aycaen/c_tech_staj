using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp2
{
    internal class ZmqRequest
    {
        public string binPath { get; set; } = "";
        public string outputFolder { get; set; } = "";
        public ushort numChannels { get; set; } = 1;
        public uint sampleRate { get; set; } = 44100;
        public ushort bitsPerSample { get; set; } = 8;
        public int durationSec { get; set; } = 0;
        public bool isFromEnd { get; set; } = false;
    }
}
