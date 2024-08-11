[![progress-banner](https://backend.codecrafters.io/progress/http-server/6ea47dee-3ee4-4050-a7aa-df154397b822)](https://app.codecrafters.io/users/codecrafters-bot?r=2qF)

This is a starting point for C# solutions to the
["Build Your Own HTTP server" Challenge](https://app.codecrafters.io/courses/http-server/overview).

[HTTP](https://en.wikipedia.org/wiki/Hypertext_Transfer_Protocol) is the
protocol that powers the web. In this challenge, you'll build a HTTP/1.1 server
that is capable of serving multiple clients.

**Note**: If you're viewing this repo on GitHub, head over to
[codecrafters.io](https://codecrafters.io) to try the challenge.

# Running this program

dotnet 8 needs to be available locally. <br/>
run **dotnet run** to start server

# Available endpoints that can be tested using curl:

**1. Default Endpoint : http://localhost:4221/ Should Return:**

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; HTTP/1.1 200 OK Content-Type: text/plain

**2. Echo Endpoint: http://localhost:4221/echo/{text} Should Return: **

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;HTTP/1.1 200 OK Content-Type: text/plain Content-Length: 3
{text}

**3. User Agent: http://localhost:4221/user-agent -H "User-Agent: {text} Should Return:**

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;HTTP/1.1 200 OK Content-Type: text/plain Content-Length: 17
{text}

**4. GET Files: http://localhost:4221/files/{file name}**

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;When the program is run with a --directory flag which specifies where the should be stored i.e

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;dotnet run --directory .\tmp\ (directory provided)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;**If file exists:**

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;HTTP/1.1 200 OK Content-Type: application/octet-stream Content-Length: 17 {text in file}

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;**If file does not exist:**

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;HTTP/1.1 404 Not Found

 





