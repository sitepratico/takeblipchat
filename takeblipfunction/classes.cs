using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace takeblipfunction
{
    /// <summary>
    /// CLASSE PARA GESTAO DO ARMAZENAMENTO DAS MENSAGENS
    /// </summary>
    public class ChatMessage
    {
        [JsonProperty(PropertyName = "sessionid")]
        public string sessionid { get; set; }

        [JsonProperty(PropertyName = "publik")]
        public bool publik { get; set; }

        [JsonProperty(PropertyName = "recipient")]
        public string recipient { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string message { get; set; }

        [JsonProperty(PropertyName = "datetime")]
        public DateTime datetime { get; set; }

        [JsonProperty(PropertyName = "messageComplete")]
        public string messageComplete { get; set; }

        [JsonProperty(PropertyName = "room")]
        public string room { get; set; }
    }

    /// <summary>
    /// CLASSE PARA GESTAO DO ARMAZENAMENTO DAS SALAS
    /// </summary>
    public class ChatRoom
    {
        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }

        [JsonProperty(PropertyName = "lastupdate")]
        public DateTime lastupdate { get; set; }
    }

    /// <summary>
    /// CLASSE PARA GESTAO DO ARMAZENAMENTO DOS USUARIOS
    /// </summary>
    public class ChatUser
    {
        [JsonProperty(PropertyName = "sessionid")]
        public string sessionid { get; set; }

        [JsonProperty(PropertyName = "nickname")]
        public string nickname { get; set; }

        [JsonProperty(PropertyName = "lastupdate")]
        public DateTime lastupdate { get; set; }

        [JsonProperty(PropertyName = "offline")]
        public bool offline { get; set; }

        [JsonProperty(PropertyName = "room")]
        public string room { get; set; }
    }

    /// <summary>
    /// CLASSE PARA GESTAO DA COMUNICACAO ENTRE O AZURE WEB APP E A FUNCTION APP
    /// </summary>
    public class _TBCPKG
    {
        [JsonProperty(PropertyName = "sessionid")]
        public string sessionid { get; set; }

        [JsonProperty(PropertyName = "nickname")]
        public string nickname { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string message { get; set; }

        [JsonProperty(PropertyName = "publik")]
        public string publik { get; set; }

        [JsonProperty(PropertyName = "console")]
        public string console { get; set; }

        [JsonProperty(PropertyName = "room")]
        public string room { get; set; }
    }
}
