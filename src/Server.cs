using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
 TcpListener server = new TcpListener(IPAddress.Any, 4221);
 server.Start();

while (true)
{
    Socket socket = server.AcceptSocket();
    Task.Run(() => ParseRequestAndSendResponse(socket));
}



Task ParseRequestAndSendResponse(Socket socket)
{
    byte[] responseBytes = new byte[256];
    socket.Receive(responseBytes);

    string response = Encoding.ASCII.GetString(responseBytes);
    string[] responseLines = response.Split('\n');
    string requestLine = responseLines.FirstOrDefault(_ => _.Contains("HTTP"));
    string requestTarget = requestLine.Split(' ')[1];
    var endpoint = requestTarget.Split('/');
    
    if (responseLines != null)
    {
        string headerLine = responseLines.FirstOrDefault(_ => _.Contains("User-Agent"));
        var userAgent = "";

        if (headerLine != null)
        {
            userAgent = headerLine.Split(':')[1];
        }
        switch(endpoint[1])
        {
            case "":
                SendResponse("200 OK","text/plain","testing", socket);
                break;
            case "echo":
                try{
                    SendResponse("200 OK","text/plain",endpoint[2], socket);
                }
                catch(IndexOutOfRangeException){
                    SendResponse("200 OK","text/plain","", socket);
                }
                break;
            case "user-agent":
                SendResponse("200 OK","text/plain",userAgent.Trim(), socket);
                break;
            case "files":
                try{
                    var filePath = GetFilePath();
                    var body = ReadFile(filePath,endpoint[2], socket);
                }
                catch (FileNotFoundException) 
                {
                    Console.WriteLine("File Not Found");
                }
                SendResponse("200 OK","application/octet-stream", "", socket);                                                 
                break;
            default:
                SendResponse("404 Not Found","", "", socket);
                break;
        }
    }
 
    return Task.CompletedTask;
}

void SendResponse(string statusLine,string? headerContentType, string? body, Socket socket)
{
    var response = new StringBuilder();

    response.Append($"HTTP/1.1 {statusLine}\r\n");
    response.Append($"Content-Type: {headerContentType}\r\n");
    response.Append($"Content-Length: {body?.Length}\r\n");
    response.Append($"\r\n{body}");

    socket.Send(Encoding.ASCII.GetBytes(response.ToString()));
    socket.Close();
}

string? ReadFile(string dir, string file, Socket socket)
{
    string line ="";
    StreamReader sr;
    try{
        sr = new StreamReader($"C:\\{dir}\\{file}");
        line = sr.ReadToEnd();
        sr.Close();

        return line;
    }
    catch{
        socket.Send(Encoding.ASCII.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n"));
        socket.Close();
        throw new FileNotFoundException(); 
    }
}

string GetFilePath()
{
    string flags = "";
    foreach(var arg in args)
    {
        flags += arg;
    }
    
    return flags.Split('/')[1];
}