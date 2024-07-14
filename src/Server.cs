using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
 TcpListener server = new TcpListener(IPAddress.Any, 4221);
 server.Start();
 server.AcceptSocket(); // wait for client

// AcceptSocket() will block untill a client is connected. Once a client has connected the print statement below will execute

Console.WriteLine("Client Connectected");

// AcceptSocket returns a socket you can use to send and receive data
server.AcceptSocket().Send(Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n"));