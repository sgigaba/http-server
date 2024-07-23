using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
 TcpListener server = new TcpListener(IPAddress.Any, 4221);
 server.Start();

Socket socket; 

// Receive request from client
while (true)
{
    // AcceptSocket() will block untill a client is connected. AcceptSocket returns a socket you can use to send and receive data
    socket = server.AcceptSocket();
    
    byte[] responseBytes = new byte[256];
    socket.Receive(responseBytes);
    string response = Encoding.ASCII.GetString(responseBytes);
    Console.WriteLine("" + response);

    socket.Send(Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\n"));
    string[] responseLines = response.Split('\n');
    string requestLine = responseLines.FirstOrDefault(_ => _.Contains("HTTP"));
    string requestTarget = requestLine.Split(' ')[1];
    var endpoint = requestTarget.Split('/');
    
    SendStatusLine(endpoint[1]);
    if (responseLines != null)
    {
        string headerLine = responseLines.FirstOrDefault(_ => _.Contains("User-Agent"));
        var userAgent = "";

        if (headerLine != null)
        {
            userAgent = headerLine.Split(':')[1];
        }

        string body="";
        string headerContentType ="";
        int headerContentLength = 0;
        string finalResponse = "";

        switch(endpoint[1])
        {
            case "":
                body = "testing";
                headerContentType = "text/plain";
                headerContentLength = body.Length;
                SendResponse(headerContentLength,headerContentType,body);
                break;
            case "echo":
                try{
                    body = endpoint[2];
                    headerContentType = "text/plain";
                    headerContentLength = body.Length;
                    SendResponse(headerContentLength,headerContentType,body);
                }
                catch(IndexOutOfRangeException){

                }
                break;
            case "user-agent":
                headerContentType = "text/plain";
                body = userAgent.Trim();
                headerContentLength = body.Length;
                SendResponse(headerContentLength,headerContentType,body);
                break;
            default:
                SendResponse(headerContentLength,headerContentType,body);
                break;
        }
        Console.WriteLine(finalResponse);
        //socket.Send(Encoding.ASCII.GetBytes($"HTTP/1.1 {statusLine}\r\nContent-Type: {headerContentType}\r\nContent-Length: {headerContentLength}\r\n\r\n{body}"));
        socket.Close();
    }
}

void SendStatusLine(string endpoint)
{
    string statusLine = "";
    switch(endpoint)
    {
        case "":
            statusLine = "200 OK";
            break;
        case "echo":
            statusLine = "200 OK";
            break;
        case "user-agent":
            statusLine = "200 OK";
            break;
        default:
            statusLine = "404 Not Found";
            break;
    }
    socket.Send(Encoding.ASCII.GetBytes($"HTTP/1.1 {statusLine}\r\n"));
}

void SendResponse(int headerContentLength, string? headerContentType, string? body)
{
    var response = new StringBuilder();

    response.Append($"Content-Type: {headerContentType}\r\n");
    response.Append($"Content-Length: {headerContentLength}\r\n");
    response.Append($"\r\n{body}");

    socket.Send(Encoding.ASCII.GetBytes(response.ToString()));
    socket.Close();
}