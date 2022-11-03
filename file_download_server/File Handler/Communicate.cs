﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using UDP_FTP.Error_Handling;
using UDP_FTP.Models;
using static UDP_FTP.Models.Enums;

namespace UDP_FTP.File_Handler
{
    class Communicate
    {
        private const string Server = "MyServer";
        private string Client = "Client";
        private int SessionID;
        private Socket socket;
        private IPEndPoint remoteEndpoint;
        private EndPoint remoteEP;
        private ErrorType Status;
        private byte[] buffer;
        byte[] msg;
        private string file;
        ConSettings C;


        public Communicate()
        {
            // TODO: Initializes another instance of the IPEndPoint for the remote host
            remoteEP = (EndPoint)new IPEndPoint(IPAddress.Any, 5010);

            // TODO: Specify the buffer size
            buffer = new byte[(int)Enums.Params.BUFFER_SIZE];

            // TODO: Get a random SessionID
            SessionID = new Random().Next(1000, 9999);

            // TODO: Create local IPEndpoints and a Socket to listen 
            //       Keep using port numbers and protocols mention in the assignment description
            //       Associate a socket to the IPEndpoints to start the communication
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5004));
        }

        public ErrorType StartDownload()
        {
            // TODO: Instantiate and initialize different messages needed for the communication
            // required messages are: HelloMSG, RequestMSG, DataMSG, AckMSG, CloseMSG
            // Set attribute values for each class accordingly 
            HelloMSG greetBack = new HelloMSG();
            RequestMSG req = new RequestMSG();
            DataMSG data = new DataMSG();
            AckMSG ack = new AckMSG();
            CloseMSG cls = new CloseMSG();

            var conSettings = new ConSettings()
            {
                Type = Messages.HELLO,
                To = Server,
                From = Client,
                ConID = 1,
            };


            // TODO: Start the communication by receiving a HelloMSG message
            // Receive and deserialize HelloMSG message 
            // Verify if there are no errors
            // Type must match one of the ConSettings' types and receiver address must be the server address
            int dataSize;
            string data2;

            Console.WriteLine("\n Waiting for the next client message..");

            // Receive message
            dataSize = socket.ReceiveFrom(buffer, ref remoteEP);
            data2 = Encoding.ASCII.GetString(buffer, 0, dataSize);
            var hello = JsonSerializer.Deserialize<HelloMSG>(data2);
            Console.WriteLine("A message received from " + remoteEP.ToString() + " " + data);

            // Verify message
            var error = ErrorHandler.VerifyGreeting(hello, conSettings);
            if (error == 0) Console.WriteLine("No error..");
            else throw new Exception(error.ToString());

            // TODO: If no error is found then HelloMSG will be sent back
            greetBack.Type = Messages.HELLO_REPLY;
            greetBack.To = hello.From;
            greetBack.From = hello.To;
            greetBack.ConID = hello.ConID;

            msg = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(greetBack));
            socket.SendTo(msg, msg.Length, SocketFlags.None, remoteEP);

            // TODO: Receive the next message
            // Expected message is a download RequestMSG message containing the file name
            // Receive the message and verify if there are no errors
            dataSize = socket.ReceiveFrom(buffer, ref remoteEP);
            data2 = Encoding.ASCII.GetString(buffer, 0, dataSize);
            var requestMsg = JsonSerializer.Deserialize<RequestMSG>(data2);
            Console.WriteLine("A message received from " + remoteEP.ToString() + " " + data);
            conSettings = new ConSettings()
            {
                Type = Messages.REQUEST,
                To = Server,
                From = Client,
                ConID = 1,
            };

            error = ErrorHandler.VerifyRequest(requestMsg, conSettings);
            if (error == 0) Console.WriteLine("No error..");
            else throw new Exception(error.ToString());


            // TODO: Send a RequestMSG of type REPLY message to remoteEndpoint verifying the status
            req.To = requestMsg.From;
            req.From = requestMsg.To;
            req.ConID = requestMsg.ConID;
            req.Status = File.Exists($"{requestMsg.FileName}") ? ErrorType.NOERROR : ErrorType.BADREQUEST;
            req.FileName = requestMsg.FileName;

            msg = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(req));
            socket.SendTo(msg, msg.Length, SocketFlags.None, remoteEP);

            // TODO:  Start sending file data by setting first the socket ReceiveTimeout value
            socket.ReceiveTimeout = 1000;

            // TODO: Open and read the text-file first
            // Make sure to locate a path on windows and macos platforms
            var file = Encoding.ASCII.GetBytes(File.ReadAllText(requestMsg.FileName));

            // TODO: Sliding window with go-back-n implementation
            // Calculate the length of data to be sent
            // Send file-content as DataMSG message as long as there are still values to be sent
            // Consider the WINDOW_SIZE and SEGMENT_SIZE when sending a message  
            // Make sure to address the case if remaining bytes are less than WINDOW_SIZE
            //
            // Suggestion: while there are still bytes left to send,
            // first you send a full window of data
            // second you wait for the acks
            // then you start again.

            var segmentsSent = 0;
            var totalSegments = Math.Ceiling(file.Length / (double)Params.SEGMENT_SIZE);
            var bytesLeft = segmentsSent * (int)Params.SEGMENT_SIZE;

            Console.WriteLine(file.Length);

            while (segmentsSent < totalSegments)
            {
                byte[] dataToSend;
                for (var i = 0; i < (int)Params.WINDOW_SIZE; i++)
                {
                    if (totalSegments - segmentsSent <= 1)
                        dataToSend = new byte[file.Length % (int)Params.SEGMENT_SIZE + 1];

                    else
                        dataToSend = new byte[(int)Params.SEGMENT_SIZE];

                    for (int j = 0; j < dataToSend.Length; j++)
                    {

                        Console.WriteLine(j + segmentsSent * (int)Params.SEGMENT_SIZE);
                        dataToSend[j] = file[j + segmentsSent * (int)Params.SEGMENT_SIZE];
                    }

                    // Configure data msg
                    data.Type = Messages.DATA;
                    data.From = hello.To;
                    data.To = hello.From;
                    data.ConID = requestMsg.ConID;
                    data.Size = (int)Params.WINDOW_SIZE;
                    data.More = segmentsSent < totalSegments;
                    data.Sequence = segmentsSent;
                    data.Data = dataToSend;

                    // Send data msg
                    msg = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(data));
                    socket.SendTo(msg, msg.Length, SocketFlags.None, remoteEP);
                    segmentsSent++;

                    // Receive ACK msg
                    dataSize = socket.ReceiveFrom(buffer, ref remoteEP);
                    data2 = Encoding.ASCII.GetString(buffer, 0, dataSize);
                    var ackMSG = JsonSerializer.Deserialize<AckMSG>(data2);
                    Console.WriteLine("ACK: " + segmentsSent);

                    Console.WriteLine(segmentsSent + " - " + totalSegments);
                }
            }



            // TODO: Receive and verify the acknowledgements (AckMSG) of sent messages
            // Your client implementation should send an AckMSG message for each received DataMSG message   



            // TODO: Print each confirmed sequence in the console
            // receive the message and verify if there are no errors


            // TODO: Send a CloseMSG message to the client for the current session
            // Send close connection request
            cls.ConID = req.ConID;
            cls.From = req.To;
            cls.To = req.From;
            cls.Type = Messages.CLOSE_REQUEST;

            msg = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(cls));
            socket.SendTo(msg, msg.Length, SocketFlags.None, remoteEP);


            // TODO: Receive and verify a CloseMSG message confirmation for the current session
            // Get close connection confirmation
            // Receive the message and verify if there are no errors
            dataSize = socket.ReceiveFrom(buffer, ref remoteEP);
            data2 = Encoding.ASCII.GetString(buffer, 0, dataSize);
            var closeReply = JsonSerializer.Deserialize<CloseMSG>(data2);
            conSettings = new ConSettings()
            {
                Type = Messages.CLOSE_CONFIRM,
                To = requestMsg.From,
                From = requestMsg.To,
                ConID = requestMsg.ConID,
            };

            error = ErrorHandler.VerifyClose(closeReply, conSettings);
            if (error != 0) throw new Exception(error.ToString());

            Console.WriteLine("Group members: {0} | {1}", "student_1", "student_2");
            return ErrorType.NOERROR;
        }
    }
}
