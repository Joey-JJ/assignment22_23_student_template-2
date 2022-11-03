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
                To = "MyServer",
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

            try
            {
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
                var conSettings = new ConSettings()
                {
                    Type = Messages.HELLO_REPLY,
                    To = "Client",
                    From = "MyServer",
                    ConID = helloReply.ConID,
                };

                var error = ErrorHandler.VerifyGreetingReply(helloReply, conSettings);
                if (error != 0) throw new Exception(error.ToString());

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
                b = sock.ReceiveFrom(buffer, ref remoteEP);
                data = Encoding.ASCII.GetString(buffer, 0, b);
                var reqReply = JsonSerializer.Deserialize<RequestMSG>(data);


                Console.WriteLine("Server said: " + data);

                if (reqReply.Status != 0)
                {
                    Console.WriteLine("Error: " + reqReply.Status + ", File can not be found.");
                    return;
                }


                // TODO: Check if there are more DataMSG messages to be received 
                // receive the message and verify if there are no errors

                DataMSG dataMSG;

                var transferring = true;
                while (transferring)
                {
                    // Receive data msg and decode it
                    b = sock.ReceiveFrom(buffer, ref remoteEP);
                    data = Encoding.ASCII.GetString(buffer, 0, b);
                    dataMSG = JsonSerializer.Deserialize<DataMSG>(data);

                    // Stop receiving once last packet is received
                    if (!dataMSG.More) transferring = false;

                    // Logging received data
                    Console.WriteLine($"DATA RECEIVED: {data}");

                    // Configuring ACK msg
                    ack.ConID = dataMSG.ConID;
                    ack.From = dataMSG.To;
                    ack.To = dataMSG.From;
                    ack.Type = Messages.ACK;
                    ack.Sequence = dataMSG.Sequence;

                    // Sending ACK msg
                    msg = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ack));
                    sock.SendTo(msg, msg.Length, SocketFlags.None, ServerEndpoint);
                }

                // TODO: Receive close message
                // receive the message and verify if there are no errors
                b = sock.ReceiveFrom(buffer, ref remoteEP);
                data = Encoding.ASCII.GetString(buffer, 0, b);
                var closeMSG = JsonSerializer.Deserialize<CloseMSG>(data);
                Console.WriteLine($"CLOSE: {data}");

                cls.ConID = closeMSG.ConID;
                cls.From = closeMSG.To;
                cls.To = closeMSG.From;
                cls.Type = Messages.CLOSE_CONFIRM;

                // TODO: confirm close message
                msg = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(cls));
                sock.SendTo(msg, msg.Length, SocketFlags.None, ServerEndpoint);
            }
            catch
            {
                Console.WriteLine("\nSocket Error. Terminating");
            }

            Console.WriteLine("Download Complete!");

        }
    }
}
