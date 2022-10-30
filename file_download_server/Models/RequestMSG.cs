using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UDP_FTP.Models.Enums;

namespace UDP_FTP.Models
{
    public class RequestMSG
    {
        public Messages Type { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string FileName { get; set; }
        public int ConID { get; set; }
        public ErrorType Status { get; set; }
    }
}
