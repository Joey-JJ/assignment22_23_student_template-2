using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using UDP_FTP.Models;
using UDP_FTP.Error_Handling;
using static UDP_FTP.Models.Enums;

using System.Text.Json.Serialization;

namespace Client
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
            _ = student_1 + student_2;

            byte[] buffer = new byte[1000];
            byte[] msg = new byte[100];
            Socket sock;
            int b;
            string data;

            // TODO: Initialise the socket/s as needed from the description of the assignment

            HelloMSG h = new HelloMSG()
            {
                Type = Messages.HELLO,
                To = "Server",
                From = "Client",
                ConID = 1,
            };

            RequestMSG r = new RequestMSG();
            DataMSG D = new DataMSG();
            AckMSG ack = new AckMSG();
            CloseMSG cls = new CloseMSG();

            var ServerEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5004);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remoteEP = (EndPoint)sender;

            // try
            // {
            // TODO: Instantiate and initialize your socket 
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // TODO: Send hello mesg
            msg = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(h));
            sock.SendTo(msg, msg.Length, SocketFlags.None, ServerEndpoint);

            // TODO: Receive and verify a HelloMSG 
            b = sock.ReceiveFrom(buffer, ref remoteEP);
            data = Encoding.ASCII.GetString(buffer, 0, b);
            var helloReply = JsonSerializer.Deserialize<HelloMSG>(data);
            Console.WriteLine("Server said: " + data);

            // TODO: VERIFY HELLO REPLY

            // TODO: Send the RequestMSG message requesting to download a file name
            r.Type = Messages.REQUEST;
            r.To = helloReply.From;
            r.From = helloReply.To;
            r.FileName = "test.txt";
            r.ConID = helloReply.ConID;

            msg = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(r));
            sock.SendTo(msg, msg.Length, SocketFlags.None, ServerEndpoint);

            // TODO: Receive a RequestMSG from remoteEndpoint
            // receive the message and verify if there are no errors
            // b = sock.ReceiveFrom(buffer, ref remoteEP);
            // data = Encoding.ASCII.GetString(buffer, 0, b);
            // Console.WriteLine("Server said: " + data);

            // TODO: Check if there are more DataMSG messages to be received 
            // receive the message and verify if there are no errors

            // TODO: Send back AckMSG for each received DataMSG 


            // TODO: Receive close message
            // receive the message and verify if there are no errors

            // TODO: confirm close message

            // }
            // catch
            // {
            //     Console.WriteLine("\n Socket Error. Terminating");
            // }

            Console.WriteLine("Download Complete!");

        }
    }
}
