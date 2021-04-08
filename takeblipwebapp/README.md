---
languages:
- csharp

products:
- aspnet-core
- azure-cosmos-db
- azure-function-app
- azure-web-app

description: "This chat uses Microsoft Azure Cosmos DB service to store and access user messages from an ASP.NET application."
---

## A Chat as a Serverless Web application (Azure Web App, Azure Function App) development with ASP.NET using Azure Cosmos DB
This chat uses Microsoft Azure Cosmos DB service to store and access user messages from an ASP.NET application.

## About the author
Wanderson Andrade, a Take Blip candidate

## Azure Resources

## Azure Function App
HTTP Triggered function updates de Cosmos DB

## Azure App Service (Web Application)
User Interface comunicate with the Function App

## Azure Cosmos DB
Contain 3 Containers: message, room and user, columns defined as classes below

public class ChatMessage
{
    public string sessionid { get; set; }
    public bool publik { get; set; }
    public string recipient { get; set; }
    public string message { get; set; }
    public DateTime datetime { get; set; }
    public string messageComplete { get; set; }
    public string room { get; set; }
}

public class ChatRoom
{
    public string name { get; set; }
    public DateTime lastupdate { get; set; }
}

public class ChatUser
{
    public string sessionid { get; set; }
    public string nickname { get; set; }
    public DateTime lastupdate { get; set; }
    public bool offline { get; set; }
    public string room { get; set; }
}

public class _TBCPKG
{
    public string sessionid { get; set; }
    public string nickname { get; set; }
    public string message { get; set; }
    public string publik { get; set; }
    public string console { get; set; }
    public string room { get; set; }
}

## Azure Storage
Storage for the Serverless Services

## Chat Help

Take Blip Chat - Commands

/to [user] [message]
send a chat message to a user, example: /to zemaria olá
*check PRV for private message

/rooms
list available rooms

/change [room]
change to room

/newroom [room]
create new room

/exit
logoff




