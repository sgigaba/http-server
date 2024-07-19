using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
 TcpListener server = new TcpListener(IPAddress.Any, 4221);
 server.Start();
// AcceptSocket() will block untill a client is connected. AcceptSocket returns a socket you can use to send and receive data
var socket = server.AcceptSocket();

string clientMessage = "HTTP/1.1 200 OK\r\n\r\n";
Byte[] sendBytes = Encoding.ASCII.GetBytes(clientMessage);

// Receive request from client
byte[] responseBytes = new byte[256];
socket.Receive(responseBytes);

string response = Encoding.ASCII.GetString(responseBytes);
Console.WriteLine("" + response);

string[] responseLines = response.Split('\n');

if (responseLines != null)
{
    string requestLine = responseLines.FirstOrDefault(_ => _.Contains("HTTP"));
    string headerLine = responseLines.FirstOrDefault(_ => _.Contains("User-Agent"));

    string requestTarget = requestLine.Split(' ')[1];
    var endpoint = requestTarget.Split('/');
    var userAgent = "";
    
    if (headerLine != null)
    {
        userAgent = headerLine.Split(':')[1];
    }

    string body="";
    string statusLine;
    string headerContentType ="";
    int headerContentLength = 0;

    switch(endpoint[1])
    {
        case "":
            statusLine = "200 OK";
            break;
        case "echo":
            statusLine = "200 OK";
            try{
                body = endpoint[2];
                headerContentType = "text/plain";
                headerContentLength = body.Length;
            }
            catch(IndexOutOfRangeException){

            }
            break;
        case "user-agent":
            statusLine = "200 OK";
            headerContentType = "text/plain";
            body = userAgent.Trim();
            headerContentLength = body.Length;
            break;
        default:
            statusLine = "404 Not Found";
            break;
    }

    socket.Send(Encoding.ASCII.GetBytes($"HTTP/1.1 {statusLine}\r\nContent-Type: {headerContentType}\r\nContent-Length: {headerContentLength}\r\n\r\n{body}"));
}
socket.Close();