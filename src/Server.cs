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

    Console.WriteLine(response);

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
                switch(requestType)
                {
                    case "GET":
                        try{
                            if (File.Exists(arg[2] + endpoint[2]))
                            {
                                var body = File.ReadAllText(arg[2]+endpoint[2]);
                                SendResponse("200 OK","application/octet-stream", body, socket);                                                 
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
                        var data = responseLines.Last();
                        using (StreamWriter sw = File.CreateText(arg[2] + newFile)){
                            sw.WriteLine(data.Trim());
                        }
                        SendResponse("201 Created","","" , socket);                                                 
                        socket.Close();
                    break;                                       
                }
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
