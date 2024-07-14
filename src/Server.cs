using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
 TcpListener server = new TcpListener(IPAddress.Any, 4221);
 server.Start();
// AcceptSocket() will block untill a client is connected. 
var socket = server.AcceptSocket();

string clientMessage = "HTTP/1.1 200 OK\r\n\r\n";
Byte[] sendBytes = Encoding.ASCII.GetBytes(clientMessage);

socket.Send(sendBytes);

// AcceptSocket returns a socket you can use to send and receive data