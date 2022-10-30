using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UDP_FTP.Models.Enums;

namespace UDP_FTP.Models
{
    public class DataMSG
    {
        public Messages Type { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public int ConID { get; set; }
        public int Size { get; set; }
        public bool More { get; set; }
        public int Sequence { get; set; }
        public byte[] Data { get; set; }

    }
}
