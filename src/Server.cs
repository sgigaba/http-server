using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
 TcpListener server = new TcpListener(IPAddress.Any, 4221);
 server.Start();
// AcceptSocket() will block untill a client is connected. 
// AcceptSocket returns a socket you can use to send and receive data
var socket = server.AcceptSocket();

string clientMessage = "HTTP/1.1 200 OK\r\n\r\n";
Byte[] sendBytes = Encoding.ASCII.GetBytes(clientMessage);


byte[] responseBytes = new byte[256];
char[] responseChars = new char[256];

int byteCount = socket.Receive(responseBytes);
Encoding.ASCII.GetChars(responseBytes, 0, byteCount, responseChars, 0);

Console.WriteLine(responseChars);

string request = ""; 
int i = 0;

while (responseChars[i] != '\r')
{
    request = request + responseChars[i];
    i++;
}

string requestTarget = request.Split(' ')[1];
string requestResponse;
switch(requestTarget)
{
    case "/":
        requestResponse = "200 OK";
        break;
    default:
        requestResponse = "404 Not Found";
        break;
}

socket.Send(Encoding.ASCII.GetBytes($"HTTP/1.1 {requestResponse}\r\n\r\n"));
socket.Close();