using System.Buffers;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

while (true)
{
    Socket socket = server.AcceptSocket();
    Task.Run(() => ClientHandler(socket));
}

Task ClientHandler(Socket socket)
{
    byte[] clientBuffer = new byte[256];
    socket.Receive(clientBuffer);

    string clientRequest = Encoding.ASCII.GetString(clientBuffer);

    string[] requestLines = clientRequest.Split('\n');
    string statusLine = requestLines.FirstOrDefault(_ => _.Contains("HTTP")); // Request Line
    string requestMethod = statusLine.Split(' ')[0]; // Get or Post 
    string[] endpoint = statusLine.Split(' ')[1].Split('/');
    string[] arg = Environment.GetCommandLineArgs();

    if (requestLines != null)
    {
        string headerLine = requestLines.FirstOrDefault(_ => _.Contains("User-Agent"));
        var userAgent = "";

        if (headerLine != null)
        {
            userAgent = headerLine.Split(':')[1];
        }
        switch(endpoint[1])
        {
            case "":
                DefaultEndpoint(socket);
                break;
            case "echo":
                EchoEndpoint(socket, requestLines, endpoint);
                break;
            case "user-agent":
                UserAgentEndpoint(socket, userAgent);
                break;
            case "files":
                FileEndpoint(requestMethod, arg, endpoint, socket, requestLines);
                break;
            default:
                NotFound(socket);
                break;
        }
    }
 
    return Task.CompletedTask;
}

void SendResponse(string statusLine,string? headerContentType, string? body, Socket socket, string? encoding)
{
    var bytesBody = Encoding.UTF8.GetBytes(body);
    byte[] compressedBody = null;

    if (encoding != null && encoding.Contains("gzip"))
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress)){
                gzipStream.Write(bytesBody, 0,bytesBody.Length);
            }

            compressedBody = memoryStream.ToArray();
            body = Encoding.UTF8.GetString(compressedBody);
        }
        var compressionResponse = Encoding.UTF8.GetBytes($"HTTP/1.1 {statusLine}\r\nContent-Encoding: gzip\r\nContent-Type: {headerContentType}\r\nContent-Length: {compressedBody.Length}\r\n\r\n");
        socket.Send(compressionResponse);
        socket.Send(compressedBody);
        socket.Close();
    }
    else{
        socket.Send(Encoding.UTF8.GetBytes($"HTTP/1.1 {statusLine}\r\nContent-Type: {headerContentType}\r\nContent-Length: {body.Length}\r\nContent-Length: {body.Length}\r\n\r\n{body}"));
        socket.Close();
    }
}

void DefaultEndpoint(Socket socket) => SendResponse("200 OK","text/plain","", socket, null);

void EchoEndpoint(Socket socket, string[] requestLines, string[] endpoint)
{
  try{
        var encoding = requestLines.FirstOrDefault(_ => _.Contains("Accept-Encoding"));
        SendResponse("200 OK","text/plain",endpoint[2], socket, encoding);
    }
    catch(IndexOutOfRangeException){
        SendResponse("200 OK","text/plain","", socket, null);
    }   
}

void FileEndpoint(string requestMethod, string[] arg, string[] endpoint, Socket socket, string[] requestLines)
{
    switch(requestMethod)
    {
        case "GET":
            try{
                if (File.Exists(arg[2] + endpoint[2]))
                {
                    var body = File.ReadAllText(arg[2]+endpoint[2]);
                    SendResponse("200 OK","application/octet-stream", body, socket, null);                                                 
                }
                else{
                    socket.Send(Encoding.ASCII.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n"));
                    socket.Close();
                }
            }
            catch (FileNotFoundException) 
            {
                Console.WriteLine("File Not Found");
            }
        break;

        case "POST":
            var newFile = endpoint[2].Split(' ')[0];
            string contentLength = requestLines.FirstOrDefault(_ => _.Contains("Content-Length")).Split(":")[1].Trim();
            int finalLength = 0;
            Int32.TryParse(contentLength, out finalLength);
            var data = requestLines.Last().Substring(0, finalLength);
            Console.WriteLine(data);
            using (StreamWriter sw = File.CreateText(arg[2] + newFile)){
                sw.Write(data);
            }
            SendResponse("201 Created","","" , socket, null);                                                 
            socket.Close();
        break;                                       
    }
}

void NotFound(Socket socket)
{
    SendResponse("404 Not Found","", "", socket, null);
}

void UserAgentEndpoint(Socket socket, string userAgent)
{
    SendResponse("200 OK","text/plain",userAgent.Trim(), socket, null);
}