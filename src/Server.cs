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
    Task.Run(() => ParseRequestAndSendResponse(socket));
}



Task ParseRequestAndSendResponse(Socket socket)
{
    byte[] responseBytes = new byte[256];
    socket.Receive(responseBytes);

    string response = Encoding.ASCII.GetString(responseBytes);

    string[] responseLines = response.Split('\n');
    string requestLine = responseLines.FirstOrDefault(_ => _.Contains("HTTP"));
    string requestType = requestLine.Split(' ')[0]; 
    string requestTarget = requestLine.Split(' ')[1];
    var endpoint = requestTarget.Split('/');
    string[] arg = Environment.GetCommandLineArgs();

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
                SendResponse("200 OK","text/plain","testing", socket, null);
                break;
            case "echo":
                try{
                    var encoding = responseLines.FirstOrDefault(_ => _.Contains("Accept-Encoding"));
                    SendResponse("200 OK","text/plain",endpoint[2], socket, encoding);
                }
                catch(IndexOutOfRangeException){
                    SendResponse("200 OK","text/plain","", socket, null);
                }
                break;
            case "user-agent":
                SendResponse("200 OK","text/plain",userAgent.Trim(), socket, null);
                break;
            case "files":
                switch(requestType)
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
                        string contentLength = responseLines.FirstOrDefault(_ => _.Contains("Content-Length")).Split(":")[1].Trim();
                        int finalLength = 0;
                        Int32.TryParse(contentLength, out finalLength);
                        var data = responseLines.Last().Substring(0, finalLength);
                        Console.WriteLine(data);
                        using (StreamWriter sw = File.CreateText(arg[2] + newFile)){
                            sw.Write(data);
                        }
                        SendResponse("201 Created","","" , socket, null);                                                 
                        socket.Close();
                    break;                                       
                }
                break;
            default:
                SendResponse("404 Not Found","", "", socket, null);
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