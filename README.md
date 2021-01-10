# BeardServer_Client
Unity client to connect to a server

Usage:
Create a game object and attach a Client script to it.

In Client.cs - InitializePacketHandlers() all server responses can be set here
ipv4 vs ipv6 can be set in the TCP class, in Client.cs in the Connect method. By default it is ipv4
ClientHandler contains the client code for handling server responses

Server responses can be awaited. In ServerResponse.cs, create a new Response class and pass that response with a send.
When the server returns a response that matches the ResponseIndex of the send, the task returned by SendTransmissionToServer will complete

Data sent to and from the server can be of any form using the Packet class, but this client has been designed with passing JSON messages back and forth


In the project provided with this code there is an example of connecting to a server on localhost
