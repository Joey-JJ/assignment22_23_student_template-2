using System.Linq.Expressions;
using System;
using System.Net;
using System.Net.Sockets;
using UDP_FTP.File_Handler;
using static UDP_FTP.Models.Enums;

namespace UDP_FTP
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: add the student number of your group members as a string value. 
            // string format example: "Jan Jansen 09123456" 
            // If a group has only one member fill in an empty string for the second student
            string student_1 = "";
            string student_2 = "";



            Console.WriteLine("Server is waiting for new request!");
            Communicate FileShare = new Communicate();
            Console.WriteLine("The file download request terminated with code {0}.", FileShare.StartDownload());

            Console.WriteLine("Group members: {0} | {1}", student_1, student_2);
        }
    }
}
