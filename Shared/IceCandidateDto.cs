using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBC.Shared
{
    public class IceCandidateDto
    {
        public string SdpMid { get; set; }
        public int SdpMLineIndex { get; set; }
        public string Candidate { get; set; }
    }
}
