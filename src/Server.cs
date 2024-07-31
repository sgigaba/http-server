using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
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
    string statusLine = requestLines.First(_ => _.Contains("HTTP")); 
    string[] endpoint = statusLine.Split(' ')[1].Split('/');

    switch(endpoint[1])
    {
        case "":
            DefaultEndpoint(socket);
            break;
        case "echo":
            EchoEndpoint(socket, requestLines, endpoint);
            break;
        case "user-agent":
            UserAgentEndpoint(socket, requestLines);
            break;
        case "files":
            FileEndpoint(socket, requestLines, endpoint);
            break;
        default:
            NotFound(socket);
            break;
    }
    return Task.CompletedTask;
}

void DefaultEndpoint(Socket socket) {
    socket.Send(Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n"));
    socket.Close();
} 

void EchoEndpoint(Socket socket, string[] requestLines, string[] endpoint)
{
  try{
        var encoding = requestLines.FirstOrDefault(_ => _.Contains("Accept-Encoding"));
        if (encoding != null && encoding.Contains("gzip"))
        {
            var compressedBody = CompressBody(endpoint[2]);
            var compressionResponse = 
                Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Encoding: gzip\r\nContent-Type: text/plain\r\nContent-Length: {compressedBody.Length}\r\n\r\n");

            socket.Send(compressionResponse);
            socket.Send(compressedBody);
            socket.Close();
        }
        else{
            socket.Send(Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {endpoint[2].Length}\r\n\r\n{endpoint[2]}"));
            socket.Close();
        }
    }
    catch(IndexOutOfRangeException){
        socket.Send(Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n"));
        socket.Close();
    }   
}

void FileEndpoint(Socket socket, string[] requestLines, string[] endpoint)
{
    string[] arg = Environment.GetCommandLineArgs();
    string requestMethod = requestLines.First(_ => _.Contains("HTTP")).Split(' ')[0]; 
    switch(requestMethod)
    {
        case "GET":
            if (File.Exists(arg[2] + endpoint[2])){
                var body = File.ReadAllText(arg[2] + endpoint[2]);
                socket.Send(Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {body.Length}\r\n\r\n{body}"));
                socket.Close();
            }
            else{
                socket.Send(Encoding.ASCII.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n"));
                socket.Close();
            }
        break;

        case "POST":
            var newFile = endpoint[2].Split(' ')[0];
            string contentLength = requestLines.First(_ => _.Contains("Content-Length")).Split(":")[1].Trim();
            int finalLength = 0;

            Int32.TryParse(contentLength, out finalLength);
            var data = requestLines.Last().Substring(0, finalLength);

            using (StreamWriter sw = File.CreateText(arg[2] + newFile)){
                sw.Write(data);
            }
            socket.Send(Encoding.UTF8.GetBytes($"HTTP/1.1 201 Created\r\n\r\n"));
            socket.Close();
        break;                                       
    }
}

void NotFound(Socket socket)
{
    socket.Send(Encoding.UTF8.GetBytes($"HTTP/1.1 404 Not Found\r\n\r\n"));
    socket.Close();
}

void UserAgentEndpoint(Socket socket, string[] requestLines)
{
    string? headerLine = requestLines.FirstOrDefault(_ => _.Contains("User-Agent"));
    var userAgent = "";

    if (headerLine != null)
    {
        userAgent = headerLine.Split(':')[1];
    }
    socket.Send(Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {userAgent.Trim().Length}\r\n\r\n{userAgent.Trim()}"));
    socket.Close();
}

byte[] CompressBody(string body)
{
    var bytesBody = Encoding.UTF8.GetBytes(body);
    byte[] compressedBody = null;

    using (var memoryStream = new MemoryStream())
    {
        using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress)){
            gzipStream.Write(bytesBody, 0,bytesBody.Length);
        }
        compressedBody = memoryStream.ToArray();
    }

    return compressedBody;
}
