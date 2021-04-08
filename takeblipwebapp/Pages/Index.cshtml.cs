using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using takeblipfunction;

namespace takeblipwebapp.Pages
{
    public class IndexModel : PageModel
    {
        private AppSettings AppSettings { get; set; }

        public IndexModel(IOptions<AppSettings> settings)
        {
            AppSettings = settings.Value;
        }

        public void OnGet()
        {
        }

        public static void SerializeJsonIntoStream(object value, Stream stream)
        {
            using (var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
            {
                using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
                {
                    var js = new JsonSerializer();
                    js.Serialize(jtw, value);
                    jtw.Flush();
                }
            }
        }

        private static HttpContent CreateHttpContent(object content)
        {
            HttpContent httpContent = null;

            if (content != null)
            {
                var ms = new MemoryStream();
                SerializeJsonIntoStream(content, ms);
                ms.Seek(0, SeekOrigin.Begin);
                httpContent = new StreamContent(ms);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return httpContent;
        }

        public async Task<IActionResult> OnPostAsync(string nickname, string message, string publik, string update, string sessionid, string room)
        {
            string SessionID = sessionid;
            if (String.IsNullOrEmpty(SessionID)) SessionID = HttpContext.Session.GetString("SessionID");

            if (String.IsNullOrEmpty(SessionID))
            {
                SessionID = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("SessionID", SessionID);
            }

            string RoomName = room;
            if (String.IsNullOrEmpty(RoomName)) RoomName = HttpContext.Session.GetString("RoomID");

            if (String.IsNullOrEmpty(RoomName))
            {
                RoomName = "#general";
                HttpContext.Session.SetString("RoomID", RoomName);
            }

            var Url = AppSettings.TakeBlipFunctionUrl;
            
            _TBCPKG TBCPKG = new _TBCPKG() {
                 sessionid = SessionID,
                 nickname = nickname,
                 message = message,
                 publik = publik,
                 console = update,
                 room = RoomName
            };

            CancellationToken cancellationToken;


            /// <summary>
            /// EFETUA ENVIO DA COMUNICACAO PARA A AZURE FUNCTION APP
            /// </summary>

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, Url))
                {
                    using (var httpContent = CreateHttpContent(TBCPKG))
                    {
                        request.Content = httpContent;

                        using (var response = await client
                            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                            .ConfigureAwait(false))
                        {
                            var resualtList = response.Content.ReadAsStringAsync().Result;

                            _TBCPKG tbcpkg = JsonConvert.DeserializeObject<_TBCPKG>(resualtList as string);

                            ViewData["tbcpkg"] = tbcpkg;

                            return Page();
                        }
                    }
                }
            }
        }
    }
}
