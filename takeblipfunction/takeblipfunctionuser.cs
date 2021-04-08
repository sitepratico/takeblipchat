using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;

namespace takeblipfunction
{
    public static class takeblipfunctionuser
    {
        [FunctionName("takeblipfunctionuser")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            [CosmosDB(databaseName: "takeblipchatdb", collectionName: "user", ConnectionStringSetting = "CosmosDBConnection", SqlQuery = "SELECT * FROM user WHERE user.offline <> true ORDER BY user.lastupdate DESC OFFSET 0 LIMIT 5")] IEnumerable<ChatUser> users,
            [CosmosDB(databaseName: "takeblipchatdb", collectionName: "message", ConnectionStringSetting = "CosmosDBConnection", SqlQuery = "SELECT * FROM message ORDER BY message.datetime DESC OFFSET 0 LIMIT 15")] IEnumerable<ChatMessage> messages,
            [CosmosDB(databaseName: "takeblipchatdb", collectionName: "room", ConnectionStringSetting = "CosmosDBConnection", SqlQuery = "SELECT * FROM room ORDER BY room.lastupdate DESC OFFSET 0 LIMIT 5")] IEnumerable<ChatRoom> rooms,
            [CosmosDB(databaseName: "takeblipchatdb", collectionName: "user", ConnectionStringSetting = "CosmosDBConnection")] out dynamic newUser,
            [CosmosDB(databaseName: "takeblipchatdb", collectionName: "message", ConnectionStringSetting = "CosmosDBConnection")] out dynamic newMessage,
            [CosmosDB(databaseName: "takeblipchatdb", collectionName: "room", ConnectionStringSetting = "CosmosDBConnection")] out dynamic newRoom,
            //[CosmosDB(databaseName: "takeblipchatdb", collectionName: "user", ConnectionStringSetting = "CosmosDBConnection")] DocumentClient cosmosClient,
            //[CosmosDB(databaseName: "takeblipchatdb", collectionName: "user", ConnectionStringSetting = "CosmosDBConnection", Id = "{Query.sessionid}", PartitionKey = "{Query.sessionid}")] Document cosmosDocument,
            ILogger log)
        {
            List<string> console = new List<string>();

            _TBCPKG TBCPKG = req.Content.ReadAsAsync<_TBCPKG>().Result;

            const string GENERAL_ROOM = "#general";
            List<string> chatRooms = new List<string>();

            string mySessionId = TBCPKG.sessionid;
            string mySessionNickname = TBCPKG.nickname;
            string mySessionRoom = String.IsNullOrEmpty(TBCPKG.room) ? GENERAL_ROOM : TBCPKG.room;
            ChatUser mySelf = users.Where<ChatUser>(x => x.sessionid.Equals(mySessionId)).FirstOrDefault();
            if (String.IsNullOrEmpty(mySessionNickname) && !(mySelf is null)) mySessionNickname = mySelf.nickname;

            bool userDataComplete = !(TBCPKG is null) && !(TBCPKG.sessionid is null) && !(TBCPKG.nickname is null);

            bool userLogging = false;

            string userLogged = null;
            newMessage = null;

            newRoom = null;

            newUser = null;
            ChatUser userAlreadyLogged = users.Where(x => x.sessionid.Equals(TBCPKG.sessionid)).FirstOrDefault();

            // LOGIN USUARIO ENTRA NA SALA

            userLogging = userAlreadyLogged is null && userDataComplete;
            if (userLogging)
            {
                newUser = new
                {
                    id = TBCPKG.sessionid,
                    sessionid = TBCPKG.sessionid,
                    nickname = TBCPKG.nickname,
                    lastupdate = DateTime.Now,
                    offline = false,
                    room = mySessionRoom
                };
                userLogged = TBCPKG.nickname;
            }

            if (!(userAlreadyLogged is null))
            {
                newUser = new
                {
                    id = userAlreadyLogged.sessionid,
                    sessionid = userAlreadyLogged.sessionid,
                    nickname = userAlreadyLogged.nickname,
                    lastupdate = DateTime.Now,
                    offline = false,
                    room = mySessionRoom
                };
                userLogged = userAlreadyLogged.nickname;
            }

            List<string> nicknames = new List<string>();
            foreach (ChatUser u in users)
            {
                nicknames.Add(u.nickname);

                if (u.sessionid.Equals(TBCPKG.sessionid)) userLogged = u.nickname;
            }

            if (userDataComplete && nicknames.Contains(TBCPKG.nickname))
            {
                var userLoggedData = users.Where(
                    x => x.sessionid.Equals(TBCPKG.sessionid) && x.nickname.Equals(TBCPKG.nickname)
                ).FirstOrDefault();

                if (userLoggedData is null)
                {
                    console.Add("nickname already taken");
                    newUser = null;
                    userLogged = null;
                }
            }

            if (!(newUser is null) && userLogging)
            {
                console.Add($"*** you are registered as <span class='color-link'>{userLogged}</span>. joining <span class='color-link'>{mySessionRoom}</span>.");
                newMessage = new
                {
                    sessionid = Guid.NewGuid().ToString(),
                    publik = true,
                    recipient = String.Empty,
                    message = $"'{userLogged}' has joined {mySessionRoom}",
                    messageComplete = $"'{userLogged}' has joined {mySessionRoom}",
                    datetime = DateTime.Now,
                    room = mySessionRoom
                };
            }

            // MENSAGENS
            if (!String.IsNullOrEmpty(userLogged) && !String.IsNullOrEmpty(TBCPKG.message))
            {
                bool newMessageValid = true;
                string newMessageMessage = TBCPKG.message;
                string newMessageRecipient = String.Empty;

                // MESSAGE TO RECIPIENT
                if (newMessageMessage.Length > 4 && newMessageMessage.ToLower().Trim().Substring(0, 4).Equals("/to "))
                {
                    string[] commandParts = newMessageMessage.Split(" ");

                    // VALID COMMAND
                    if (commandParts.Length > 2)
                    {
                        newMessageRecipient = commandParts[1];
                        newMessageValid = !(users.Where(x => x.nickname.Equals(newMessageRecipient)).FirstOrDefault() is null);

                        if (newMessageValid)
                        {
                            newMessageMessage = newMessageMessage.Substring(newMessageMessage.IndexOf(newMessageRecipient) + newMessageRecipient.Length + 1);
                        }
                        else
                        {
                            newMessageValid = false;
                            newMessageRecipient = String.Empty;
                            newMessage = null;

                            console.Add("invalid command format");
                        }
                    }
                }

                if (newMessageValid)
                {
                    bool flagPublik = TBCPKG.publik is null || !TBCPKG.publik.Equals("no");

                    newMessage = new
                    {
                        sessionid = TBCPKG.sessionid,
                        publik = flagPublik,
                        recipient = newMessageRecipient,
                        message = newMessageMessage,
                        messageComplete = messageFormat(TBCPKG.sessionid, newMessageRecipient, newMessageMessage, flagPublik, users),
                        datetime = DateTime.Now,
                        room = mySessionRoom
                    };

                    // LOGOFF
                    if (TBCPKG.message.ToLower().Trim().Equals("/exit"))
                    {
                        newUser = new
                        {
                            id = userAlreadyLogged.sessionid,
                            sessionid = userAlreadyLogged.sessionid,
                            nickname = userAlreadyLogged.nickname,
                            lastupdate = DateTime.Now,
                            offline = true,
                            room = mySessionRoom
                        };

                        console.Add("*** Disconected. Bye!");
                        newMessage = new
                        {
                            sessionid = Guid.NewGuid().ToString(),
                            publik = true,
                            recipient = String.Empty,
                            message = $"'{userLogged}' has left {mySessionRoom}",
                            messageComplete = $"'{userLogged}' has left {mySessionRoom}",
                            datetime = DateTime.Now,
                            room = mySessionRoom
                        };

                        userLogged = null;
                    }

                    // ROOMS
                    if (TBCPKG.message.ToLower().Trim().Equals("/rooms"))
                    {
                        newMessage = null;

                        string linkTagOpen = $"<a href='#' onclick=\"$('#message').val('/change {GENERAL_ROOM}')\">";
                        string linkTagClose = "</a>";
                        chatRooms.Add($"{linkTagOpen}{GENERAL_ROOM}{linkTagClose}");

                        foreach (ChatRoom r in rooms)
                        {
                            linkTagOpen = $"<a href='#' onclick=\"$('#message').val('/change {r.name}')\">";
                            chatRooms.Add($"{linkTagOpen}{r.name}{linkTagClose}");
                        }

                        console.Add("available rooms: " + String.Join(", ", chatRooms));
                        console.Add(String.Empty);
                    }

                    // NEW ROOM
                    if (TBCPKG.message.Length > 10 && TBCPKG.message.ToLower().Trim().Substring(0, 9).Equals("/newroom "))
                    {
                        newMessage = null;
                        string[] commandParts = TBCPKG.message.Split(" ");

                        // VALID COMMAND
                        if (commandParts.Length.Equals(2))
                        {
                            String newRoomCreate = commandParts[1];
                            bool newRoomExists = !(rooms.Where(x => x.name.Equals(newRoomCreate)).FirstOrDefault() is null);

                            if (!newRoomExists)
                            {
                                newRoomCreate = newRoomCreate.Replace("#", "");
                                newRoomCreate = "#" + newRoomCreate;

                                newRoom = new ChatRoom()
                                {
                                    name = newRoomCreate,
                                    lastupdate = DateTime.Now
                                };

                                newMessage = new
                                {
                                    sessionid = Guid.NewGuid().ToString(),
                                    publik = true,
                                    recipient = String.Empty,
                                    message = $"'{userLogged}' created a new room {newRoomCreate}",
                                    messageComplete = $"'{userLogged}' created a new room {newRoomCreate}",
                                    datetime = DateTime.Now,
                                    room = mySessionRoom
                                };
                            }
                            else
                            {
                                newRoom = null;
                                console.Add("room already exists");
                                console.Add(String.Empty);
                            }
                        }
                    }

                    // CHANGE ROOM
                    if (TBCPKG.message.Length > 9 && TBCPKG.message.ToLower().Trim().Substring(0, 8).Equals("/change "))
                    {
                        newMessage = null;
                        string[] commandParts = TBCPKG.message.Split(" ");

                        // VALID COMMAND
                        if (commandParts.Length.Equals(2))
                        {
                            String newRoomChange = commandParts[1];
                            bool newRoomExists = !(rooms.Where(x => x.name.Equals(newRoomChange)).FirstOrDefault() is null) || newRoomChange.Equals(GENERAL_ROOM);

                            if (newRoomExists)
                            {
                                newMessage = new
                                {
                                    sessionid = Guid.NewGuid().ToString(),
                                    publik = true,
                                    recipient = String.Empty,
                                    message = $"'{userLogged}' has left {mySessionRoom} and joined {newRoomChange}",
                                    messageComplete = $"'{userLogged}' has left {mySessionRoom} and joined {newRoomChange}",
                                    datetime = DateTime.Now,
                                    room = mySessionRoom
                                };

                                TBCPKG.room = newRoomChange;
                                mySessionRoom = newRoomChange;
                            }
                            else
                            {
                                console.Add("room does not exists, create it first");
                                console.Add(String.Empty);
                            }
                        }
                    }
                }
            }

            // USUARIOS LOGADOS NO CHAT
            if (!String.IsNullOrEmpty(userLogged))
            {
                List<string> nicknamesLinks = new List<string>();
                foreach (ChatUser u in users)
                {
                    if (u.room.Equals(mySessionRoom))
                    {
                        string linkTagOpen = $"<a href='#' onclick=\"$('#message').val('/to {u.nickname} ?')\">";
                        string linkTagClose = "</a>";
                        nicknamesLinks.Add($"{linkTagOpen}{u.nickname}{linkTagClose}");
                    }
                }

                string tempChatUsers = "";
                if (nicknamesLinks.Count() > 0) tempChatUsers = "chat users: " + String.Join(", ", nicknamesLinks.ToArray());
                console.Add($"logged as <span class='color-link'>{mySessionNickname}</span> in <span class='color-link'>{mySessionRoom}</span>, {tempChatUsers}");
                console.Add(String.Empty);
            }

            if (!String.IsNullOrEmpty(userLogged))
            {
                if (!(newMessage is null))
                {
                    ChatMessage msg = new ChatMessage()
                    {
                        sessionid = newMessage.sessionid,
                        publik = newMessage.publik,
                        recipient = newMessage.recipient,
                        message = newMessage.message,
                        messageComplete = messageFormat(newMessage.sessionid, newMessage.recipient, newMessage.message, newMessage.publik, users),
                        datetime = newMessage.datetime,
                        room = mySessionRoom
                    };
                    console.Add(msg.messageComplete);
                }

                foreach (ChatMessage m in messages)
                {
                    // WHICH MESSAGES CAN SEE 
                    if (m.room.Equals(mySessionRoom))
                    {
                        if (m.publik || m.sessionid.Equals(mySessionId) || m.recipient.Equals(mySessionNickname) || m.sessionid is null || m.recipient is null || String.IsNullOrEmpty(m.sessionid) || String.IsNullOrEmpty(m.recipient))
                        {
                            console.Add(m.messageComplete);
                        }
                    }
                }
            }

            string consoleReturn = String.Join(System.Environment.NewLine, console.ToArray());
            if (String.IsNullOrEmpty(consoleReturn)) consoleReturn = "-";

            TBCPKG = new _TBCPKG()
            {
                sessionid = TBCPKG.sessionid,
                message = null,
                nickname = userLogged,
                console = consoleReturn,
                room = mySessionRoom
            };

            return new OkObjectResult(JsonConvert.SerializeObject(TBCPKG));
        }

        /// <summary>
        /// METODO PARA FORMATACAO DA FRASE COMPLETA DO USUARIO CONTENDO AS INFORMACOES DE REMETENTE E DESTINATARIO
        /// </summary>
        private static string messageFormat(string sessionid, string recipient, string message, bool publik, IEnumerable<ChatUser> users)
        {
            ChatMessage m = new ChatMessage()
            {
                sessionid = sessionid,
                recipient = recipient,
                message = message,
                publik = publik
            };
            return messageFormat(m, users);
        }

        /// <summary>
        /// METODO PARA FORMATACAO DA FRASE COMPLETA DO USUARIO CONTENDO AS INFORMACOES DE REMETENTE E DESTINATARIO
        /// </summary>
        private static string messageFormat(ChatMessage m, IEnumerable<ChatUser> users)
        {
            string msg = String.Empty;

            ChatUser sender = users.Where(x => x.sessionid.Equals(m.sessionid)).FirstOrDefault();
            if (!(sender is null))
            {
                msg = $"{sender.nickname} says";

                if (!String.IsNullOrEmpty(m.recipient))
                {
                    if (!m.publik) msg = msg + " privately";

                    msg = msg + $" to {m.recipient}";
                }

                msg = msg + ": ";
            }

            msg = msg + m.message;

            if (!m.publik) msg = $"<span class='color-private'>{msg}</span>";

            return msg;
        }
    }
}

