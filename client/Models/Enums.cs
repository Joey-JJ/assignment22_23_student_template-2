using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDP_FTP.Models
{
    public class Enums
    {
        public enum ErrorType 
        {
            NOERROR = 0,
            BADREQUEST = 1,
            CONNECTION_ERROR = 2
        }
        public enum Params : int
        {
            BUFFER_SIZE = 1000,
            WINDOW_SIZE = 5,
            SEGMENT_SIZE = 10
        }
        public enum Messages : int
        {
            HELLO = 1,
            HELLO_REPLY = 2,
            REQUEST = 3,
            REPLY = 4,
            DATA = 5,
            ACK = 6,
            CLOSE_REQUEST = 7,
            CLOSE_CONFIRM = 8
        }
    }
}
