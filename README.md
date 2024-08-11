[![progress-banner](https://backend.codecrafters.io/progress/http-server/6ea47dee-3ee4-4050-a7aa-df154397b822)](https://app.codecrafters.io/users/codecrafters-bot?r=2qF)

This is a starting point for C# solutions to the
["Build Your Own HTTP server" Challenge](https://app.codecrafters.io/courses/http-server/overview).

[HTTP](https://en.wikipedia.org/wiki/Hypertext_Transfer_Protocol) is the
protocol that powers the web. In this challenge, you'll build a HTTP/1.1 server
that is capable of serving multiple clients.

**Note**: If you're viewing this repo on GitHub, head over to
[codecrafters.io](https://codecrafters.io) to try the challenge.

# Running this program

For this challenge, 4 endpoints are implemented that you can test using curl

1. Default Endpoint : http://localhost:4221/
Should Return:

HTTP/1.1 200 OK
Content-Type: text/plain

2. Echo Endpoint: http://localhost:4221/echo/{text}

Should return text in body of result and provide Content Length of body.

HTTP/1.1 200 OK
Content-Type: text/plain
Content-Length: 3

{text}

3. User Agent: http://localhost:4221/user-agent -H "User-Agent: grape/apple-mango"

Should read the user agent request header and return it in response body:

HTTP/1.1 200 OK
Content-Type: text/plain
Content-Length: 17

grape/apple-mango

4. Get Files: http://localhost:4221/files/{file name}

When the program is run with a --directory flag which specifies where the should be stored i.e

dotnet run --directory .\tmp\

If file exists:

HTTP/1.1 200 OK
Content-Type: application/octet-stream
Content-Length: 17

{text in file}

If file does not exist:

HTTP/1.1 404 Not Found

 





